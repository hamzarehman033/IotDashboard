using System.Text.Json;
using IotDashboard.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Api.Hubs
{
    [Authorize]
    public class CameraStreamHub : Hub
    {
        private readonly IDeviceRepository _deviceRepository;

        public CameraStreamHub(
            IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task StartStream(long deviceId, byte cameraIndex)
        {
            await GetDeviceAsync(deviceId, requirePublishTopic: false, cameraIndex);
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(deviceId));
        }

        public async void Signal(long deviceId, byte cameraIndex, string sessionId, JsonElement payload)
        {
            var device = await GetDeviceAsync(deviceId, requirePublishTopic: true, cameraIndex);
        }

        public Task StopStream(long deviceId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(deviceId));
        }

        public static string GroupName(long deviceId) => $"camera-{deviceId}";

        private async Task<Domain.Entities.Device> GetDeviceAsync(long deviceId, bool requirePublishTopic, byte? cameraIndex = null)
        {
            var device = await _deviceRepository.GetAllAsync().AsNoTracking().FirstOrDefaultAsync(x => x.Id == deviceId) ?? throw new HubException("Device not found");
            return device;
        }
    }
}
