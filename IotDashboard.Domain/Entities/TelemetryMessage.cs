namespace IotDashboard.Domain.Entities
{
    public class TelemetryMessage
    {
        public long Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public string SiteId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public DateTime ReceivedAtUtc { get; set; }
        public string DecodedPayloadJson { get; set; } = "{}";
        public bool? IsCrcValid { get; set; }
        public string? DecodeError { get; set; }
    }
}