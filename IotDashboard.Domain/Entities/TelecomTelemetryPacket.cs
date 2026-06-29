using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Domain.Entities
{
    public class TelecomTelemetryPacket
    {
        // Primary / Identity
        public long Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public string SiteId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public int TenantNumber { get; set; }
        public int SiteNumber { get; set; }
        public int DeviceNumber { get; set; }

        // Packet Metadata
        public DateTimeOffset? EpochTime { get; set; }
        public DateTimeOffset? PortalReceiveTime { get; set; }
        public byte? PacketVersion { get; set; }
        public byte? DeviceType { get; set; }
        public ManufacturerType? Manufacturer { get; set; }
        public ModelType? Model { get; set; }
        public uint? SiteIdHash { get; set; }
        public uint? DeviceIdHash { get; set; }
        public ushort? PacketSequence { get; set; }

        // System Status
        public SystemStatusFlags? SystemStatus { get; set; }
        public byte? ActiveAlarmCount { get; set; }

        // AC / Mains
        public decimal? LineAVoltage { get; set; }
        public decimal? LineBVoltage { get; set; }
        public decimal? LineCVoltage { get; set; }
        public decimal? LineACurrent { get; set; }
        public decimal? LineBCurrent { get; set; }
        public decimal? LineCCurrent { get; set; }
        public decimal? AcFrequency { get; set; }
        public uint? TotalAcInputPowerW { get; set; }
        public uint? TotalAcEnergyWh { get; set; }
        public bool? MainsAvailable { get; set; }
        public bool? MainsFailure { get; set; }

        // DC Load
        public decimal? DcBusVoltage { get; set; }
        public decimal? DcLoadCurrent { get; set; }
        public uint? DcLoadPowerW { get; set; }
        public decimal? DcLoadPercent { get; set; }
        public uint? TotalDcEnergyWh { get; set; }

        // Rectifier
        public byte? RectifierInstalledCount { get; set; }
        public byte? RectifierCommCount { get; set; }
        public decimal? RectifierTotalCurrent { get; set; }
        public uint? RectifierTotalDcPowerW { get; set; }
        public bool? RectifierAcFail { get; set; }
        public bool? RectifierMissing { get; set; }
        public decimal? RectifierMaxTemperature { get; set; }

        // Battery / BMU
        public BatteryStatusType? BatteryStatus { get; set; }
        public decimal? BatteryVoltage { get; set; }
        public decimal? BatteryCurrent { get; set; }
        public byte? BatteryRemainingPercent { get; set; }
        public decimal? BatteryTotalCapacityAh { get; set; }
        public decimal? BatteryRemainingCapacityAh { get; set; }
        public ushort? BatteryBackupTimeMin { get; set; }
        public decimal? BatteryTemperature { get; set; }
        public byte? BatterySoh { get; set; }
        public byte? BmuOnlineCount { get; set; }
        public decimal? BatteryChargeDischargeKw { get; set; }

        // Solar
        public bool? SolarAvailable { get; set; }
        public decimal? SolarVoltage { get; set; }
        public decimal? SolarCurrent { get; set; }
        public uint? SolarPowerW { get; set; }
        public uint? SolarEnergyTodayWh { get; set; }
        public byte? SolarControllerCount { get; set; }
        public byte? SolarControllerCommFail { get; set; }
        public ushort? SolarChargingHours { get; set; }

        // Genset / Fuel
        public bool? GensetAvailable { get; set; }
        public bool? GensetRunning { get; set; }
        public bool? GensetStartFailure { get; set; }
        public GensetControlModeType? GensetControlMode { get; set; }
        public ushort? GensetRunHours { get; set; }
        public ushort? GensetStartCount { get; set; }
        public byte? FuelLevelPercent { get; set; }
        public ushort? FuelVolumeL { get; set; }
        public bool? FuelTheftAlarm { get; set; }
        public bool? FuelLowAlarm { get; set; }

        // Environment / Sensors
        public decimal? AmbientTemperature1 { get; set; }
        public decimal? AmbientTemperature2 { get; set; }
        public decimal? Humidity { get; set; }
        public bool? DoorOpenAlarm { get; set; }
        public bool? SmokeAlarm { get; set; }
        public bool? WaterLeakAlarm { get; set; }
        public bool? MotionAlarm { get; set; }
        public ushort? DigitalInputBitmap { get; set; }
        public ushort? RelayOutputBitmap { get; set; }

        // Alarms
        public ushort? Alarm1Code { get; set; }
        public AlarmLevelType? Alarm1Level { get; set; }
        public ushort? Alarm2Code { get; set; }
        public AlarmLevelType? Alarm2Level { get; set; }
        public ushort? Alarm3Code { get; set; }
        public AlarmLevelType? Alarm3Level { get; set; }
        public uint? AlarmBitmap1 { get; set; }

        // Validation / Metadata
        public ushort? Crc16 { get; set; }
        public bool IsCrcValid { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public string? Error { get; set; }
    }

    [Flags]
    public enum SystemStatusFlags
    {
        Normal = 1 << 0,
        MajorAlarm = 1 << 1,
        CriticalAlarm = 1 << 2,
        Warning = 1 << 3,
        CommsIssue = 1 << 4
    }

    public enum ManufacturerType : byte
    {
        Huawei = 1,
        Vertiv = 2,
        Zte = 3,
        Delta = 4,
        Generic = 5
    }

    public enum ModelType : byte
    {
        Smu03A = 1,
        Smu02C = 2,
        GenericSnmp = 0x10,
        GenericModbus = 0x20
    }

    public enum BatteryStatusType : byte
    {
        Unknown = 0,
        Float = 1,
        Boost = 2,
        Discharge = 3,
        Idle = 4
    }

    public enum GensetControlModeType : byte
    {
        Unknown = 0,
        Auto = 1,
        Manual = 2,
        Disabled = 3
    }

    public enum AlarmLevelType : byte
    {
        Critical = 1,
        Major = 2,
        Minor = 3,
        Warning = 4
    }
}
