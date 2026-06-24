using IotDashboard.Infrastructure.Persistence;
using IotDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IotDashboard.Api.Services
{
    public class LatestDeviceTelemetryStatusDto
    {
        public string TenantId { get; set; } = string.Empty;
        public string SiteId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTime ReceivedAtUtc { get; set; }
        public bool? IsCrcValid { get; set; }
        public string? DecodeError { get; set; }
        public string SummaryPayloadJson { get; set; } = "{}";
    }

    public interface ITelemetryPersistenceService
    {
        Task PersistAsync(string topic, MqttPayloadDecodeResult decodedPayload, DateTime receivedAtUtc, CancellationToken cancellationToken = default);
        Task<List<LatestDeviceTelemetryStatusDto>> GetLatestBySiteAsync(string siteId, CancellationToken cancellationToken = default);
    }

    public class TelemetryPersistenceService : ITelemetryPersistenceService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TelemetryPersistenceService> _logger;

        public TelemetryPersistenceService(
            IServiceScopeFactory scopeFactory,
            ILogger<TelemetryPersistenceService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task PersistAsync(string topic, MqttPayloadDecodeResult decodedPayload, DateTime receivedAtUtc, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();

            var ids = ResolveTelemetryIdentifiers(topic, decodedPayload);

            var fullPayloadJson = JsonSerializer.Serialize(new
            {
                Topic = topic,
                DecodedPayload = decodedPayload.TelemetryPacket,
                FallbackFields = decodedPayload.Fields,
                decodedPayload.IsHexPayload,
                decodedPayload.NormalizedHexPayload,
                decodedPayload.Error
            });

            var summaryPayloadJson = JsonSerializer.Serialize(BuildSummary(decodedPayload));

            var historyRecord = new TelemetryMessage
            {
                TenantId = ids.TenantId,
                SiteId = ids.SiteId,
                DeviceId = ids.DeviceId,
                Topic = topic,
                ReceivedAtUtc = receivedAtUtc,
                DecodedPayloadJson = fullPayloadJson,
                IsCrcValid = decodedPayload.TelemetryPacket?.IsCrcValid,
                DecodeError = decodedPayload.Error
            };

            await dbContext.TelemetryMessages.AddAsync(historyRecord, cancellationToken);

            var latestRecord = await dbContext.DeviceTelemetryLatest
                .FirstOrDefaultAsync(x => x.DeviceId == ids.DeviceId, cancellationToken);

            if (latestRecord == null)
            {
                latestRecord = new DeviceTelemetryLatest
                {
                    TenantId = ids.TenantId,
                    SiteId = ids.SiteId,
                    DeviceId = ids.DeviceId,
                    ReceivedAtUtc = receivedAtUtc,
                    SummaryPayloadJson = summaryPayloadJson,
                    IsCrcValid = decodedPayload.TelemetryPacket?.IsCrcValid,
                    DecodeError = decodedPayload.Error
                };

                await dbContext.DeviceTelemetryLatest.AddAsync(latestRecord, cancellationToken);
            }
            else
            {
                latestRecord.TenantId = ids.TenantId;
                latestRecord.SiteId = ids.SiteId;
                latestRecord.ReceivedAtUtc = receivedAtUtc;
                latestRecord.SummaryPayloadJson = summaryPayloadJson;
                latestRecord.IsCrcValid = decodedPayload.TelemetryPacket?.IsCrcValid;
                latestRecord.DecodeError = decodedPayload.Error;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Telemetry persisted for TenantId {TenantId}, SiteId {SiteId}, DeviceId {DeviceId}",
                ids.TenantId,
                ids.SiteId,
                ids.DeviceId);
        }

        public async Task<List<LatestDeviceTelemetryStatusDto>> GetLatestBySiteAsync(string siteId, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();

            return await dbContext.DeviceTelemetryLatest
                .AsNoTracking()
                .Where(x => x.SiteId == siteId)
                .OrderByDescending(x => x.ReceivedAtUtc)
                .Select(x => new LatestDeviceTelemetryStatusDto
                {
                    TenantId = x.TenantId,
                    SiteId = x.SiteId,
                    DeviceId = x.DeviceId,
                    ReceivedAtUtc = x.ReceivedAtUtc,
                    IsCrcValid = x.IsCrcValid,
                    DecodeError = x.DecodeError,
                    SummaryPayloadJson = x.SummaryPayloadJson
                })
                .ToListAsync(cancellationToken);
        }

        private static object BuildSummary(MqttPayloadDecodeResult decodedPayload)
        {
            var packet = decodedPayload.TelemetryPacket;

            return new
            {
                packet?.EpochTime,
                packet?.ActiveAlarmCount,
                packet?.MainsAvailable,
                packet?.DcBusVoltage,
                packet?.BatteryVoltage,
                packet?.BatteryCurrent,
                packet?.BatteryRemainingPercent,
                packet?.SolarPowerW,
                packet?.GensetRunning,
                packet?.IsCrcValid,
                decodedPayload.Error
            };
        }

        private static (string TenantId, string SiteId, string DeviceId) ResolveTelemetryIdentifiers(string topic, MqttPayloadDecodeResult decodedPayload)
        {
            if (decodedPayload.TelemetryPacket != null)
            {
                return (
                    decodedPayload.TelemetryPacket.TenantId,
                    decodedPayload.TelemetryPacket.SiteId,
                    decodedPayload.TelemetryPacket.DeviceId);
            }

            if (TryParseTopic(topic, out var tenantId, out var siteId, out var deviceId))
            {
                return (tenantId, siteId, deviceId);
            }

            return ("unknown-tenant", "unknown-site", "unknown-device");
        }

        private static bool TryParseTopic(string topic, out string tenantId, out string siteId, out string deviceId)
        {
            tenantId = string.Empty;
            siteId = string.Empty;
            deviceId = string.Empty;

            if (string.IsNullOrWhiteSpace(topic))
            {
                return false;
            }

            var segments = topic.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length != 5)
            {
                return false;
            }

            if (!string.Equals(segments[0], "telecom", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(segments[4], "telemetry", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            tenantId = segments[1];
            siteId = segments[2];
            deviceId = segments[3];
            return true;
        }
    }
}