using IotDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using AiVisionPacketEntity = IotDashboard.Domain.Entities.AiVisionPacket;

namespace IotDashboard.Api.Services
{
    public interface IAiVisionPersistenceService
    {
        Task PersistAsync(
            int deviceNumber,
            string topic,
            AiVisionPacket packet,
            DateTime receivedAtUtc,
            CancellationToken cancellationToken = default);
    }

    public class AiVisionPersistenceService : IAiVisionPersistenceService
    {
        private const byte MessageTypeAlertSnapshot = 1;
        private const byte MessageTypeHeartbeat = 2;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AiVisionPersistenceService> _logger;

        public AiVisionPersistenceService(
            IServiceScopeFactory scopeFactory,
            ILogger<AiVisionPersistenceService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task PersistAsync(
            int deviceNumber,
            string topic,
            AiVisionPacket packet,
            DateTime receivedAtUtc,
            CancellationToken cancellationToken = default)
        {
            if (packet.MessageType != MessageTypeAlertSnapshot && packet.MessageType != MessageTypeHeartbeat)
            {
                _logger.LogWarning(
                    "Skipping AI Vision persist for unknown MessageType {MessageType}. DeviceNumber={DeviceNumber}, Topic={Topic}",
                    packet.MessageType,
                    deviceNumber,
                    topic);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();

            var entity = MapToEntity(deviceNumber, topic, packet, receivedAtUtc);
            await dbContext.AiVisionPackets.AddAsync(entity, cancellationToken);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "AI Vision packet persisted. DeviceNumber={DeviceNumber}, MessageType={MessageType}, PacketSequence={PacketSequence}",
                    deviceNumber,
                    packet.MessageType,
                    packet.PacketSequence);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _logger.LogInformation(
                    "Duplicate AI Vision packet ignored. DeviceNumber={DeviceNumber}, MessageIdHash={MessageIdHash}, PacketSequence={PacketSequence}",
                    deviceNumber,
                    packet.MessageIdHash,
                    packet.PacketSequence);
            }
        }

        private static AiVisionPacketEntity MapToEntity(
            int deviceNumber,
            string topic,
            AiVisionPacket packet,
            DateTime receivedAtUtc)
        {
            return new AiVisionPacketEntity
            {
                DeviceNumber = deviceNumber,
                Topic = topic,
                ReceivedAtUtc = receivedAtUtc,
                PacketSignature = packet.PacketSignature,
                ProtocolVersion = packet.ProtocolVersion,
                MessageType = packet.MessageType,
                HeaderLength = packet.HeaderLength,
                Flags = packet.Flags,
                PacketSequence = packet.PacketSequence,
                TimestampUtc = packet.TimestampUtc,
                SiteIdHash = packet.SiteIdHash,
                EdgeDeviceIdHash = packet.EdgeDeviceIdHash,
                MessageIdHash = packet.MessageIdHash,
                EventIdHash = packet.EventIdHash,
                CameraId = packet.CameraId,
                EventType = packet.EventType,
                Severity = packet.Severity,
                ConfidenceRaw = packet.ConfidenceRaw,
                ActivityZone = packet.ActivityZone,
                ObjectCount = packet.ObjectCount,
                EhsCodeCount = packet.EhsCodeCount,
                EhsCodes = packet.EhsCodes?.ToArray() ?? Array.Empty<byte>(),
                SnapshotReasonCode = packet.SnapshotReasonCode,
                ActiveCameraCount = packet.ActiveCameraCount,
                ConfiguredCameraCount = packet.ConfiguredCameraCount,
                DetectionEnabled = packet.DetectionEnabled,
                SystemStatus = packet.SystemStatus,
                HeartbeatIntervalSec = packet.HeartbeatIntervalSec,
                EdgeUptimeSec = packet.EdgeUptimeSec,
                CpuUsagePercent = packet.CpuUsagePercent,
                RamUsagePercent = packet.RamUsagePercent,
                DiskFreePercent = packet.DiskFreePercent,
                CameraStatusBitmap = packet.CameraStatusBitmap,
                ModelId = packet.ModelId,
                ImageFormat = packet.ImageFormat,
                ImageEncoding = packet.ImageEncoding,
                ImageWidth = packet.ImageWidth,
                ImageHeight = packet.ImageHeight,
                ImageSizeBytes = packet.ImageSizeBytes,
                ImageCrc32 = packet.ImageCrc32,
                HeaderCrc16 = packet.HeaderCrc16,
                IsHeaderCrcValid = packet.IsHeaderCrcValid,
                IsImageCrcValid = packet.IsImageCrcValid,
                ImageBytes = packet.ImageBytes is { Length: > 0 } ? packet.ImageBytes : null
            };
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                if (inner is PostgresException postgres && postgres.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
