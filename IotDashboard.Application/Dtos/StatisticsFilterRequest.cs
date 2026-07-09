using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Application.Dtos
{
    public class DashboardFilterRequest
    {
        public long? RegionId { get; set; }
        public long? SubRegionId { get; set; }
        public long? ZoneId { get; set; }

        public int? Status { get; set; }
        public long? DeviceId { get; set; }

        public TimeRange TimeRange { get; set; }
    }

    public class DashboardSummaryRequest : DashboardFilterRequest
    {
    }


    public enum TimeRange
    {
        OneDay,
        OneWeek,
        OneMonth,
        ThreeMonths
    }

    public enum ReportType
    {
        BatteryStatus,
        SolarStatus,
        GridStatus,
        AlarmStatus,
        EnergyConsumption
    }

    public enum ReportFileFormat
    {
        Excel,
        Json,
        Csv,
        Pdf
    }

    public class ReportDownloadRequest
    {
        public ReportType ReportType { get; set; }
        public ReportFileFormat Format { get; set; } = ReportFileFormat.Excel;

        [Range(1, long.MaxValue)]
        public long? TenantId { get; set; }

        [Range(1, long.MaxValue)]
        public long? DeviceId { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public TimeRange? TimeRange { get; set; }
    }

    public class DashboardSummaryResponse
    {
        public int ActiveDevices { get; set; }
        public int OnlineOnce { get; set; }
        public int OfflineOnce { get; set; }
        public int ActiveAlerts { get; set; }
        public decimal PacketsPerMinute { get; set; }
    }

    public class MetricDto
    {
        public double Current { get; set; }
        public double Previous { get; set; }
        public double Difference { get; set; }
        public double Percentage { get; set; }
    }

    public class StatsResult
    {
        public double TotalSites { get; set; }
        public double OnlineOnce { get; set; }
        public double ActiveAlerts { get; set; }
        public double MessagesPerMinute { get; set; }
    }

    public class EnvironmentStatsResponse
    {
        public decimal AverageTemperature { get; set; }
        public decimal AverageHumidity { get; set; }
        public int AnomaliesCount { get; set; }
    }

    public class EnvironmentStatsRequest
    {
        public long? DeviceId { get; set; } // null => all devices
        public EnvironmentTimeRange TimeRange { get; set; }
    }

    public enum EnvironmentTimeRange
    {
        OneHour,
        TwentyFourHours,
        SevenDays,
        ThirtyDays
    }

    public class HourlyEnvironmentRequest
    {
        public string? DeviceId { get; set; }
    }

    public class HourlyEnvironmentDto
    {
        public DateTime Hour { get; set; }
        public decimal? AvgTemperature { get; set; }
        public decimal? AvgHumidity { get; set; }
    }

    public class TopSensorActivityDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public int Reads { get; set; }
        public decimal AvgTemperature { get; set; }
    }

    public class AnomalyEventDto
    {
        public string Title { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public DateTime EventTime { get; set; }
    }

    public class BatteryStatusReportRequest
    {
        [Range(1, long.MaxValue)]
        public long? TenantId { get; set; }

        [Range(1, long.MaxValue)]
        public long? DeviceId { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public TimeRange? TimeRange { get; set; }
    }

    public class BatteryStatusReportRowDto
    {
        public long DeviceId { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public DateTime DateUtc { get; set; }
        public int PacketsCount { get; set; }
        public decimal? AvgBatteryVoltage { get; set; }
        public decimal? AvgBatteryCurrent { get; set; }
        public decimal? AvgBatteryTemperature { get; set; }
        public decimal? AvgBatteryRemainingPercent { get; set; }
        public decimal? AvgBatterySoh { get; set; }
    }

    public class BatteryStatusReportResponse
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public int TotalRecords { get; set; }
        public List<BatteryStatusReportRowDto> Records { get; set; } = new();
    }

    public class SolarStatusReportRequest
    {
        [Range(1, long.MaxValue)]
        public long? TenantId { get; set; }

        [Range(1, long.MaxValue)]
        public long? DeviceId { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public TimeRange? TimeRange { get; set; }
    }

    public class SolarStatusReportRowDto
    {
        public long DeviceId { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public DateTime DateUtc { get; set; }
        public int PacketsCount { get; set; }
        public decimal? AvgSolarVoltage { get; set; }
        public decimal? AvgSolarCurrent { get; set; }
        public decimal? AvgSolarPowerW { get; set; }
        public decimal? AvgSolarEnergyTodayWh { get; set; }
        public decimal SolarAvailablePercent { get; set; }
    }

    public class SolarStatusReportResponse
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public int TotalRecords { get; set; }
        public List<SolarStatusReportRowDto> Records { get; set; } = new();
    }

    public class GridStatusReportRequest
    {
        [Range(1, long.MaxValue)]
        public long? TenantId { get; set; }

        [Range(1, long.MaxValue)]
        public long? DeviceId { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public TimeRange? TimeRange { get; set; }
    }

    public class GridStatusReportRowDto
    {
        public long DeviceId { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public DateTime DateUtc { get; set; }
        public int PacketsCount { get; set; }
        public decimal? AvgLineAVoltage { get; set; }
        public decimal? AvgLineBVoltage { get; set; }
        public decimal? AvgLineCVoltage { get; set; }
        public decimal? AvgLineACurrent { get; set; }
        public decimal? AvgLineBCurrent { get; set; }
        public decimal? AvgLineCCurrent { get; set; }
        public decimal? AvgAcFrequency { get; set; }
        public decimal? AvgAcInputPowerW { get; set; }
        public decimal GridAvailablePercent { get; set; }
    }

    public class GridStatusReportResponse
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public int TotalRecords { get; set; }
        public List<GridStatusReportRowDto> Records { get; set; } = new();
    }

    public class AlarmStatusReportRequest
    {
        [Range(1, long.MaxValue)]
        public long? TenantId { get; set; }

        [Range(1, long.MaxValue)]
        public long? DeviceId { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public TimeRange? TimeRange { get; set; }
    }

    public class AlarmStatusReportRowDto
    {
        public long DeviceId { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public DateTime DateUtc { get; set; }
        public int PacketsCount { get; set; }
        public decimal? AvgActiveAlarmCount { get; set; }
        public decimal? AvgAlarm1Code { get; set; }
        public decimal? AvgAlarm2Code { get; set; }
        public decimal? AvgAlarm3Code { get; set; }
        public int PacketsWithActiveAlarms { get; set; }
        public int CriticalAlarmPackets { get; set; }
        public int MajorAlarmPackets { get; set; }
        public int MinorAlarmPackets { get; set; }
        public int WarningAlarmPackets { get; set; }
        public int DoorOpenEvents { get; set; }
        public int SmokeEvents { get; set; }
        public int WaterLeakEvents { get; set; }
        public int MotionEvents { get; set; }
    }

    public class AlarmStatusReportResponse
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public int TotalRecords { get; set; }
        public List<AlarmStatusReportRowDto> Records { get; set; } = new();
    }

    public class EnergyConsumptionReportRequest
    {
        [Range(1, long.MaxValue)]
        public long? TenantId { get; set; }

        [Range(1, long.MaxValue)]
        public long? DeviceId { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public TimeRange? TimeRange { get; set; }
    }

    public class EnergyConsumptionReportRowDto
    {
        public long DeviceId { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public DateTime DateUtc { get; set; }
        public int PacketsCount { get; set; }
        public decimal? AvgTotalAcEnergyWh { get; set; }
        public decimal? AvgTotalDcEnergyWh { get; set; }
        public decimal? AvgSolarEnergyTodayWh { get; set; }
        public decimal? AvgAcInputPowerW { get; set; }
        public decimal? AvgDcLoadPowerW { get; set; }
        public decimal? AvgTenantLoadW { get; set; }
    }

    public class EnergyConsumptionReportResponse
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public int TotalRecords { get; set; }
        public List<EnergyConsumptionReportRowDto> Records { get; set; } = new();
    }

    public class WeeklyAlertDto
    {
        public DateTime Date { get; set; }
        public int Alerts { get; set; }
    }

    public class RecentSiteDto
    {
        public long DeviceId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public long RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;

        public long SubRegionId { get; set; }
        public string SubRegionName { get; set; } = string.Empty;

        public long ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;

        public int BatteryQty { get; set; }
        public DateTime InstallationDate { get; set; }

        public string Coordinates { get; set; } = string.Empty;
    }
}
