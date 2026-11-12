using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface IDeviceHandler : IBaseHandler<DeviceVM>
    {
        Response<CameraSettingsVM> GetCameraSettings();
        Task<Response<DeviceVM>> PatchInfrastructureByDeviceIdAsync(long deviceId, DeviceInfrastructurePatchVM model);
        Task<Response<bool>> SubscribeMqttAsync(long deviceId);
        Task<Response<bool>> UnsubscribeMqttAsync(long deviceId);
        Task<Response<bool>> SendCommandAsync(long deviceId, SendDeviceCommandRequest model, CancellationToken cancellationToken = default);
    }
}
