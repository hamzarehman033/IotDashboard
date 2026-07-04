using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface IDeviceHandler : IBaseHandler<DeviceVM>
    {
        Task<Response<DeviceVM>> PatchInfrastructureByDeviceIdAsync(long deviceId, DeviceInfrastructurePatchVM model);
        Task<Response<bool>> SubscribeMqttAsync(long deviceId);
        Task<Response<bool>> UnsubscribeMqttAsync(long deviceId);
    }
}
