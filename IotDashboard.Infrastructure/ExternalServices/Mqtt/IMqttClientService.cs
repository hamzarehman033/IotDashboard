namespace IotDashboard.Infrastructure.ExternalServices.Mqtt
{
    /// <summary>
    /// Interface for MQTT client service to handle device connections and message subscriptions
    /// </summary>
    public interface IMqttClientService
    {
        /// <summary>
        /// Connect to MQTT broker for a specific device
        /// </summary>
        /// <param name="deviceId">Device identifier</param>
        /// <param name="host">MQTT broker host</param>
        /// <param name="port">MQTT broker port (default 1883)</param>
        /// <param name="clientId">MQTT client ID</param>
        /// <param name="username">MQTT username (optional)</param>
        /// <param name="password">MQTT password (optional)</param>
        /// <param name="useTls">Use TLS/SSL for connection</param>
        /// <param name="keepAliveSeconds">Keep alive interval in seconds</param>
        /// <returns>Task representing the async operation</returns>
        Task ConnectAsync(
            int deviceId,
            string host,
            int port,
            string clientId,
            string? username,
            string? password,
            bool useTls = false,
            int keepAliveSeconds = 60);

        /// <summary>
        /// Subscribe to MQTT topics for a device
        /// </summary>
        /// <param name="deviceId">Device identifier</param>
        /// <param name="topics">Topics to subscribe to</param>
        /// <returns>Task representing the async operation</returns>
        Task SubscribeToTopicsAsync(int deviceId, params string[] topics);

        /// <summary>
        /// Unsubscribe from MQTT topics for a device
        /// </summary>
        /// <param name="deviceId">Device identifier</param>
        /// <param name="topics">Topics to unsubscribe from</param>
        /// <returns>Task representing the async operation</returns>
        Task UnsubscribeFromTopicsAsync(int deviceId, params string[] topics);

        /// <summary>
        /// Publish a message to MQTT topic
        /// </summary>
        /// <param name="topic">Topic to publish to</param>
        /// <param name="payload">Message payload</param>
        /// <param name="retainFlag">Retain message on broker</param>
        /// <returns>Task representing the async operation</returns>
        Task PublishAsync(string topic, string payload, bool retainFlag = false);

        /// <summary>
        /// Disconnect device from MQTT broker
        /// </summary>
        /// <param name="deviceId">Device identifier</param>
        /// <returns>Task representing the async operation</returns>
        Task DisconnectAsync(int deviceId);

        /// <summary>
        /// Disconnect all devices
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        Task DisconnectAllAsync();

        /// <summary>
        /// Check if device is connected to MQTT broker
        /// </summary>
        /// <param name="deviceId">Device identifier</param>
        /// <returns>True if connected, false otherwise</returns>
        bool IsConnected(int deviceId);

        /// <summary>
        /// Register a callback for received messages
        /// </summary>
        /// <param name="callback">Callback function to handle received messages</param>
        void RegisterMessageReceivedCallback(Func<MqttMessageReceivedEventArgs, Task> callback);
    }

    /// <summary>
    /// Event args for MQTT message received events
    /// </summary>
    public class MqttMessageReceivedEventArgs
    {
        public int DeviceId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }
}
