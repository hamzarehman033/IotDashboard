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
        // v1.2 extended packet: 425 bytes (0x1A9). Extension CRC at 0x1A7 over 0xA0–0x1A6.
        private const int PacketLength = 425;
        private const int ExtensionCrcOffset = 0x1A7;
        private const int ExtensionCrcCoverageStart = 0xA0;
        private const int ExtensionCrcCoverageEndExclusive = 0x1A7;

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

            if (bytes.Length != PacketLength)
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
                    Error = $"Payload length is {bytes.Length} bytes (expected {PacketLength} bytes)."
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
                    ["PacketVersion"] = packet.PacketVersion ?? 0,
                    ["PayloadByteLength"] = bytes.Length,
                    ["IsCrcValid"] = packet.IsCrcValid,
                    ["IsExtensionCrcValid"] = packet.IsExtensionCrcValid,
                    ["TopicMatchedTelecomPattern"] = topicMatched
                },
                Error = topicMatched
                    ? null
                    : "Topic did not match telecom/{tenant}/{site}/{device}/telemetry. Decoded using default identifiers."
            };
        }

        private static TelecomTelemetryPacket ParseTelecomTelemetry(
            byte[] bytes,
            string tenantId,
            string siteId,
            string topicDeviceId)
        {
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
                SiteIdHash = NullableU32(ReadU32(bytes, 0x0C), 0xFFFFFFFF),
                DeviceIdHash = NullableU32(ReadU32(bytes, 0x10), 0xFFFFFFFF),
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

                Alarm1Code = NullableU16(ReadU16(bytes, 0x89), 0xFFFF),
                Alarm1Level = EnumOrNull<AlarmLevelType>(ReadU8(bytes, 0x8B), 0xFF),
                Alarm2Code = NullableU16(ReadU16(bytes, 0x8C), 0xFFFF),
                Alarm2Level = EnumOrNull<AlarmLevelType>(ReadU8(bytes, 0x8E), 0xFF),
                Alarm3Code = NullableU16(ReadU16(bytes, 0x8F), 0xFFFF),
                Alarm3Level = EnumOrNull<AlarmLevelType>(ReadU8(bytes, 0x91), 0xFF),
                AlarmBitmap1 = ReadU32(bytes, 0x92),

                GensetPowerW = NullableU32(ReadU32(bytes, 0xA0), 0xFFFFFFFF),
                Tenant1LoadW = NullableU32(ReadU32(bytes, 0xA4), 0xFFFFFFFF),
                Tenant1CurrentA = ScaleI16Nullable(ReadI16(bytes, 0xA8), 0x7FFF, 10),
                Tenant2LoadW = NullableU32(ReadU32(bytes, 0xAA), 0xFFFFFFFF),
                Tenant2CurrentA = ScaleI16Nullable(ReadI16(bytes, 0xAE), 0x7FFF, 10),
                Tenant3LoadW = NullableU32(ReadU32(bytes, 0xB0), 0xFFFFFFFF),
                Tenant3CurrentA = ScaleI16Nullable(ReadI16(bytes, 0xB4), 0x7FFF, 10),
                Tenant4LoadW = NullableU32(ReadU32(bytes, 0xB6), 0xFFFFFFFF),
                Tenant4CurrentA = ScaleI16Nullable(ReadI16(bytes, 0xBA), 0x7FFF, 10),

                Crc16 = ReadU16(bytes, 0x9E)
            };

            var calculatedCrc = ComputeCrc16Modbus(bytes, 0, 0x9E);
            packet.IsCrcValid = packet.Crc16 == calculatedCrc;

            ApplyV12Fields(packet, bytes);

            packet.ExtensionCrc16 = ReadU16(bytes, ExtensionCrcOffset);
            var extensionCrcLength = ExtensionCrcCoverageEndExclusive - ExtensionCrcCoverageStart;
            var calculatedExtensionCrc = ComputeCrc16Modbus(bytes, ExtensionCrcCoverageStart, extensionCrcLength);
            packet.IsExtensionCrcValid = packet.ExtensionCrc16 == calculatedExtensionCrc;

            return packet;
        }

        private static void ApplyV12Fields(TelecomTelemetryPacket packet, byte[] bytes)
        {
            packet.DeviceUptimeSeconds = NullableU32(ReadU32(bytes, 0xBC), 0xFFFFFFFF);
            packet.SignalStrengthDbm = NullableI16(ReadI16(bytes, 0xC0), 0x7FFF);
            packet.NetworkType = EnumOrNull<NetworkType>(ReadU8(bytes, 0xC2), 0xFF);
            packet.SimStatus = EnumOrNull<SimStatusType>(ReadU8(bytes, 0xC3), 0xFF);
            packet.DataValidityBitmap = NullableU32(ReadU32(bytes, 0xC4), 0xFFFFFFFF);
            packet.LastSuccessfulPollAgeSeconds = NullableU16(ReadU16(bytes, 0xC8), 0xFFFF);
            packet.GatewayCpuUsagePercent = NullableU8(ReadU8(bytes, 0xCA), 0xFF);
            packet.GatewayRamUsagePercent = NullableU8(ReadU8(bytes, 0xCB), 0xFF);
            packet.GatewayTemperature = ScaleI16Nullable(ReadI16(bytes, 0xCC), 0x7FFF, 10);
            packet.ActivePowerSource = EnumOrNull<ActivePowerSourceType>(ReadU8(bytes, 0xCE), 0xFF);
            packet.PowerSourcePriority = NullableU16(ReadU16(bytes, 0xCF), 0xFFFF);
            packet.HybridModeEnabled = NullableBoolFromByte(ReadU8(bytes, 0xD1), 0xFF);

            packet.GensetVoltageL1 = ScaleU16Nullable(ReadU16(bytes, 0xD2), 0xFFFF, 10);
            packet.GensetVoltageL2 = ScaleU16Nullable(ReadU16(bytes, 0xD4), 0xFFFF, 10);
            packet.GensetVoltageL3 = ScaleU16Nullable(ReadU16(bytes, 0xD6), 0xFFFF, 10);
            packet.GensetCurrentL1 = ScaleI16Nullable(ReadI16(bytes, 0xD8), 0x7FFF, 10);
            packet.GensetCurrentL2 = ScaleI16Nullable(ReadI16(bytes, 0xDA), 0x7FFF, 10);
            packet.GensetCurrentL3 = ScaleI16Nullable(ReadI16(bytes, 0xDC), 0x7FFF, 10);
            packet.GensetFrequency = ScaleU16Nullable(ReadU16(bytes, 0xDE), 0xFFFF, 10);
            packet.GensetBatteryVoltage = ScaleU16Nullable(ReadU16(bytes, 0xE0), 0xFFFF, 10);
            packet.GensetFuelConsumptionRateLph = ScaleU16Nullable(ReadU16(bytes, 0xE2), 0xFFFF, 10);
            packet.GensetNextServiceHours = NullableU16(ReadU16(bytes, 0xE4), 0xFFFF);

            packet.FuelTankCapacityL = NullableU16(ReadU16(bytes, 0xE6), 0xFFFF);
            packet.FuelSensorStatus = EnumOrNull<FuelSensorStatusType>(ReadU8(bytes, 0xE8), 0xFF);
            packet.FuelConsumptionRateLph = ScaleU16Nullable(ReadU16(bytes, 0xE9), 0xFFFF, 10);
            packet.FuelRuntimeRemainingMin = NullableU16(ReadU16(bytes, 0xEB), 0xFFFF);

            packet.BatterySoc = NullableU8(ReadU8(bytes, 0xED), 0xFF);
            packet.BatteryCycleCount = NullableU16(ReadU16(bytes, 0xEE), 0xFFFF);
            packet.BatteryTotalDischargeTimes = NullableU16(ReadU16(bytes, 0xF0), 0xFFFF);
            packet.BatteryTotalDischargeEnergyWh = NullableU32(ReadU32(bytes, 0xF2), 0xFFFFFFFF);
            packet.BatteryMaxCellVoltageMv = NullableU16(ReadU16(bytes, 0xF6), 0xFFFF);
            packet.BatteryMinCellVoltageMv = NullableU16(ReadU16(bytes, 0xF8), 0xFFFF);
            packet.BatteryMaxCellTemp = ScaleI16Nullable(ReadI16(bytes, 0xFA), 0x7FFF, 10);
            packet.BatteryStatusExtended = NullableU16(ReadU16(bytes, 0xFC), 0xFFFF);
            packet.BatteryContactorStatus = EnumOrNull<BatteryContactorStatusType>(ReadU8(bytes, 0xFE), 0xFF);

            packet.RectifierFaultCount = NullableU8(ReadU8(bytes, 0xFF), 0xFF);
            packet.RectifierCapacityTotalW = NullableU32(ReadU32(bytes, 0x100), 0xFFFFFFFF);
            packet.RectifierCapacityUsedPercent = ScaleU16Nullable(ReadU16(bytes, 0x104), 0xFFFF, 10);
            packet.RectifierEfficiencyPercent = ScaleU16Nullable(ReadU16(bytes, 0x106), 0xFFFF, 10);
            packet.RectifierRedundancyStatus = EnumOrNull<RectifierRedundancyStatusType>(ReadU8(bytes, 0x108), 0xFF);
            packet.RectifierHighestLoadModulePercent = ScaleU16Nullable(ReadU16(bytes, 0x109), 0xFFFF, 10);

            packet.DcLvd1Status = EnumOrNull<DcLvdStatusType>(ReadU8(bytes, 0x10B), 0xFF);
            packet.DcLvd2Status = EnumOrNull<DcLvdStatusType>(ReadU8(bytes, 0x10C), 0xFF);
            packet.DcFuseAlarmBitmap = NullableU32(ReadU32(bytes, 0x10D), 0xFFFFFFFF);
            packet.DcBranchAlarmBitmap = NullableU32(ReadU32(bytes, 0x111), 0xFFFFFFFF);
            packet.DcCriticalLoadCurrent = ScaleI16Nullable(ReadI16(bytes, 0x115), 0x7FFF, 10);
            packet.DcNonCriticalLoadCurrent = ScaleI16Nullable(ReadI16(bytes, 0x117), 0x7FFF, 10);
            packet.BatteryLvdStatus = EnumOrNull<DcLvdStatusType>(ReadU8(bytes, 0x119), 0xFF);

            packet.SolarTotalEnergyLifetimeWh = NullableU32(ReadU32(bytes, 0x11A), 0xFFFFFFFF);
            packet.SolarControllerFaultCount = NullableU8(ReadU8(bytes, 0x11E), 0xFF);
            packet.SolarBatteryChargeCurrent = ScaleI16Nullable(ReadI16(bytes, 0x11F), 0x7FFF, 10);
            packet.SolarMpptStatus = EnumOrNull<SolarMpptStatusType>(ReadU8(bytes, 0x121), 0xFF);
            packet.SolarDailyPeakPowerW = NullableU32(ReadU32(bytes, 0x122), 0xFFFFFFFF);
            packet.SolarPanelStringAlarmBitmap = NullableU32(ReadU32(bytes, 0x126), 0xFFFFFFFF);

            packet.Rectifier1OutputCurrent = ScaleU16Nullable(ReadU16(bytes, 0x12A), 0xFFFF, 10);
            packet.Rectifier2OutputCurrent = ScaleU16Nullable(ReadU16(bytes, 0x12C), 0xFFFF, 10);
            packet.Rectifier3OutputCurrent = ScaleU16Nullable(ReadU16(bytes, 0x12E), 0xFFFF, 10);
            packet.Rectifier4OutputCurrent = ScaleU16Nullable(ReadU16(bytes, 0x130), 0xFFFF, 10);

            packet.Alarm4Code = NullableU16(ReadU16(bytes, 0x132), 0xFFFF);
            packet.Alarm4Level = EnumOrNull<AlarmLevelType>(ReadU8(bytes, 0x134), 0xFF);
            packet.Alarm5Code = NullableU16(ReadU16(bytes, 0x135), 0xFFFF);
            packet.Alarm5Level = EnumOrNull<AlarmLevelType>(ReadU8(bytes, 0x137), 0xFF);
            packet.Alarm6Code = NullableU16(ReadU16(bytes, 0x138), 0xFFFF);
            packet.Alarm6Level = EnumOrNull<AlarmLevelType>(ReadU8(bytes, 0x13A), 0xFF);

            packet.ExtMainL1Voltage = ScaleU16Nullable(ReadU16(bytes, 0x13B), 0xFFFF, 10);
            packet.ExtMainL2Voltage = ScaleU16Nullable(ReadU16(bytes, 0x13D), 0xFFFF, 10);
            packet.ExtMainL3Voltage = ScaleU16Nullable(ReadU16(bytes, 0x13F), 0xFFFF, 10);
            packet.ExtMainL1Current = ScaleI16Nullable(ReadI16(bytes, 0x141), 0x7FFF, 10);
            packet.ExtMainL2Current = ScaleI16Nullable(ReadI16(bytes, 0x143), 0x7FFF, 10);
            packet.ExtMainL3Current = ScaleI16Nullable(ReadI16(bytes, 0x145), 0x7FFF, 10);
            packet.ExtMainFrequency = ScaleU16Nullable(ReadU16(bytes, 0x147), 0xFFFF, 10);
            packet.ExtMainTotalPowerW = NullableU32(ReadU32(bytes, 0x149), 0xFFFFFFFF);
            packet.ExtMainTotalEnergyWh = NullableU32(ReadU32(bytes, 0x14D), 0xFFFFFFFF);

            packet.ExtGensetL1Voltage = ScaleU16Nullable(ReadU16(bytes, 0x151), 0xFFFF, 10);
            packet.ExtGensetL2Voltage = ScaleU16Nullable(ReadU16(bytes, 0x153), 0xFFFF, 10);
            packet.ExtGensetL3Voltage = ScaleU16Nullable(ReadU16(bytes, 0x155), 0xFFFF, 10);
            packet.ExtGensetL1Current = ScaleI16Nullable(ReadI16(bytes, 0x157), 0x7FFF, 10);
            packet.ExtGensetL2Current = ScaleI16Nullable(ReadI16(bytes, 0x159), 0x7FFF, 10);
            packet.ExtGensetL3Current = ScaleI16Nullable(ReadI16(bytes, 0x15B), 0x7FFF, 10);
            packet.ExtGensetFrequency = ScaleU16Nullable(ReadU16(bytes, 0x15D), 0xFFFF, 10);
            packet.ExtGensetTotalPowerW = NullableU32(ReadU32(bytes, 0x15F), 0xFFFFFFFF);
            packet.ExtGensetTotalEnergyWh = NullableU32(ReadU32(bytes, 0x163), 0xFFFFFFFF);
            // 0x167–0x1A6: future_reserved_buffer (skipped)
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

        private static short? NullableI16(short value, short sentinel)
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

        // CRC-16/MODBUS (poly 0xA001 reflected, init 0xFFFF). Matches PDF example CRCs 0x1DB4 / 0x9A1A.
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
