using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface IAiVisionHandler
    {
        Task<Response<List<AiVisionPacketDetailVM>>> GetHistoryByDeviceAsync(
            int deviceNumber,
            byte? messageType,
            string? timeSpan,
            int limit,
            CancellationToken cancellationToken = default);

        Task<Response<string?>> GetVisionPacketDetails(
            long id,
            CancellationToken cancellationToken = default);
    }
}
