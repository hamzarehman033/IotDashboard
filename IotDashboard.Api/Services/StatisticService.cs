using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;
using IotDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Api.Services
{
    public interface IStatisticService
    {
        Task<DashboardSummaryResponse> GetSummary(
                DashboardSummaryRequest request);
        Task<EnvironmentStatsResponse> TelemetryEnvironmentCounts(
    EnvironmentStatsRequest request);
        Task<List<HourlyEnvironmentDto>> TelemetryGetHourlyTempHumidityStats(
            HourlyEnvironmentRequest request);
        Task<List<TopSensorActivityDto>> GetTop5DevicesByActivityInLastHour();
        Task<List<AnomalyEventDto>> GetRecentAnomalies();
    }
    public class StatisticService : IStatisticService
    {
        private readonly AppDBContext _context;

        public StatisticService(AppDBContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<DashboardSummaryResponse> GetSummary(
                DashboardSummaryRequest request)
        {
            var (start, end) = GetRange(request.TimeRange);

            //--------------------------------
            // DEVICE QUERY
            //--------------------------------
            var deviceQuery = _context.Devices.AsQueryable();

            if (request.RegionId.HasValue)
                deviceQuery = deviceQuery.Where(x => x.RegionId == request.RegionId);

            if (request.SubRegionId.HasValue)
                deviceQuery = deviceQuery.Where(x => x.SubRegionId == request.SubRegionId);

            if (request.ZoneId.HasValue)
                deviceQuery = deviceQuery.Where(x => x.ZoneId == request.ZoneId);

            if (request.DeviceId.HasValue)
                deviceQuery = deviceQuery.Where(x => x.Id == request.DeviceId);

            var activeDevices = await deviceQuery.CountAsync();

            var onlineDevices = await deviceQuery
                .CountAsync(x => x.Status == "Online");

            var offlineDevices = await deviceQuery
                .CountAsync(x => x.Status == "Offline");

            //--------------------------------
            // PACKET QUERY
            //--------------------------------
            var packetQuery = _context.TelecomTelemetryPackets
                .Where(x =>
                    x.ReceivedAtUtc >= start &&
                    x.ReceivedAtUtc <= end);

            if (request.RegionId.HasValue)
                packetQuery = packetQuery.Where(x => x.RegionId == request.RegionId);

            if (request.SubRegionId.HasValue)
                packetQuery = packetQuery.Where(x => x.SubRegionId == request.SubRegionId);

            if (request.ZoneId.HasValue)
                packetQuery = packetQuery.Where(x => x.ZoneId == request.ZoneId);

            if (request.DeviceId.HasValue)
            {
                var deviceIdString = request.DeviceId.Value.ToString();
                packetQuery = packetQuery.Where(x => x.DeviceId == deviceIdString);
            }

            var activeAlerts = await packetQuery
                .Where(x => (x.ActiveAlarmCount ?? 0) > 0)
                .Select(x => x.DeviceId)
                .Distinct()
                .CountAsync();

            var totalPackets = await packetQuery.CountAsync();

            var totalMinutes = (decimal)(end - start).TotalMinutes;

            var packetsPerMinute = totalMinutes == 0
                ? 0
                : Math.Round(totalPackets / totalMinutes, 2);

            return new DashboardSummaryResponse
            {
                ActiveDevices = activeDevices,
                OnlineOnce = onlineDevices,
                OfflineOnce = offlineDevices,
                ActiveAlerts = activeAlerts,
                PacketsPerMinute = packetsPerMinute
            };
        }

        private (DateTime Start, DateTime End) GetRange(TimeRange range)
        {
            var end = DateTime.UtcNow;

            return range switch
            {
                TimeRange.OneDay => (end.AddDays(-1), end),
                TimeRange.OneWeek => (end.AddDays(-7), end),
                TimeRange.OneMonth => (end.AddMonths(-1), end),
                TimeRange.ThreeMonths => (end.AddMonths(-3), end),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #region Telemetry

        #region TelemetryEnvironmentCounts
        public async Task<EnvironmentStatsResponse> TelemetryEnvironmentCounts(
    EnvironmentStatsRequest request)
        {
            var startDate = GetStartDate(request.TimeRange);

            var query = _context.TelecomTelemetryPackets
                .Where(x => x.ReceivedAtUtc >= startDate);

            if (request.DeviceId is not null)
            {
                query = query.Where(x => x.DeviceNumber == request.DeviceId);
            }

            var avgTemp = await query
                .AverageAsync(x =>
                    (
                        (x.AmbientTemperature1 ?? 0) +
                        (x.AmbientTemperature2 ?? 0)
                    ) / 2m);

            var avgHumidity = await query
                .AverageAsync(x => x.Humidity ?? 0);

            var anomalies = await query.CountAsync(x =>
                (x.ActiveAlarmCount ?? 0) > 0 ||
                x.DoorOpenAlarm == true ||
                x.SmokeAlarm == true ||
                x.WaterLeakAlarm == true ||
                x.MotionAlarm == true ||
                (x.AmbientTemperature1 ?? 0) > 45 ||
                (x.AmbientTemperature2 ?? 0) > 45 ||
                (x.Humidity ?? 0) > 85
            );

            return new EnvironmentStatsResponse
            {
                AverageTemperature = Math.Round(avgTemp, 2),
                AverageHumidity = Math.Round(avgHumidity, 2),
                AnomaliesCount = anomalies
            };
        }

        private DateTime GetStartDate(EnvironmentTimeRange range)
        {
            var now = DateTime.UtcNow;

            return range switch
            {
                EnvironmentTimeRange.OneHour => now.AddHours(-1),
                EnvironmentTimeRange.TwentyFourHours => now.AddHours(-24),
                EnvironmentTimeRange.SevenDays => now.AddDays(-7),
                EnvironmentTimeRange.ThirtyDays => now.AddDays(-30),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        #endregion TelemetryEnvironmentCounts

        #region Telemetry24HourRecordForHumidityAndTemp

        public async Task<List<HourlyEnvironmentDto>> TelemetryGetHourlyTempHumidityStats(
            HourlyEnvironmentRequest request)
        {
            var utcNow = DateTime.UtcNow;

            // round to current hour
            var endHour = new DateTime(
                utcNow.Year,
                utcNow.Month,
                utcNow.Day,
                utcNow.Hour,
                0,
                0,
                DateTimeKind.Utc);

            var startHour = endHour.AddHours(-23);

            var query = _context.TelecomTelemetryPackets
                .Where(x => x.ReceivedAtUtc >= startHour);

            if (!string.IsNullOrWhiteSpace(request.DeviceId))
            {
                query = query.Where(x => x.DeviceId == request.DeviceId);
            }

            var groupedData = await query
                .GroupBy(x => new
                {
                    x.ReceivedAtUtc.Year,
                    x.ReceivedAtUtc.Month,
                    x.ReceivedAtUtc.Day,
                    x.ReceivedAtUtc.Hour
                })
                .Select(g => new HourlyEnvironmentDto
                {
                    Hour = new DateTime(
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Day,
                        g.Key.Hour,
                        0,
                        0,
                        DateTimeKind.Utc),

                    AvgTemperature = Math.Round(
                        g.Average(x =>
                            (
                                (x.AmbientTemperature1 ?? 0) +
                                (x.AmbientTemperature2 ?? 0)
                            ) / 2m
                        ), 2),

                    AvgHumidity = Math.Round(
                        g.Average(x => x.Humidity ?? 0), 2)
                })
                .OrderBy(x => x.Hour)
                .ToListAsync();

            return FillMissingHours(groupedData, startHour);
        }

        private List<HourlyEnvironmentDto> FillMissingHours(
            List<HourlyEnvironmentDto> existingData,
            DateTime startHour)
        {
            var result = new List<HourlyEnvironmentDto>();

            for (int i = 0; i < 24; i++)
            {
                var currentHour = startHour.AddHours(i);

                var existing = existingData.FirstOrDefault(x =>
                    x.Hour.Year == currentHour.Year &&
                    x.Hour.Month == currentHour.Month &&
                    x.Hour.Day == currentHour.Day &&
                    x.Hour.Hour == currentHour.Hour);

                if (existing != null)
                {
                    result.Add(existing);
                }
                else
                {
                    result.Add(new HourlyEnvironmentDto
                    {
                        Hour = currentHour,
                        AvgTemperature = null,
                        AvgHumidity = null
                    });
                }
            }

            return result;
        }


        #endregion

        #region TelemetryMostActiveDeviceInLastHour
        public async Task<List<TopSensorActivityDto>> GetTop5DevicesByActivityInLastHour()
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            var result = await _context.TelecomTelemetryPackets
                .Where(x => x.ReceivedAtUtc >= oneHourAgo)
                .GroupBy(x => x.DeviceId)
                .Select(g => new TopSensorActivityDto
                {
                    DeviceId = g.Key,
                    Reads = g.Count(),

                    AvgTemperature = Math.Round(
                        g.Average(x =>
                            (
                                (x.AmbientTemperature1 ?? 0m) +
                                (x.AmbientTemperature2 ?? 0m)
                            ) / 2m
                        ), 2)
                })
                .OrderByDescending(x => x.Reads)
                .Take(5)
                .ToListAsync();

            return result;
        }
        #endregion

        #region TelemetryRecentAnomolies

        public async Task<List<AnomalyEventDto>> GetRecentAnomalies()
        {
            var since = DateTime.UtcNow.AddHours(-24);

            var packets = await _context.TelecomTelemetryPackets
                .Where(x => x.ReceivedAtUtc >= since)
                .OrderByDescending(x => x.ReceivedAtUtc)
                .ToListAsync();

            var anomalies = new List<AnomalyEventDto>();

            foreach (var packet in packets)
            {
                string? anomaly = DetectAnomaly(packet);

                if (anomaly == null)
                    continue;

                anomalies.Add(new AnomalyEventDto
                {
                    Title = anomaly,
                    DeviceId = packet.DeviceId,
                    EventTime = packet.ReceivedAtUtc,
                    TimeAgo = GetTimeAgo(packet.ReceivedAtUtc)
                });
            }

            return anomalies
                .OrderByDescending(x => x.EventTime)
                .Take(5)
                .ToList();
        }

        private string? DetectAnomaly(TelecomTelemetryPacket packet)
        {
            var avgTemp = GetAvgTemperature(packet);

            if (avgTemp > 45)
                return $"Temperature spike +{Math.Round(avgTemp - 45, 1)}°C";

            if ((packet.Humidity ?? 0) < 25)
                return $"Humidity drop {packet.Humidity}%";

            if (packet.SmokeAlarm == true)
                return "Smoke detected";

            if (packet.WaterLeakAlarm == true)
                return "Water leak detected";

            if (packet.DoorOpenAlarm == true)
                return "Door opened";

            if (packet.MotionAlarm == true)
                return "Motion detected";

            if ((packet.ActiveAlarmCount ?? 0) > 0)
                return $"{packet.ActiveAlarmCount} active alarms";

            return null;
        }

        private decimal GetAvgTemperature(TelecomTelemetryPacket packet)
        {
            var t1 = packet.AmbientTemperature1;
            var t2 = packet.AmbientTemperature2;

            if (t1.HasValue && t2.HasValue)
                return (t1.Value + t2.Value) / 2m;

            return t1 ?? t2 ?? 0;
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var diff = DateTime.UtcNow - dateTime;

            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} min ago";

            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";

            return $"{(int)diff.TotalDays}d ago";
        }

        #endregion

        #endregion Telemetry
    }
}
