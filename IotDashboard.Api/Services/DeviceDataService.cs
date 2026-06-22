using IotDashboard.Infrastructure.ExternalServices.Mqtt;
using IotDashboard.Application.Util;
using IotDashboard.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace IotDashboard.Api.Services
{
    /// <summary>
    /// Service to bridge MQTT messages and SignalR real-time notifications
    /// </summary>
    public interface IDeviceDataService
    {
        /// <summary>
        /// Initialize the service with cache and SignalR hub context
        /// </summary>
        Task InitializeAsync();
    }

    public class DeviceDataService : IDeviceDataService
    {
        private readonly IMqttClientService _mqttClientService;
        private readonly IDeviceDataCache _deviceDataCache;
        private readonly IHubContext<DeviceDataHub> _hubContext;
        private readonly ILogger<DeviceDataService> _logger;

        public DeviceDataService(
            IMqttClientService mqttClientService,
            IDeviceDataCache deviceDataCache,
            IHubContext<DeviceDataHub> hubContext,
            ILogger<DeviceDataService> logger)
        {
            _mqttClientService = mqttClientService;
            _deviceDataCache = deviceDataCache;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            // Register callback with MQTT service to handle incoming messages
            _mqttClientService.RegisterMessageReceivedCallback(async (eventArgs) =>
            {
                try
                {
                    // Store in cache
                    _deviceDataCache.SetDeviceData(eventArgs.DeviceId, eventArgs.Topic, eventArgs.Payload);

                    // Broadcast to all connected clients on device group
                    var groupName = $"device-{eventArgs.DeviceId}";
                    await _hubContext.Clients.Group(groupName).SendAsync(
                        "DeviceDataReceived",
                        new
                        {
                            DeviceId = eventArgs.DeviceId,
                            Topic = eventArgs.Topic,
                            Payload = eventArgs.Payload,
                            ReceivedAt = eventArgs.ReceivedAt
                        });

                    _logger.LogDebug(
                        $"Broadcasted device {eventArgs.DeviceId} data to SignalR clients on topic {eventArgs.Topic}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing device data for device {eventArgs.DeviceId}");
                }
            });

            _logger.LogInformation("DeviceDataService initialized with MQTT and SignalR integration");
            await Task.CompletedTask;
        }
    }
}
