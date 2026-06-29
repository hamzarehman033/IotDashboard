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

        public string? Status { get; set; }
        public string? DeviceId { get; set; }
        public string? Type { get; set; }

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
}
