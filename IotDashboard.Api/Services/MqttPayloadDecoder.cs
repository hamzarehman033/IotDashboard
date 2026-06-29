using IotDashboard.Domain.Entities;

namespace IotDashboard.Api.Services
{
    public interface IMqttPayloadDecoder
    {
        MqttPayloadDecodeResult Decode(string topic, string payload);
    }

    public class MqttPayloadDecodeResult
    {
        public bool IsHexPayload { get; set; }
        public string? NormalizedHexPayload { get; set; }
        public Dictionary<string, object>? Fields { get; set; }
        public TelecomTelemetryPacket? TelemetryPacket { get; set; }
        public string? Error { get; set; }
    }




    public class MqttPayloadDecoder : IMqttPayloadDecoder
    {
        private const int PacketLength = 160;

        public MqttPayloadDecodeResult Decode(string topic, string payload)
        {
            var normalized = NormalizeHex(payload);
            if (normalized == null)
            {
                return new MqttPayloadDecodeResult
                {
                    IsHexPayload = false,
                    Error = "Payload is not a valid hex payload"
                };
            }

            var bytes = Convert.FromHexString(normalized);

            var topicMatched = TryParseTelecomTopic(topic, out var tenantId, out var siteId, out var topicDeviceId);
            if (!topicMatched && bytes.Length != PacketLength)
            {
                return new MqttPayloadDecodeResult
                {
                    IsHexPayload = true,
                    NormalizedHexPayload = normalized,
                    Fields = new Dictionary<string, object>
                    {
                        ["Topic"] = topic,
                        ["PayloadByteLength"] = bytes.Length,
                        ["ExpectedPacketLength"] = PacketLength
                    },
                    TelemetryPacket = null,
                    Error = $"Topic does not match telecom format and payload length is {bytes.Length} bytes (expected {PacketLength})."
                };
            }

            if (!topicMatched)
            {
                tenantId = "unknown-tenant";
                siteId = "unknown-site";
                topicDeviceId = "unknown-device";
            }

            var packet = ParseTelecomTelemetry(bytes, tenantId, siteId, topicDeviceId);

            return new MqttPayloadDecodeResult
            {
                IsHexPayload = true,
                NormalizedHexPayload = normalized,
                TelemetryPacket = packet,
                Fields = new Dictionary<string, object>
                {
                    ["TenantId"] = packet.TenantId,
                    ["SiteId"] = packet.SiteId,
                    ["DeviceId"] = packet.DeviceId,
                    ["IsCrcValid"] = packet.IsCrcValid,
                    ["TopicMatchedTelecomPattern"] = topicMatched
                },
                Error = topicMatched ? null : "Topic did not match telecom/{tenant}/{site}/{device}/telemetry. Decoded using default identifiers."
            };
        }

        private static TelecomTelemetryPacket ParseTelecomTelemetry(
            byte[] bytes,
            string tenantId,
            string siteId,
            string topicDeviceId)
        {
            if (bytes.Length != PacketLength)
            {
                throw new ArgumentException($"Telemetry payload must be exactly {PacketLength} bytes but got {bytes.Length}.");
            }

            var packet = new TelecomTelemetryPacket
            {
                TenantId = tenantId,
                SiteId = siteId,
                DeviceId = topicDeviceId,

                EpochTime = UnixTimeOrNull(ReadU32(bytes, 0x00), 0xFFFFFFFF),
                PortalReceiveTime = UnixTimeOrNull(ReadU32(bytes, 0x04), 0xFFFFFFFF),
                PacketVersion = ReadU8(bytes, 0x08),
                DeviceType = ReadU8(bytes, 0x09),
                Manufacturer = EnumOrNull<ManufacturerType>(ReadU8(bytes, 0x0A), 0xFF),
                Model = EnumOrNull<ModelType>(ReadU8(bytes, 0x0B), 0xFF),
                SiteIdHash = ReadU32(bytes, 0x0C),
                DeviceIdHash = ReadU32(bytes, 0x10),
                PacketSequence = ReadU16(bytes, 0x14),
                SystemStatus = (SystemStatusFlags)ReadU16(bytes, 0x16),
                ActiveAlarmCount = ReadU8(bytes, 0x18),

                LineAVoltage = ScaleU16Nullable(ReadU16(bytes, 0x19), 0xFFFF, 10),
                LineBVoltage = ScaleU16Nullable(ReadU16(bytes, 0x1B), 0xFFFF, 10),
                LineCVoltage = ScaleU16Nullable(ReadU16(bytes, 0x1D), 0xFFFF, 10),
                LineACurrent = ScaleI16Nullable(ReadI16(bytes, 0x1F), 0x7FFF, 10),
                LineBCurrent = ScaleI16Nullable(ReadI16(bytes, 0x21), 0x7FFF, 10),
                LineCCurrent = ScaleI16Nullable(ReadI16(bytes, 0x23), 0x7FFF, 10),
                AcFrequency = ScaleU16Nullable(ReadU16(bytes, 0x25), 0xFFFF, 10),
                TotalAcInputPowerW = NullableU32(ReadU32(bytes, 0x27), 0xFFFFFFFF),
                TotalAcEnergyWh = NullableU32(ReadU32(bytes, 0x2B), 0xFFFFFFFF),
                MainsAvailable = BoolFromByte(ReadU8(bytes, 0x2F)),
                MainsFailure = BoolFromByte(ReadU8(bytes, 0x30)),

                DcBusVoltage = ScaleU16Nullable(ReadU16(bytes, 0x31), 0xFFFF, 10),
                DcLoadCurrent = ScaleI16Nullable(ReadI16(bytes, 0x33), 0x7FFF, 10),
                DcLoadPowerW = NullableU32(ReadU32(bytes, 0x35), 0xFFFFFFFF),
                DcLoadPercent = ScaleU16Nullable(ReadU16(bytes, 0x39), 0xFFFF, 10),
                TotalDcEnergyWh = NullableU32(ReadU32(bytes, 0x3B), 0xFFFFFFFF),

                RectifierInstalledCount = ReadU8(bytes, 0x3F),
                RectifierCommCount = ReadU8(bytes, 0x40),
                RectifierTotalCurrent = ScaleU16Nullable(ReadU16(bytes, 0x41), 0xFFFF, 10),
                RectifierTotalDcPowerW = NullableU32(ReadU32(bytes, 0x43), 0xFFFFFFFF),
                RectifierAcFail = BoolFromByte(ReadU8(bytes, 0x47)),
                RectifierMissing = BoolFromByte(ReadU8(bytes, 0x48)),
                RectifierMaxTemperature = ScaleI16Nullable(ReadI16(bytes, 0x49), 0x7FFF, 10),

                BatteryStatus = EnumOrNull<BatteryStatusType>(ReadU8(bytes, 0x4B), 0xFF),
                BatteryVoltage = ScaleU16Nullable(ReadU16(bytes, 0x4C), 0xFFFF, 10),
                BatteryCurrent = ScaleI16Nullable(ReadI16(bytes, 0x4E), 0x7FFF, 10),
                BatteryRemainingPercent = NullableU8(ReadU8(bytes, 0x50), 0xFF),
                BatteryTotalCapacityAh = ScaleU16Nullable(ReadU16(bytes, 0x51), 0xFFFF, 10),
                BatteryRemainingCapacityAh = ScaleU16Nullable(ReadU16(bytes, 0x53), 0xFFFF, 10),
                BatteryBackupTimeMin = NullableU16(ReadU16(bytes, 0x55), 0xFFFF),
                BatteryTemperature = ScaleI16Nullable(ReadI16(bytes, 0x57), 0x7FFF, 10),
                BatterySoh = NullableU8(ReadU8(bytes, 0x59), 0xFF),
                BmuOnlineCount = ReadU8(bytes, 0x5A),
                BatteryChargeDischargeKw = ScaleU16Nullable(ReadU16(bytes, 0x5B), 0xFFFF, 100),

                SolarAvailable = NullableBoolFromByte(ReadU8(bytes, 0x5D), 0xFF),
                SolarVoltage = ScaleU16Nullable(ReadU16(bytes, 0x5E), 0xFFFF, 10),
                SolarCurrent = ScaleI16Nullable(ReadI16(bytes, 0x60), 0x7FFF, 10),
                SolarPowerW = NullableU32(ReadU32(bytes, 0x62), 0xFFFFFFFF),
                SolarEnergyTodayWh = NullableU32(ReadU32(bytes, 0x66), 0xFFFFFFFF),
                SolarControllerCount = NullableU8(ReadU8(bytes, 0x6A), 0xFF),
                SolarControllerCommFail = NullableU8(ReadU8(bytes, 0x6B), 0xFF),
                SolarChargingHours = NullableU16(ReadU16(bytes, 0x6C), 0xFFFF),

                GensetAvailable = NullableBoolFromByte(ReadU8(bytes, 0x6E), 0xFF),
                GensetRunning = NullableBoolFromByte(ReadU8(bytes, 0x6F), 0xFF),
                GensetStartFailure = BoolFromByte(ReadU8(bytes, 0x70)),
                GensetControlMode = EnumOrNull<GensetControlModeType>(ReadU8(bytes, 0x71), 0xFF),
                GensetRunHours = NullableU16(ReadU16(bytes, 0x72), 0xFFFF),
                GensetStartCount = NullableU16(ReadU16(bytes, 0x74), 0xFFFF),
                FuelLevelPercent = NullableU8(ReadU8(bytes, 0x76), 0xFF),
                FuelVolumeL = NullableU16(ReadU16(bytes, 0x77), 0xFFFF),
                FuelTheftAlarm = NullableBoolFromByte(ReadU8(bytes, 0x79), 0xFF),
                FuelLowAlarm = NullableBoolFromByte(ReadU8(bytes, 0x7A), 0xFF),

                AmbientTemperature1 = ScaleI16Nullable(ReadI16(bytes, 0x7B), 0x7FFF, 10),
                AmbientTemperature2 = ScaleI16Nullable(ReadI16(bytes, 0x7D), 0x7FFF, 10),
                Humidity = ScaleU16Nullable(ReadU16(bytes, 0x7F), 0xFFFF, 10),
                DoorOpenAlarm = NullableBoolFromByte(ReadU8(bytes, 0x81), 0xFF),
                SmokeAlarm = NullableBoolFromByte(ReadU8(bytes, 0x82), 0xFF),
                WaterLeakAlarm = NullableBoolFromByte(ReadU8(bytes, 0x83), 0xFF),
                MotionAlarm = NullableBoolFromByte(ReadU8(bytes, 0x84), 0xFF),
                DigitalInputBitmap = ReadU16(bytes, 0x85),
                RelayOutputBitmap = ReadU16(bytes, 0x87),

                Alarm1Code = ReadU16(bytes, 0x89),
                Alarm1Level = EnumOrNull<AlarmLevelType>(ReadU8(bytes, 0x8B), 0xFF),
                Alarm2Code = ReadU16(bytes, 0x8C),
                Alarm2Level = EnumOrNull<AlarmLevelType>(ReadU8(bytes, 0x8E), 0xFF),
                Alarm3Code = ReadU16(bytes, 0x8F),
                Alarm3Level = EnumOrNull<AlarmLevelType>(ReadU8(bytes, 0x91), 0xFF),
                AlarmBitmap1 = ReadU32(bytes, 0x92),
                Crc16 = ReadU16(bytes, 0x9E)
            };

            var calculatedCrc = ComputeCrc16Arc(bytes, 0, 0x9E);
            packet.IsCrcValid = packet.Crc16 == calculatedCrc;

            return packet;
        }

        private static bool TryParseTelecomTopic(string topic, out string tenantId, out string siteId, out string deviceId)
        {
            tenantId = string.Empty;
            siteId = string.Empty;
            deviceId = string.Empty;

            if (string.IsNullOrWhiteSpace(topic))
            {
                return false;
            }

            var segments = topic.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length != 5)
            {
                return false;
            }

            if (!string.Equals(segments[0], "telecom", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(segments[4], "telemetry", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            tenantId = segments[1];
            siteId = segments[2];
            deviceId = segments[3];
            return true;
        }

        private static DateTimeOffset? UnixTimeOrNull(uint value, uint sentinel)
        {
            if (value == sentinel)
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeSeconds(value);
        }

        private static byte? NullableU8(byte value, byte sentinel)
        {
            return value == sentinel ? null : value;
        }

        private static ushort? NullableU16(ushort value, ushort sentinel)
        {
            return value == sentinel ? null : value;
        }

        private static uint? NullableU32(uint value, uint sentinel)
        {
            return value == sentinel ? null : value;
        }

        private static decimal? ScaleU16Nullable(ushort value, ushort sentinel, decimal scale)
        {
            return value == sentinel ? null : value / scale;
        }

        private static decimal? ScaleI16Nullable(short value, short sentinel, decimal scale)
        {
            return value == sentinel ? null : value / scale;
        }

        private static bool? BoolFromByte(byte value)
        {
            return value != 0;
        }

        private static bool? NullableBoolFromByte(byte value, byte sentinel)
        {
            if (value == sentinel)
            {
                return null;
            }

            return value != 0;
        }

        private static TEnum? EnumOrNull<TEnum>(byte value, byte nullSentinel) where TEnum : struct, Enum
        {
            if (value == nullSentinel)
            {
                return null;
            }

            if (Enum.IsDefined(typeof(TEnum), value))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), value);
            }

            return null;
        }

        private static byte ReadU8(byte[] bytes, int offset)
        {
            return bytes[offset];
        }

        private static ushort ReadU16(byte[] bytes, int offset)
        {
            return (ushort)((bytes[offset] << 8) | bytes[offset + 1]);
        }

        private static short ReadI16(byte[] bytes, int offset)
        {
            return unchecked((short)ReadU16(bytes, offset));
        }

        private static uint ReadU32(byte[] bytes, int offset)
        {
            return ((uint)bytes[offset] << 24)
                | ((uint)bytes[offset + 1] << 16)
                | ((uint)bytes[offset + 2] << 8)
                | bytes[offset + 3];
        }

        private static ushort ComputeCrc16Arc(byte[] data, int offset, int length)
        {
            ushort crc = 0x0000;

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

        private static string? NormalizeHex(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            var compact = payload
                .Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Trim();

            if (compact.Length == 0 || compact.Length % 2 != 0)
            {
                return null;
            }

            foreach (var c in compact)
            {
                if (!Uri.IsHexDigit(c))
                {
                    return null;
                }
            }

            return compact.ToUpperInvariant();
        }
    }
}
