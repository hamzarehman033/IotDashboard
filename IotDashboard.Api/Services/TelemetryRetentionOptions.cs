namespace IotDashboard.Api.Services
{
    public class TelemetryRetentionOptions
    {
        public const string SectionName = "TelemetryRetention";

        public int RetentionMonths { get; set; } = 3;
        public int BatchSize { get; set; } = 5000;
        public int IntervalHours { get; set; } = 24;
    }
}
