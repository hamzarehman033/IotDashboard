using System.Text.Json;
using IotDashboard.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Api.Hubs
{
    [AllowAnonymous]
    public class CameraStreamHub : Hub
    {
        private readonly IDeviceRepository _deviceRepository;

        public CameraStreamHub(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task StartStream(long deviceId, byte cameraIndex)
        {
            await EnsureDeviceAsync(deviceId);
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(deviceId));
        }

        public async Task Signal(long deviceId, byte cameraIndex, string sessionId, JsonElement payload)
        {
            await EnsureDeviceAsync(deviceId);
            await Clients.OthersInGroup(GroupName(deviceId)).SendAsync(
                "CameraSignal",
                new { deviceId, cameraIndex, sessionId, payload });
        }

        public Task StopStream(long deviceId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(deviceId));
        }

        public static string GroupName(long deviceId) => $"camera-{deviceId}";

        private async Task EnsureDeviceAsync(long deviceId)
        {
            var exists = await _deviceRepository
                .GetAllAsync()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(x => x.Id == deviceId && x.IsActive);

            if (!exists)
            {
                throw new HubException("Device not found");
            }
        }
    }
}
