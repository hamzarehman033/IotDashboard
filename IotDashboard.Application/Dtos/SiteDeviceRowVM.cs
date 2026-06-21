namespace IotDashboard.Application.Dtos
{
    public class SiteDeviceRowVM
    {
        public long SiteId { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public string SiteCode { get; set; } = string.Empty;
        public string SiteStatus { get; set; } = string.Empty;
        public long RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;
        public long SubRegionId { get; set; }
        public string SubRegionName { get; set; } = string.Empty;
        public long ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Coordinates { get; set; } = string.Empty;

        public long? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceCode { get; set; }
        public string? DeviceStatus { get; set; }
        public DateTime? DeviceInstallationDate { get; set; }
        public string? MqttHost { get; set; }
        public int? MqttPort { get; set; }
        public string? MqttClientId { get; set; }
        public bool? UseTls { get; set; }
        public int? KeepAliveSeconds { get; set; }
        public string? RmsSubscribeTopic { get; set; }
        public string? AiSubscribeTopic { get; set; }
        public string? RectifierBrand { get; set; }
        public int? RectifierQty { get; set; }
        public string? RectifierCapacity { get; set; }
        public string? BatteryBrand { get; set; }
        public int? BatteryQty { get; set; }
        public string? BatteryCapacity { get; set; }
        public string? SolarBrand { get; set; }
        public int? SolarQty { get; set; }
        public string? SolarCapacity { get; set; }
        public string? GeneratorBrand { get; set; }
        public int? GeneratorQty { get; set; }
        public string? GeneratorCapacity { get; set; }
        public string? RmsSerialNumber { get; set; }
        public string? SimCardNumber { get; set; }
        public int? CamerasInstalledCount { get; set; }
        public bool? AiEhsInstalled { get; set; }
        public bool? AiSecurityInstalled { get; set; }
    }
}
