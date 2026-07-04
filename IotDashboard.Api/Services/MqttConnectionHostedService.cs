using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IotDashboard.Api.Services
{
    /// <summary>
    /// Hosted service lifecycle hook for MQTT; device subscriptions are controlled manually from API endpoints.
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
            _logger.LogInformation("MQTT startup auto-subscription is disabled. Use manual device subscribe/unsubscribe APIs.");

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MQTT Connection Hosted Service stopping...");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var mqttClientService = scope.ServiceProvider.GetRequiredService<IotDashboard.Infrastructure.ExternalServices.Mqtt.IMqttClientService>();
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
