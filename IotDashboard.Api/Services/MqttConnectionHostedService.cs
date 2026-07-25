using IotDashboard.Domain.Entities;
using IotDashboard.Infrastructure.ExternalServices.Mqtt;
using IotDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IotDashboard.Api.Services
{
    /// <summary>
    /// On startup, connects and subscribes every active device that has MQTT config.
    /// Manual subscribe/unsubscribe APIs remain available as overrides.
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
            _logger.LogInformation("MQTT startup: auto-subscribing configured devices...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                var mqttClientService = scope.ServiceProvider.GetRequiredService<IMqttClientService>();

                // No X-Customer-Id at startup — bypass customer query filters.
                var devices = await dbContext.Devices
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .Where(x => !string.IsNullOrWhiteSpace(x.MqttHost))
                    .Where(x => !string.IsNullOrWhiteSpace(x.MqttClientId))
                    .ToListAsync(cancellationToken);

                var subscribed = 0;
                var skipped = 0;
                var failed = 0;

                foreach (var device in devices)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var topics = GetConfiguredTopics(device);
                    if (topics.Count == 0)
                    {
                        skipped++;
                        _logger.LogWarning(
                            "Skipping MQTT subscribe for device {DeviceId} ({DeviceName}): no topics configured",
                            device.Id,
                            device.Name);
                        continue;
                    }

                    try
                    {
                        await mqttClientService.ConnectAsync(
                            (int)device.Id,
                            device.MqttHost,
                            device.MqttPort,
                            device.MqttClientId,
                            device.MqttUsername,
                            device.MqttPassword,
                            device.UseTls,
                            device.KeepAliveSeconds);

                        await mqttClientService.SubscribeToTopicsAsync((int)device.Id, topics.ToArray());
                        subscribed++;

                        _logger.LogInformation(
                            "MQTT subscribed device {DeviceId} ({DeviceName}) to {TopicCount} topic(s)",
                            device.Id,
                            device.Name,
                            topics.Count);
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.LogError(
                            ex,
                            "Failed MQTT subscribe for device {DeviceId} ({DeviceName})",
                            device.Id,
                            device.Name);
                    }
                }

                _logger.LogInformation(
                    "MQTT startup complete. Subscribed={Subscribed}, Skipped={Skipped}, Failed={Failed}, Candidates={Candidates}",
                    subscribed,
                    skipped,
                    failed,
                    devices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT startup auto-subscription failed");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MQTT Connection Hosted Service stopping...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var mqttClientService = scope.ServiceProvider.GetRequiredService<IMqttClientService>();
                await mqttClientService.DisconnectAllAsync();
                _logger.LogInformation("All MQTT devices disconnected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting MQTT devices");
            }
        }

        private static List<string> GetConfiguredTopics(Device device)
        {
            return new[] { device.RmsSubscribeTopic, device.AiSubscribeTopic }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }
    }
}
