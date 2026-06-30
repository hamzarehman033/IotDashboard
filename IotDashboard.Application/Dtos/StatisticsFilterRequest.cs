using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Application.Dtos
{
    public class StatisticsFilterRequest
    {
        public long? RegionId { get; set; }
        public long? SubRegionId { get; set; }
        public long? ZoneId { get; set; }

        public int? Status { get; set; }
        public long? DeviceId { get; set; }

        public TimeRange TimeRange { get; set; }
    }

    public enum TimeRange
    {
        OneDay,
        OneWeek,
        OneMonth,
        ThreeMonths
    }

    public class DashboardStatsResponse
    {
        public MetricDto TotalSites { get; set; } = new();
        public MetricDto OnlineOnce { get; set; } = new();
        public MetricDto ActiveAlerts { get; set; } = new();
        public MetricDto MessagesPerMinute { get; set; } = new();
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
}
