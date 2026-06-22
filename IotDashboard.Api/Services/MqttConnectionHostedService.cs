using IotDashboard.Infrastructure.ExternalServices.Mqtt;
using IotDashboard.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Api.Services
{
    /// <summary>
    /// Hosted service to auto-connect all active devices to their MQTT brokers on app startup
    /// </summary>
    public class MqttConnectionHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MqttConnectionHostedService> _logger;

        public MqttConnectionHostedService(
            IServiceProvider serviceProvider,
            ILogger<MqttConnectionHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MQTT Connection Hosted Service starting...");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                    var mqttClientService = scope.ServiceProvider.GetRequiredService<IMqttClientService>();

                    // Fetch all active devices with MQTT configuration.
                    var devices = dbContext.Devices
                        .IgnoreQueryFilters()
                        .Where(d => d.IsActive && !string.IsNullOrEmpty(d.MqttHost))
                        .ToList();

                    _logger.LogInformation($"Found {devices.Count} active devices with MQTT configuration");

                    foreach (var device in devices)
                    {
                        try
                        {
                            _logger.LogInformation(
                                $"Connecting device {device.Id} ({device.Name}) to MQTT broker {device.MqttHost}:{device.MqttPort}");

                            // Connect to MQTT broker
                            await mqttClientService.ConnectAsync(
                                (int)device.Id,
                                device.MqttHost,
                                device.MqttPort,
                                device.MqttClientId,
                                device.MqttUsername,
                                device.MqttPassword,
                                device.UseTls,
                                device.KeepAliveSeconds);

                            // Subscribe to topics if configured
                            var topics = new List<string>();
                            if (!string.IsNullOrEmpty(device.RmsSubscribeTopic))
                            {
                                topics.Add(device.RmsSubscribeTopic);
                            }
                            if (!string.IsNullOrEmpty(device.AiSubscribeTopic))
                            {
                                topics.Add(device.AiSubscribeTopic);
                            }

                            if (topics.Count > 0)
                            {
                                await mqttClientService.SubscribeToTopicsAsync((int)device.Id, topics.ToArray());
                                _logger.LogInformation(
                                    $"Device {device.Id} subscribed to topics: {string.Join(", ", topics)}");
                            }
                            else
                            {
                                _logger.LogWarning($"Device {device.Id} has no topics configured for subscription");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to connect device {device.Id} ({device.Name}) to MQTT broker");
                            // Continue with next device instead of failing completely
                        }
                    }

                    _logger.LogInformation("MQTT Connection Hosted Service startup completed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MQTT Connection Hosted Service startup");
            }

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MQTT Connection Hosted Service stopping...");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var mqttClientService = scope.ServiceProvider.GetRequiredService<IMqttClientService>();
                    await mqttClientService.DisconnectAllAsync();
                    _logger.LogInformation("All MQTT devices disconnected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting MQTT devices");
            }

            await Task.CompletedTask;
        }
    }
}
