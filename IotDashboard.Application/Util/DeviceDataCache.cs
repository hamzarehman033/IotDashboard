namespace IotDashboard.Application.Util
{
    /// <summary>
    /// In-memory cache for storing latest device data from MQTT
    /// </summary>
    public interface IDeviceDataCache
    {
        /// <summary>
        /// Store latest device reading
        /// </summary>
        void SetDeviceData(int deviceId, string topic, string payload);

        /// <summary>
        /// Get latest reading for device
        /// </summary>
        DeviceDataPoint? GetDeviceData(int deviceId);

        /// <summary>
        /// Get all latest readings
        /// </summary>
        Dictionary<int, DeviceDataPoint> GetAllDeviceData();
    }

    /// <summary>
    /// Represents a single device data point
    /// </summary>
    public class DeviceDataPoint
    {
        public int DeviceId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Thread-safe in-memory cache for device data
    /// </summary>
    public class DeviceDataCache : IDeviceDataCache
    {
        private readonly Dictionary<int, DeviceDataPoint> _cache = new();
        private readonly object _lockObject = new();

        public void SetDeviceData(int deviceId, string topic, string payload)
        {
            lock (_lockObject)
            {
                _cache[deviceId] = new DeviceDataPoint
                {
                    DeviceId = deviceId,
                    Topic = topic,
                    Payload = payload,
                    ReceivedAt = DateTime.UtcNow
                };
            }
        }

        public DeviceDataPoint? GetDeviceData(int deviceId)
        {
            lock (_lockObject)
            {
                return _cache.ContainsKey(deviceId) ? _cache[deviceId] : null;
            }
        }

        public Dictionary<int, DeviceDataPoint> GetAllDeviceData()
        {
            lock (_lockObject)
            {
                return new Dictionary<int, DeviceDataPoint>(_cache);
            }
        }
    }
}
