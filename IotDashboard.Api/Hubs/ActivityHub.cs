using IotDashboard.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Api.Hubs
{
    [AllowAnonymous]
    public class ActivityHub : Hub
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger<ActivityHub> _logger;

        public ActivityHub(IDeviceRepository deviceRepository, ILogger<ActivityHub> logger)
        {
            _deviceRepository = deviceRepository;
            _logger = logger;
        }

        public static string GroupName(long deviceId) => $"activity-device-{deviceId}";

        public override async Task OnConnectedAsync()
        {
            var deviceIdValue = Context.GetHttpContext()?.Request.Query["deviceId"].FirstOrDefault();
            if (long.TryParse(deviceIdValue, out var deviceId) && deviceId > 0)
            {
                await SubscribeToDevice(deviceId);
            }

            _logger.LogInformation("Device connected to ActivityHub: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Device disconnected from ActivityHub: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SubscribeToDevice(long deviceId)
        {
            await EnsureDeviceAsync(deviceId);
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(deviceId));
            _logger.LogInformation(
                "Connection {ConnectionId} subscribed to activity device {DeviceId}",
                Context.ConnectionId,
                deviceId);
            await Clients.Caller.SendAsync("SubscribeConfirmed", new { deviceId });
        }

        public async Task UnsubscribeFromDevice(long deviceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(deviceId));
            _logger.LogInformation(
                "Connection {ConnectionId} unsubscribed from activity device {DeviceId}",
                Context.ConnectionId,
                deviceId);
        }

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
