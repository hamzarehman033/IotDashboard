namespace IotDashboard.Application.Dtos
{
    public class DeviceInfrastructurePatchVM
    {
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