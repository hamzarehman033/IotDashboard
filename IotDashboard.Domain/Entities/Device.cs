namespace IotDashboard.Domain.Entities
{
    public class Device : BaseEntity
    {
        public long CustomerId { get; set; }
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
        public bool UseTls { get; set; } = false;
        public int KeepAliveSeconds { get; set; } = 60;
        public string RmsSubscribeTopic { get; set; } = string.Empty;
        public string AiSubscribeTopic { get; set; } = string.Empty;
        public string PublishTopic { get; set; } = string.Empty;
        public List<string> PowerSources { get; set; } = new();
        public string RectifierBrand { get; set; } = string.Empty;
        public int RectifierQty { get; set; }
        public string RectifierCapacity { get; set; } = string.Empty;
        public string BatteryBrand { get; set; } = string.Empty;
        public int BatteryQty { get; set; }
        public string BatteryCapacity { get; set; } = string.Empty;
        public string SolarBrand { get; set; } = string.Empty;
        public int SolarQty { get; set; }
        public string SolarCapacity { get; set; } = string.Empty;
        public string GeneratorBrand { get; set; } = string.Empty;
        public int GeneratorQty { get; set; }
        public string GeneratorCapacity { get; set; } = string.Empty;
        public string RmsSerialNumber { get; set; } = string.Empty;
        public string SimCardNumber { get; set; } = string.Empty;
        public int CamerasInstalledCount { get; set; }
        public bool AiEhsInstalled { get; set; }
        public bool AiSecurityInstalled { get; set; }

        public Customer Customer { get; set; } = null!;
        public Location Region { get; set; } = null!;
        public Location SubRegion { get; set; } = null!;
        public Location Zone { get; set; } = null!;
        public ICollection<DeviceTenant> DeviceTenants { get; set; } = new List<DeviceTenant>();
    }
}
