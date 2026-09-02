using IotDashboard.Api.Hubs;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using Microsoft.AspNetCore.SignalR;

namespace IotDashboard.Api.Services
{
    public class ActivityDeviceNotifier : IActivityDeviceNotifier
    {
        private readonly IHubContext<ActivityHub> _hubContext;
        private readonly ILogger<ActivityDeviceNotifier> _logger;

        public ActivityDeviceNotifier(IHubContext<ActivityHub> hubContext, ILogger<ActivityDeviceNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task NotifyAddedAsync(ActivityVM activity) =>
            SendAsync("ActivityAdded", activity.DeviceId, activity);

        public Task NotifyUpdatedAsync(ActivityVM activity) =>
            SendAsync("ActivityUpdated", activity.DeviceId, activity);

        public Task NotifyDeletedAsync(long activityId, long deviceId) =>
            SendAsync("ActivityDeleted", deviceId, new { id = activityId, deviceId });

        private async Task SendAsync(string method, long deviceId, object payload)
        {
            try
            {
                await _hubContext.Clients.Group(ActivityHub.GroupName(deviceId)).SendAsync(method, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to push {Method} to device {DeviceId}", method, deviceId);
            }
        }
    }
}
