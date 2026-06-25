namespace IotDashboard.Application.Dtos
{
    public class LatestDeviceTelemetryStatusVM
    {
        public string TenantId { get; set; } = string.Empty;
        public string SiteId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTime ReceivedAtUtc { get; set; }
        public bool? IsCrcValid { get; set; }
        public string? DecodeError { get; set; }
        public string SummaryPayloadJson { get; set; } = "{}";
    }

    public class TelemetryHistoryItemVM
    {
        public long Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public string SiteId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public DateTime ReceivedAtUtc { get; set; }
        public bool? IsCrcValid { get; set; }
        public string? DecodeError { get; set; }
        public string DecodedPayloadJson { get; set; } = "{}";
    }
}