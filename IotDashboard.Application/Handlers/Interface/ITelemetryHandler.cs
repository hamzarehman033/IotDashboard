using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface ITelemetryHandler
    {
        Task<Response<LatestDeviceTelemetryStatusVM>> GetLatestByDeviceAsync(string deviceId, CancellationToken cancellationToken = default);
        Task<Response<List<TelemetryHistoryItemVM>>> GetHistoryByDeviceAsync(
            string deviceId,
            DateTime? fromUtc,
            DateTime? toUtc,
            int limit,
            CancellationToken cancellationToken = default);
    }
}