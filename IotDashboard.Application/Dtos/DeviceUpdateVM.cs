namespace IotDashboard.Application.Dtos
{
    /// <summary>
    /// General device fields updatable via PUT. Infrastructure is excluded.
    /// </summary>
    public class DeviceUpdateVM
    {
        public long RegionId { get; set; }
        public long SubRegionId { get; set; }
        public long ZoneId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string Status { get; set; } = "Active";
        public string Address { get; set; } = string.Empty;
        public string Coordinates { get; set; } = string.Empty;
        public DateTime InstallationDate { get; set; }
        public string MqttHost { get; set; } = string.Empty;
        public int MqttPort { get; set; } = 1883;
        public string MqttClientId { get; set; } = string.Empty;
        public string MqttUsername { get; set; } = string.Empty;
        public string MqttPassword { get; set; } = string.Empty;
        public bool UseTls { get; set; }
        public int KeepAliveSeconds { get; set; } = 60;
        public string RmsSubscribeTopic { get; set; } = string.Empty;
        public string AiSubscribeTopic { get; set; } = string.Empty;
        public string PublishTopic { get; set; } = string.Empty;
        public List<long> TenantIds { get; set; } = new();
    }
}
