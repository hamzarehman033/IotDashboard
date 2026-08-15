namespace IotDashboard.Domain.Entities
{
    public class AiVisionPacket
    {
        public long Id { get; set; }
        public int DeviceNumber { get; set; }
        public string Topic { get; set; } = string.Empty;
        public DateTime ReceivedAtUtc { get; set; }

        public ushort PacketSignature { get; set; }
        public byte ProtocolVersion { get; set; }
        public byte MessageType { get; set; }
        public ushort HeaderLength { get; set; }
        public ushort Flags { get; set; }
        public uint PacketSequence { get; set; }
        public uint TimestampUtc { get; set; }
        public uint SiteIdHash { get; set; }
        public uint EdgeDeviceIdHash { get; set; }
        public uint MessageIdHash { get; set; }
        public uint EventIdHash { get; set; }
        public byte CameraId { get; set; }
        public byte EventType { get; set; }
        public byte Severity { get; set; }
        public ushort ConfidenceRaw { get; set; }
        public byte ActivityZone { get; set; }
        public ushort ObjectCount { get; set; }
        public byte EhsCodeCount { get; set; }
        public byte[] EhsCodes { get; set; } = Array.Empty<byte>();
        public byte SnapshotReasonCode { get; set; }
        public byte ActiveCameraCount { get; set; }
        public byte ConfiguredCameraCount { get; set; }
        public byte DetectionEnabled { get; set; }
        public byte SystemStatus { get; set; }
        public ushort HeartbeatIntervalSec { get; set; }
        public uint EdgeUptimeSec { get; set; }
        public byte CpuUsagePercent { get; set; }
        public byte RamUsagePercent { get; set; }
        public byte DiskFreePercent { get; set; }
        public ushort CameraStatusBitmap { get; set; }
        public byte ModelId { get; set; }
        public byte ImageFormat { get; set; }
        public byte ImageEncoding { get; set; }
        public ushort ImageWidth { get; set; }
        public ushort ImageHeight { get; set; }
        public uint ImageSizeBytes { get; set; }
        public uint ImageCrc32 { get; set; }
        public ushort HeaderCrc16 { get; set; }
        public bool IsHeaderCrcValid { get; set; }
        public bool IsImageCrcValid { get; set; }
        public byte[]? ImageBytes { get; set; }
    }
}
