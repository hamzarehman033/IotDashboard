using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Application.Handlers.Implimentation
{
    public class TelemetryHandler : ITelemetryHandler
    {
        private readonly AppDBContext _dbContext;

        public TelemetryHandler(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<LatestDeviceTelemetryStatusVM>> GetLatestByDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
        {
            var data = await _dbContext.DeviceTelemetryLatest
                .AsNoTracking()
                .Where(x => x.DeviceId == deviceId)
                .Select(x => new LatestDeviceTelemetryStatusVM
                {
                    TenantId = x.TenantId,
                    DeviceId = x.DeviceId,
                    ReceivedAtUtc = x.ReceivedAtUtc,
                    IsCrcValid = x.IsCrcValid,
                    DecodeError = x.DecodeError,
                    SummaryPayloadJson = x.SummaryPayloadJson
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return new Response<LatestDeviceTelemetryStatusVM>
                {
                    Status = "Error",
                    Message = new List<string> { $"No telemetry found for device '{deviceId}'" }
                };
            }

            return new Response<LatestDeviceTelemetryStatusVM>
            {
                Status = "Success",
                Data = data
            };
        }

        public async Task<Response<List<TelemetryHistoryItemVM>>> GetHistoryByDeviceAsync(
            string deviceId,
            DateTime? fromUtc,
            DateTime? toUtc,
            int limit,
            CancellationToken cancellationToken = default)
        {
            var safeLimit = Math.Clamp(limit <= 0 ? 100 : limit, 1, 1000);

            var query = _dbContext.TelemetryMessages
                .AsNoTracking()
                .Where(x => x.DeviceId == deviceId);

            if (fromUtc.HasValue)
            {
                query = query.Where(x => x.ReceivedAtUtc >= fromUtc.Value);
            }

            if (toUtc.HasValue)
            {
                query = query.Where(x => x.ReceivedAtUtc <= toUtc.Value);
            }

            var data = await query
                .OrderByDescending(x => x.ReceivedAtUtc)
                .Take(safeLimit)
                .Select(x => new TelemetryHistoryItemVM
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    DeviceId = x.DeviceId,
                    Topic = x.Topic,
                    ReceivedAtUtc = x.ReceivedAtUtc,
                    IsCrcValid = x.IsCrcValid,
                    DecodeError = x.DecodeError,
                    DecodedPayloadJson = x.DecodedPayloadJson
                })
                .ToListAsync(cancellationToken);

            return new Response<List<TelemetryHistoryItemVM>>
            {
                Status = "Success",
                Data = data
            };
        }
    }
}