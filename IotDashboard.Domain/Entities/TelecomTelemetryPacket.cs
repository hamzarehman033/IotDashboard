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

        // Extended payload (0xA0+)
        public uint? GensetPowerW { get; set; }
        public uint? Tenant1LoadW { get; set; }
        public decimal? Tenant1CurrentA { get; set; }
        public uint? Tenant2LoadW { get; set; }
        public decimal? Tenant2CurrentA { get; set; }
        public uint? Tenant3LoadW { get; set; }
        public decimal? Tenant3CurrentA { get; set; }
        public uint? Tenant4LoadW { get; set; }
        public decimal? Tenant4CurrentA { get; set; }

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

        // Alarms (v1.1 slots 1-3)
        public ushort? Alarm1Code { get; set; }
        public AlarmLevelType? Alarm1Level { get; set; }
        public ushort? Alarm2Code { get; set; }
        public AlarmLevelType? Alarm2Level { get; set; }
        public ushort? Alarm3Code { get; set; }
        public AlarmLevelType? Alarm3Level { get; set; }
        public uint? AlarmBitmap1 { get; set; }


        // Extended payload v1.2 (0xBC+)
        public uint? DeviceUptimeSeconds { get; set; }
        public short? SignalStrengthDbm { get; set; }
        public NetworkType? NetworkType { get; set; }
        public SimStatusType? SimStatus { get; set; }
        public uint? DataValidityBitmap { get; set; }
        public ushort? LastSuccessfulPollAgeSeconds { get; set; }
        public byte? GatewayCpuUsagePercent { get; set; }
        public byte? GatewayRamUsagePercent { get; set; }
        public decimal? GatewayTemperature { get; set; }
        public ActivePowerSourceType? ActivePowerSource { get; set; }
        public ushort? PowerSourcePriority { get; set; }
        public bool? HybridModeEnabled { get; set; }

        public decimal? GensetVoltageL1 { get; set; }
        public decimal? GensetVoltageL2 { get; set; }
        public decimal? GensetVoltageL3 { get; set; }
        public decimal? GensetCurrentL1 { get; set; }
        public decimal? GensetCurrentL2 { get; set; }
        public decimal? GensetCurrentL3 { get; set; }
        public decimal? GensetFrequency { get; set; }
        public decimal? GensetBatteryVoltage { get; set; }
        public decimal? GensetFuelConsumptionRateLph { get; set; }
        public ushort? GensetNextServiceHours { get; set; }

        public ushort? FuelTankCapacityL { get; set; }
        public FuelSensorStatusType? FuelSensorStatus { get; set; }
        public decimal? FuelConsumptionRateLph { get; set; }
        public ushort? FuelRuntimeRemainingMin { get; set; }

        public byte? BatterySoc { get; set; }
        public ushort? BatteryCycleCount { get; set; }
        public ushort? BatteryTotalDischargeTimes { get; set; }
        public uint? BatteryTotalDischargeEnergyWh { get; set; }
        public ushort? BatteryMaxCellVoltageMv { get; set; }
        public ushort? BatteryMinCellVoltageMv { get; set; }
        public decimal? BatteryMaxCellTemp { get; set; }
        public ushort? BatteryStatusExtended { get; set; }
        public BatteryContactorStatusType? BatteryContactorStatus { get; set; }

        public byte? RectifierFaultCount { get; set; }
        public uint? RectifierCapacityTotalW { get; set; }
        public decimal? RectifierCapacityUsedPercent { get; set; }
        public decimal? RectifierEfficiencyPercent { get; set; }
        public RectifierRedundancyStatusType? RectifierRedundancyStatus { get; set; }
        public decimal? RectifierHighestLoadModulePercent { get; set; }

        public DcLvdStatusType? DcLvd1Status { get; set; }
        public DcLvdStatusType? DcLvd2Status { get; set; }
        public uint? DcFuseAlarmBitmap { get; set; }
        public uint? DcBranchAlarmBitmap { get; set; }
        public decimal? DcCriticalLoadCurrent { get; set; }
        public decimal? DcNonCriticalLoadCurrent { get; set; }
        public DcLvdStatusType? BatteryLvdStatus { get; set; }

        public uint? SolarTotalEnergyLifetimeWh { get; set; }
        public byte? SolarControllerFaultCount { get; set; }
        public decimal? SolarBatteryChargeCurrent { get; set; }
        public SolarMpptStatusType? SolarMpptStatus { get; set; }
        public uint? SolarDailyPeakPowerW { get; set; }
        public uint? SolarPanelStringAlarmBitmap { get; set; }

        public decimal? Rectifier1OutputCurrent { get; set; }
        public decimal? Rectifier2OutputCurrent { get; set; }
        public decimal? Rectifier3OutputCurrent { get; set; }
        public decimal? Rectifier4OutputCurrent { get; set; }

        public ushort? Alarm4Code { get; set; }
        public AlarmLevelType? Alarm4Level { get; set; }
        public ushort? Alarm5Code { get; set; }
        public AlarmLevelType? Alarm5Level { get; set; }
        public ushort? Alarm6Code { get; set; }
        public AlarmLevelType? Alarm6Level { get; set; }

        public decimal? ExtMainL1Voltage { get; set; }
        public decimal? ExtMainL2Voltage { get; set; }
        public decimal? ExtMainL3Voltage { get; set; }
        public decimal? ExtMainL1Current { get; set; }
        public decimal? ExtMainL2Current { get; set; }
        public decimal? ExtMainL3Current { get; set; }
        public decimal? ExtMainFrequency { get; set; }
        public uint? ExtMainTotalPowerW { get; set; }
        public uint? ExtMainTotalEnergyWh { get; set; }

        public decimal? ExtGensetL1Voltage { get; set; }
        public decimal? ExtGensetL2Voltage { get; set; }
        public decimal? ExtGensetL3Voltage { get; set; }
        public decimal? ExtGensetL1Current { get; set; }
        public decimal? ExtGensetL2Current { get; set; }
        public decimal? ExtGensetL3Current { get; set; }
        public decimal? ExtGensetFrequency { get; set; }
        public uint? ExtGensetTotalPowerW { get; set; }
        public uint? ExtGensetTotalEnergyWh { get; set; }

        // Validation / Metadata
        public ushort? Crc16 { get; set; }
        public bool IsCrcValid { get; set; }
        public ushort? ExtensionCrc16 { get; set; }
        public bool? IsExtensionCrcValid { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public string? Error { get; set; }

        //Filteration
        public long RegionId { get; set; }
        public long SubRegionId { get; set; }
        public long ZoneId { get; set; }
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
        None = 0,
        Critical = 1,
        Major = 2,
        Minor = 3,
        Warning = 4
    }

    public enum NetworkType : byte
    {
        Unknown = 0,
        Ethernet = 1,
        Network2G = 2,
        Network3G = 3,
        Network4G = 4,
        Network5G = 5,
        WiFi = 6
    }

    public enum SimStatusType : byte
    {
        Unknown = 0,
        Missing = 1,
        Registered = 2,
        Roaming = 3,
        NoService = 4,
        PinLocked = 5
    }

    public enum ActivePowerSourceType : byte
    {
        Unknown = 0,
        Mains = 1,
        Generator = 2,
        Solar = 3,
        Battery = 4,
        Hybrid = 5
    }

    public enum FuelSensorStatusType : byte
    {
        Unknown = 0,
        Normal = 1,
        Disconnected = 2,
        Invalid = 3,
        Stuck = 4
    }

    public enum BatteryContactorStatusType : byte
    {
        Unknown = 0,
        Open = 1,
        Closed = 2,
        Fault = 3
    }

    public enum RectifierRedundancyStatusType : byte
    {
        Unknown = 0,
        NPlus1Available = 1,
        NoRedundancy = 2,
        Overloaded = 3,
        Fault = 4
    }

    public enum DcLvdStatusType : byte
    {
        Unknown = 0,
        Connected = 1,
        Disconnected = 2,
        Fault = 3
    }

    public enum SolarMpptStatusType : byte
    {
        Unknown = 0,
        Normal = 1,
        Fault = 2,
        Limited = 3,
        Offline = 4
    }
}
