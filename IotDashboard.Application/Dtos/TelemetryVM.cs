namespace IotDashboard.Application.Dtos
{
    public class LatestDeviceTelemetryStatusVM
    {
        public string TenantId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTime ReceivedAtUtc { get; set; }
        public bool? IsCrcValid { get; set; }
        public string? DecodeError { get; set; }
        public string SummaryPayloadJson { get; set; } = "{}";

        // Extended payload fields (0xA0+), when present.
        public uint? GensetPowerW { get; set; }
        public uint? Tenant1LoadW { get; set; }
        public decimal? Tenant1CurrentA { get; set; }
        public uint? Tenant2LoadW { get; set; }
        public decimal? Tenant2CurrentA { get; set; }
        public uint? Tenant3LoadW { get; set; }
        public decimal? Tenant3CurrentA { get; set; }
        public uint? Tenant4LoadW { get; set; }
        public decimal? Tenant4CurrentA { get; set; }
    }

    public class TelemetryHistoryItemVM
    {
        public long Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public DateTime ReceivedAtUtc { get; set; }
        public bool? IsCrcValid { get; set; }
        public string? DecodeError { get; set; }
        public string DecodedPayloadJson { get; set; } = "{}";
    }
}