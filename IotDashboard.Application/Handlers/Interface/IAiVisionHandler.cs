using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface IAiVisionHandler
    {
        Task<Response<List<AiVisionPacketDetailVM>>> GetHistoryByDeviceAsync(
            int deviceNumber,
            byte? messageType,
            DateTime? fromUtc,
            DateTime? toUtc,
            int limit,
            CancellationToken cancellationToken = default);
    }
}
