using IotDashboard.Application.Util;
using Microsoft.AspNetCore.SignalR;

namespace IotDashboard.Api.Hubs
{
    /// <summary>
    /// SignalR hub for real-time device data streaming
    /// </summary>
    public class DeviceDataHub : Hub
    {
        private readonly IDeviceDataCache _deviceDataCache;
        private readonly ILogger<DeviceDataHub> _logger;

        public DeviceDataHub(
            IDeviceDataCache deviceDataCache,
            ILogger<DeviceDataHub> logger)
        {
            _deviceDataCache = deviceDataCache;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected to DeviceDataHub: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected from DeviceDataHub: {Context.ConnectionId}");
            if (exception != null)
            {
                _logger.LogError(exception, "Disconnection error");
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client can call this to subscribe to specific device updates
        /// </summary>
        public async Task SubscribeToDevice(int deviceId)
        {
            var groupName = $"device-{deviceId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} subscribed to device {deviceId}");

            var cached = _deviceDataCache.GetDeviceData(deviceId);
            if (cached != null)
            {
                await Clients.Caller.SendAsync(
                    "DeviceDataReceived",
                    new
                    {
                        DeviceId = cached.DeviceId,
                        Topic = cached.Topic,
                        Payload = cached.Payload,
                        DecodedPayload = cached.DecodedPayload,
                        IsHexPayload = cached.IsHexPayload,
                        NormalizedHexPayload = cached.NormalizedHexPayload,
                        DecodingError = cached.DecodingError,
                        ReceivedAt = cached.ReceivedAt
                    });

                _logger.LogInformation(
                    "Sent cached DeviceDataReceived snapshot to {ConnectionId} for device {DeviceId}",
                    Context.ConnectionId,
                    deviceId);
            }

            await Clients.Caller.SendAsync("SubscribeConfirmed", new { DeviceId = deviceId });
        }

        /// <summary>
        /// Client can call this to unsubscribe from specific device updates
        /// </summary>
        public async Task UnsubscribeFromDevice(int deviceId)
        {
            var groupName = $"device-{deviceId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} unsubscribed from device {deviceId}");
        }
    }
}
