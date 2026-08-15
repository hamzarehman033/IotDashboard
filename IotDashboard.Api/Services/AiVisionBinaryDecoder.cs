using System.Buffers.Binary;

namespace IotDashboard.Api.Services
{
    /// <summary>
    /// AI Vision Security &amp; EHS binary protocol v2.0 — 96-byte header + optional JPEG.
    /// </summary>
    public class AiVisionPacket
    {
        public const int HeaderSize = 96;
        public const ushort PacketSignatureValue = 0xA156;

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
        public List<byte> EhsCodes { get; set; } = new();
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
        public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
    }

    public static class AiVisionBinaryDecoder
    {
        public static bool IsAiVisionTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return false;
            }

            // Protocol: telecom/{tenant}/{site}/{device}/ai
            return topic.EndsWith("/ai", StringComparison.OrdinalIgnoreCase)
                || topic.StartsWith("aivision", StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryDecode(byte[] payload, out AiVisionPacket? packet, out string? error)
        {
            packet = null;
            error = null;

            if (payload == null || payload.Length < AiVisionPacket.HeaderSize)
            {
                error = $"AI Vision payload too short. Expected at least {AiVisionPacket.HeaderSize} bytes, got {payload?.Length ?? 0}";
                return false;
            }

            var packetSignature = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(0x00, 2));
            if (packetSignature != AiVisionPacket.PacketSignatureValue)
            {
                error = $"Invalid AI Vision packet signature 0x{packetSignature:X4}, expected 0x{AiVisionPacket.PacketSignatureValue:X4}";
                return false;
            }

            var protocolVersion = payload[0x02];
            if (protocolVersion != 0x01)
            {
                error = $"Unsupported AI Vision protocol_version {protocolVersion}";
                return false;
            }

            var headerLength = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(0x04, 2));
            if (headerLength != AiVisionPacket.HeaderSize)
            {
                error = $"Invalid header_length {headerLength}, expected {AiVisionPacket.HeaderSize}";
                return false;
            }

            var headerCrc16 = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(0x50, 2));
            var calculatedHeaderCrc = ComputeCrc16Modbus(payload, 0, 0x50);
            var isHeaderCrcValid = headerCrc16 == calculatedHeaderCrc;

            var ehsCodeCount = payload[0x28];
            var ehsCodes = new List<byte>();
            for (var i = 0; i < Math.Min(ehsCodeCount, (byte)8); i++)
            {
                var code = payload[0x29 + i];
                if (code != 0xFF)
                {
                    ehsCodes.Add(code);
                }
            }

            var imageSize = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(0x48, 4));
            var imageCrc32 = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(0x4C, 4));
            byte[] imageBytes = Array.Empty<byte>();
            var isImageCrcValid = true;

            if (imageSize > 0)
            {
                var imageStart = headerLength;
                var imageEnd = imageStart + (int)imageSize;
                if (payload.Length < imageEnd)
                {
                    error = $"Image truncated. Declared {imageSize} bytes, available {Math.Max(0, payload.Length - imageStart)}";
                    return false;
                }

                imageBytes = payload.AsSpan(imageStart, (int)imageSize).ToArray();
                var calculatedImageCrc = ComputeCrc32(imageBytes);
                isImageCrcValid = calculatedImageCrc == imageCrc32;
            }

            packet = new AiVisionPacket
            {
                PacketSignature = packetSignature,
                ProtocolVersion = protocolVersion,
                MessageType = payload[0x03],
                HeaderLength = headerLength,
                Flags = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(0x06, 2)),
                PacketSequence = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(0x08, 4)),
                TimestampUtc = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(0x0C, 4)),
                SiteIdHash = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(0x10, 4)),
                EdgeDeviceIdHash = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(0x14, 4)),
                MessageIdHash = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(0x18, 4)),
                EventIdHash = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(0x1C, 4)),
                CameraId = payload[0x20],
                EventType = payload[0x21],
                Severity = payload[0x22],
                ConfidenceRaw = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(0x23, 2)),
                ActivityZone = payload[0x25],
                ObjectCount = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(0x26, 2)),
                EhsCodeCount = ehsCodeCount,
                EhsCodes = ehsCodes,
                SnapshotReasonCode = payload[0x31],
                ActiveCameraCount = payload[0x32],
                ConfiguredCameraCount = payload[0x33],
                DetectionEnabled = payload[0x34],
                SystemStatus = payload[0x35],
                HeartbeatIntervalSec = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(0x36, 2)),
                EdgeUptimeSec = BinaryPrimitives.ReadUInt32BigEndian(payload.AsSpan(0x38, 4)),
                CpuUsagePercent = payload[0x3C],
                RamUsagePercent = payload[0x3D],
                DiskFreePercent = payload[0x3E],
                CameraStatusBitmap = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(0x3F, 2)),
                ModelId = payload[0x41],
                ImageFormat = payload[0x42],
                ImageEncoding = payload[0x43],
                ImageWidth = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(0x44, 2)),
                ImageHeight = BinaryPrimitives.ReadUInt16BigEndian(payload.AsSpan(0x46, 2)),
                ImageSizeBytes = imageSize,
                ImageCrc32 = imageCrc32,
                HeaderCrc16 = headerCrc16,
                IsHeaderCrcValid = isHeaderCrcValid,
                IsImageCrcValid = isImageCrcValid,
                ImageBytes = imageBytes
            };

            if (!isHeaderCrcValid)
            {
                error = $"AI Vision header CRC16 mismatch. Expected 0x{headerCrc16:X4}, calculated 0x{calculatedHeaderCrc:X4}";
                // Still return packet so caller can decide; mark as failed via return false.
                return false;
            }

            if (imageSize > 0 && !isImageCrcValid)
            {
                error = $"AI Vision image CRC32 mismatch. Expected 0x{imageCrc32:X8}";
                return false;
            }

            return true;
        }

        private static ushort ComputeCrc16Modbus(byte[] data, int offset, int length)
        {
            ushort crc = 0xFFFF;

            for (var i = offset; i < offset + length; i++)
            {
                crc ^= data[i];

                for (var bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return crc;
        }

        private static uint ComputeCrc32(byte[] data)
        {
            // IEEE CRC-32 (poly 0xEDB88320 reflected), init 0xFFFFFFFF, xor out 0xFFFFFFFF.
            uint crc = 0xFFFFFFFFu;
            foreach (var b in data)
            {
                crc ^= b;
                for (var i = 0; i < 8; i++)
                {
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
                }
            }

            return ~crc;
        }
    }
}
