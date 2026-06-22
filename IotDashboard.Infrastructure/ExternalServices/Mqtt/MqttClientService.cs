using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Microsoft.Extensions.Logging;

namespace IotDashboard.Infrastructure.ExternalServices.Mqtt
{
    /// <summary>
    /// MQTT client service implementation using MQTTnet
    /// </summary>
    public class MqttClientService : IMqttClientService
    {
        private readonly Dictionary<int, IMqttClient> _deviceClients = new();
        private readonly ILogger<MqttClientService> _logger;
        private Func<MqttMessageReceivedEventArgs, Task>? _messageReceivedCallback;

        public MqttClientService(ILogger<MqttClientService> logger)
        {
            _logger = logger;
        }

        public async Task ConnectAsync(
            int deviceId,
            string host,
            int port,
            string clientId,
            string? username,
            string? password,
            bool useTls = false,
            int keepAliveSeconds = 60)
        {
            try
            {
                // Check if already connected
                if (_deviceClients.ContainsKey(deviceId) && _deviceClients[deviceId].IsConnected)
                {
                    _logger.LogInformation($"Device {deviceId} is already connected to MQTT broker");
                    return;
                }

                // Create MQTT client
                var mqttClient = new MqttFactory().CreateMqttClient();

                // Build connection options
                var optionsBuilder = new MqttClientOptionsBuilder()
                    .WithTcpServer(host, port)
                    .WithClientId(clientId)
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(keepAliveSeconds));

                if (useTls)
                {
                    optionsBuilder.WithTlsOptions(o => o.WithAllowUntrustedCertificates());
                }

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    optionsBuilder.WithCredentials(username, password);
                }

                var options = optionsBuilder.Build();

                // Setup message handler
                mqttClient.DisconnectedAsync += async e =>
                {
                    _logger.LogWarning($"Device {deviceId} disconnected from MQTT broker: {e.Reason}");
                    await Task.CompletedTask;
                };

                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    try
                    {
                        var payload = e.ApplicationMessage.ConvertPayloadToString();
                        var topic = e.ApplicationMessage.Topic;

                        var eventArgs = new MqttMessageReceivedEventArgs
                        {
                            DeviceId = deviceId,
                            Topic = topic,
                            Payload = payload,
                            ReceivedAt = DateTime.UtcNow
                        };

                        if (_messageReceivedCallback != null)
                        {
                            await _messageReceivedCallback(eventArgs);
                        }

                        _logger.LogInformation(
                            $"Message received from device {deviceId} on topic '{topic}': {payload}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing message from device {deviceId}");
                    }

                    await Task.CompletedTask;
                };

                // Connect to MQTT broker
                await mqttClient.ConnectAsync(options, CancellationToken.None);

                // Store client
                if (_deviceClients.ContainsKey(deviceId))
                {
                    // Dispose old client if exists
                    try
                    {
                        await _deviceClients[deviceId].DisconnectAsync();
                        _deviceClients[deviceId].Dispose();
                    }
                    catch { }
                }

                _deviceClients[deviceId] = mqttClient;

                _logger.LogInformation(
                    $"Device {deviceId} successfully connected to MQTT broker at {host}:{port}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to connect device {deviceId} to MQTT broker");
                throw;
            }
        }

        public async Task SubscribeToTopicsAsync(int deviceId, params string[] topics)
        {
            if (topics == null || topics.Length == 0)
            {
                _logger.LogWarning($"No topics provided for device {deviceId} subscription");
                return;
            }

            if (!_deviceClients.ContainsKey(deviceId))
            {
                _logger.LogWarning($"Device {deviceId} client not found. Cannot subscribe to topics");
                return;
            }

            var client = _deviceClients[deviceId];

            if (!client.IsConnected)
            {
                _logger.LogWarning($"Device {deviceId} is not connected. Cannot subscribe to topics");
                return;
            }

            try
            {
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder();

                foreach (var topic in topics)
                {
                    subscribeOptions.WithTopicFilter(f => f.WithTopic(topic).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce));
                }

                await client.SubscribeAsync(subscribeOptions.Build());

                _logger.LogInformation(
                    $"Device {deviceId} subscribed to topics: {string.Join(", ", topics)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to subscribe device {deviceId} to topics: {string.Join(", ", topics)}");
                throw;
            }
        }

        public async Task UnsubscribeFromTopicsAsync(int deviceId, params string[] topics)
        {
            if (topics == null || topics.Length == 0)
            {
                _logger.LogWarning($"No topics provided for device {deviceId} unsubscription");
                return;
            }

            if (!_deviceClients.ContainsKey(deviceId))
            {
                _logger.LogWarning($"Device {deviceId} client not found. Cannot unsubscribe from topics");
                return;
            }

            var client = _deviceClients[deviceId];

            if (!client.IsConnected)
            {
                _logger.LogWarning($"Device {deviceId} is not connected. Cannot unsubscribe from topics");
                return;
            }

            try
            {
                var unsubscribeOptions = new MqttClientUnsubscribeOptionsBuilder();

                foreach (var topic in topics)
                {
                    unsubscribeOptions.WithTopicFilter(topic);
                }

                await client.UnsubscribeAsync(unsubscribeOptions.Build());

                _logger.LogInformation(
                    $"Device {deviceId} unsubscribed from topics: {string.Join(", ", topics)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to unsubscribe device {deviceId} from topics: {string.Join(", ", topics)}");
                throw;
            }
        }

        public async Task PublishAsync(string topic, string payload, bool retainFlag = false)
        {
            try
            {
                if (_deviceClients.Count == 0)
                {
                    _logger.LogWarning("No MQTT clients connected. Cannot publish message");
                    return;
                }

                // Get first connected client for publishing
                var connectedClient = _deviceClients.Values.FirstOrDefault(c => c.IsConnected);

                if (connectedClient == null)
                {
                    _logger.LogWarning("No connected MQTT clients. Cannot publish message");
                    return;
                }

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithRetainFlag(retainFlag)
                    .Build();

                await connectedClient.PublishAsync(message);

                _logger.LogInformation($"Message published to topic '{topic}': {payload}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish message to topic '{topic}'");
                throw;
            }
        }

        public async Task DisconnectAsync(int deviceId)
        {
            if (!_deviceClients.ContainsKey(deviceId))
            {
                _logger.LogWarning($"Device {deviceId} not found in connected clients");
                return;
            }

            try
            {
                var client = _deviceClients[deviceId];

                if (client.IsConnected)
                {
                    await client.DisconnectAsync();
                }

                client.Dispose();
                _deviceClients.Remove(deviceId);

                _logger.LogInformation($"Device {deviceId} disconnected from MQTT broker");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error disconnecting device {deviceId}");
            }
        }

        public async Task DisconnectAllAsync()
        {
            var deviceIds = _deviceClients.Keys.ToList();

            foreach (var deviceId in deviceIds)
            {
                await DisconnectAsync(deviceId);
            }

            _logger.LogInformation("All MQTT devices disconnected");
        }

        public bool IsConnected(int deviceId)
        {
            if (!_deviceClients.ContainsKey(deviceId))
            {
                return false;
            }

            return _deviceClients[deviceId].IsConnected;
        }

        public void RegisterMessageReceivedCallback(Func<MqttMessageReceivedEventArgs, Task> callback)
        {
            _messageReceivedCallback = callback;
            _logger.LogInformation("Message received callback registered");
        }
    }
}
