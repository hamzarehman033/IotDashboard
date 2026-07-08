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
        Task<List<WeeklyAlertDto>> GetWeeklyAlerts();
        Task<List<RecentSiteDto>> GetRecentSites(
      DashboardFilterRequest request);
        Task<EnvironmentStatsResponse> TelemetryEnvironmentCounts(
    EnvironmentStatsRequest request);
        Task<List<HourlyEnvironmentDto>> TelemetryGetHourlyTempHumidityStats(
            HourlyEnvironmentRequest request);
        Task<List<TopSensorActivityDto>> GetTop5DevicesByActivityInLastHour();
        Task<List<AnomalyEventDto>> GetRecentAnomalies();
        Task<BatteryStatusReportResponse> GetBatteryStatusReport(BatteryStatusReportRequest request);
        Task<SolarStatusReportResponse> GetSolarStatusReport(SolarStatusReportRequest request);
        Task<GridStatusReportResponse> GetGridStatusReport(GridStatusReportRequest request);
        Task<AlarmStatusReportResponse> GetAlarmStatusReport(AlarmStatusReportRequest request);
        Task<EnergyConsumptionReportResponse> GetEnergyConsumptionReport(EnergyConsumptionReportRequest request);
    }
    public class StatisticService : IStatisticService
    {
        private readonly AppDBContext _context;

        #region dashboard
        #region dashboard-summary
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
        #endregion

        #region GetWeeklyAlertsCount
        public async Task<List<WeeklyAlertDto>> GetWeeklyAlerts()
        {
            var today = DateTime.UtcNow.Date;
            var startDate = today.AddDays(-6);

            var groupedData = await _context.TelecomTelemetryPackets
                .Where(x =>
                    x.ReceivedAtUtc >= startDate &&
                    (x.ActiveAlarmCount ?? 0) > 0)
                .GroupBy(x => x.ReceivedAtUtc.Date)
                .Select(g => new WeeklyAlertDto
                {
                    Date = g.Key,
                    Alerts = g.Sum(x => (int)(x.ActiveAlarmCount ?? 0))
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return FillMissingDays(groupedData, startDate);
        }

        private List<WeeklyAlertDto> FillMissingDays(
            List<WeeklyAlertDto> existingData,
            DateTime startDate)
        {
            var result = new List<WeeklyAlertDto>();

            for (int i = 0; i < 7; i++)
            {
                var currentDate = startDate.AddDays(i);

                var existing = existingData
                    .FirstOrDefault(x => x.Date.Date == currentDate.Date);

                result.Add(existing ?? new WeeklyAlertDto
                {
                    Date = currentDate,
                    Alerts = 0
                });
            }

            return result;
        }

        #endregion

        #region RecentSites

        public async Task<List<RecentSiteDto>> GetRecentSites(
      DashboardFilterRequest request)
        {
            var startDate = GetStartDate(request.TimeRange);

            var query = _context.Devices
                .Include(x => x.Region)
                .Include(x => x.SubRegion)
                .Include(x => x.Zone)
                .Where(x => x.InstallationDate >= startDate)
                .AsQueryable();

            // Filters
            if (request.RegionId.HasValue)
            {
                query = query.Where(x => x.RegionId == request.RegionId.Value);
            }

            if (request.SubRegionId.HasValue)
            {
                query = query.Where(x => x.SubRegionId == request.SubRegionId.Value);
            }

            if (request.ZoneId.HasValue)
            {
                query = query.Where(x => x.ZoneId == request.ZoneId.Value);
            }

            if (request.DeviceId.HasValue)
            {
                query = query.Where(x => x.Id == request.DeviceId.Value);
            }

            if (request.Status.HasValue)
            {
                switch (request.Status.Value)
                {
                    case 1:
                        query = query.Where(x => x.Status == "Active");
                        break;

                    case 2:
                        query = query.Where(x => x.Status == "Online");
                        break;

                    case 3:
                        query = query.Where(x => x.Status == "Offline");
                        break;
                }
            }

            var result = await query
                .OrderByDescending(x => x.InstallationDate)
                .Take(10)
                .Select(x => new RecentSiteDto
                {
                    DeviceId = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    Status = x.Status,
                    Address = x.Address,

                    RegionId = x.RegionId,
                    RegionName = x.Region.Name,

                    SubRegionId = x.SubRegionId,
                    SubRegionName = x.SubRegion.Name,

                    ZoneId = x.ZoneId,
                    ZoneName = x.Zone.Name,

                    BatteryQty = x.BatteryQty,
                    InstallationDate = x.InstallationDate,

                    Coordinates = x.Coordinates
                })
                .ToListAsync();

            return result;
        }

        private DateTime GetStartDate(TimeRange range)
        {
            var now = DateTime.UtcNow;

            return range switch
            {
                TimeRange.OneDay => now.AddDays(-1),
                TimeRange.OneWeek => now.AddDays(-7),
                TimeRange.OneMonth => now.AddMonths(-1),
                TimeRange.ThreeMonths => now.AddMonths(-3),
                _ => throw new ArgumentOutOfRangeException(nameof(range), range, null)
            };
        }

        #endregion

        #endregion

        #region Telemetry

        #region BatteryStatusReport

        public async Task<BatteryStatusReportResponse> GetBatteryStatusReport(BatteryStatusReportRequest request)
        {
            var (fromUtc, toUtc) = ResolveBatteryReportRange(request);

            var baseQuery = from packet in _context.TelecomTelemetryPackets
                            join device in _context.Devices on packet.DeviceNumber equals device.Id into deviceJoin
                            from device in deviceJoin.DefaultIfEmpty()
                            where packet.ReceivedAtUtc >= fromUtc && packet.ReceivedAtUtc <= toUtc
                            select new
                            {
                                Packet = packet,
                                SiteName = device != null ? device.Name : null
                            };

            if (request.DeviceId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Packet.DeviceNumber == request.DeviceId.Value);
            }

            var rows = await baseQuery
                .GroupBy(x => new
                {
                    x.Packet.DeviceNumber,
                    x.Packet.DeviceId,
                    x.SiteName,
                    x.Packet.ReceivedAtUtc.Year,
                    x.Packet.ReceivedAtUtc.Month,
                    x.Packet.ReceivedAtUtc.Day,
                    x.Packet.ReceivedAtUtc.Hour
                })
                .Select(g => new BatteryStatusReportRowDto
                {
                    DeviceId = g.Key.DeviceNumber,
                    SiteName = !string.IsNullOrWhiteSpace(g.Key.SiteName)
                        ? g.Key.SiteName
                        : (string.IsNullOrWhiteSpace(g.Key.DeviceId) ? "Unknown Site" : g.Key.DeviceId),
                    DateUtc = new DateTime(
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Day,
                        g.Key.Hour,
                        0,
                        0,
                        DateTimeKind.Utc),
                    PacketsCount = g.Count(),
                    AvgBatteryVoltage = g.Average(x => (decimal?)x.Packet.BatteryVoltage),
                    AvgBatteryCurrent = g.Average(x => (decimal?)x.Packet.BatteryCurrent),
                    AvgBatteryTemperature = g.Average(x => (decimal?)x.Packet.BatteryTemperature),
                    AvgBatteryRemainingPercent = g.Average(x => (decimal?)x.Packet.BatteryRemainingPercent),
                    AvgBatterySoh = g.Average(x => (decimal?)x.Packet.BatterySoh)
                })
                .OrderBy(x => x.DateUtc)
                .ThenBy(x => x.SiteName)
                .ToListAsync();

            return new BatteryStatusReportResponse
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalRecords = rows.Count,
                Records = rows
            };
        }

        private (DateTime FromUtc, DateTime ToUtc) ResolveBatteryReportRange(BatteryStatusReportRequest request)
        {
            var toUtc = request.ToUtc ?? DateTime.UtcNow;

            DateTime fromUtc;

            if (request.FromUtc.HasValue)
            {
                fromUtc = request.FromUtc.Value;
            }
            else if (request.TimeRange.HasValue)
            {
                fromUtc = request.TimeRange.Value switch
                {
                    TimeRange.OneDay => toUtc.AddDays(-1),
                    TimeRange.OneWeek => toUtc.AddDays(-7),
                    TimeRange.OneMonth => toUtc.AddMonths(-1),
                    TimeRange.ThreeMonths => toUtc.AddMonths(-3),
                    _ => toUtc.AddDays(-1)
                };
            }
            else
            {
                fromUtc = toUtc.AddDays(-1);
            }

            if (fromUtc > toUtc)
            {
                (fromUtc, toUtc) = (toUtc, fromUtc);
            }

            return (fromUtc, toUtc);
        }

        #endregion

        #region SolarStatusReport

        public async Task<SolarStatusReportResponse> GetSolarStatusReport(SolarStatusReportRequest request)
        {
            var (fromUtc, toUtc) = ResolveSolarReportRange(request);

            var baseQuery = from packet in _context.TelecomTelemetryPackets
                            join device in _context.Devices on packet.DeviceNumber equals device.Id into deviceJoin
                            from device in deviceJoin.DefaultIfEmpty()
                            where packet.ReceivedAtUtc >= fromUtc && packet.ReceivedAtUtc <= toUtc
                            select new
                            {
                                Packet = packet,
                                SiteName = device != null ? device.Name : null
                            };

            if (request.DeviceId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Packet.DeviceNumber == request.DeviceId.Value);
            }

            var rows = await baseQuery
                .GroupBy(x => new
                {
                    x.Packet.DeviceNumber,
                    x.Packet.DeviceId,
                    x.SiteName,
                    x.Packet.ReceivedAtUtc.Year,
                    x.Packet.ReceivedAtUtc.Month,
                    x.Packet.ReceivedAtUtc.Day,
                    x.Packet.ReceivedAtUtc.Hour
                })
                .Select(g => new SolarStatusReportRowDto
                {
                    DeviceId = g.Key.DeviceNumber,
                    SiteName = !string.IsNullOrWhiteSpace(g.Key.SiteName)
                        ? g.Key.SiteName
                        : (string.IsNullOrWhiteSpace(g.Key.DeviceId) ? "Unknown Site" : g.Key.DeviceId),
                    DateUtc = new DateTime(
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Day,
                        g.Key.Hour,
                        0,
                        0,
                        DateTimeKind.Utc),
                    PacketsCount = g.Count(),
                    AvgSolarVoltage = g.Average(x => (decimal?)x.Packet.SolarVoltage),
                    AvgSolarCurrent = g.Average(x => (decimal?)x.Packet.SolarCurrent),
                    AvgSolarPowerW = g.Average(x => x.Packet.SolarPowerW.HasValue ? (decimal?)x.Packet.SolarPowerW.Value : null),
                    AvgSolarEnergyTodayWh = g.Average(x => x.Packet.SolarEnergyTodayWh.HasValue ? (decimal?)x.Packet.SolarEnergyTodayWh.Value : null),
                    SolarAvailablePercent = g.Count(x => x.Packet.SolarAvailable.HasValue) == 0
                        ? 0
                        : Math.Round((decimal)g.Count(x => x.Packet.SolarAvailable == true) * 100m / g.Count(x => x.Packet.SolarAvailable.HasValue), 2)
                })
                .OrderBy(x => x.DateUtc)
                .ThenBy(x => x.SiteName)
                .ToListAsync();

            return new SolarStatusReportResponse
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalRecords = rows.Count,
                Records = rows
            };
        }

        private (DateTime FromUtc, DateTime ToUtc) ResolveSolarReportRange(SolarStatusReportRequest request)
        {
            var toUtc = request.ToUtc ?? DateTime.UtcNow;

            DateTime fromUtc;

            if (request.FromUtc.HasValue)
            {
                fromUtc = request.FromUtc.Value;
            }
            else if (request.TimeRange.HasValue)
            {
                fromUtc = request.TimeRange.Value switch
                {
                    TimeRange.OneDay => toUtc.AddDays(-1),
                    TimeRange.OneWeek => toUtc.AddDays(-7),
                    TimeRange.OneMonth => toUtc.AddMonths(-1),
                    TimeRange.ThreeMonths => toUtc.AddMonths(-3),
                    _ => toUtc.AddDays(-1)
                };
            }
            else
            {
                fromUtc = toUtc.AddDays(-1);
            }

            if (fromUtc > toUtc)
            {
                (fromUtc, toUtc) = (toUtc, fromUtc);
            }

            return (fromUtc, toUtc);
        }

        #endregion

        #region GridStatusReport

        public async Task<GridStatusReportResponse> GetGridStatusReport(GridStatusReportRequest request)
        {
            var (fromUtc, toUtc) = ResolveGridReportRange(request);

            var baseQuery = from packet in _context.TelecomTelemetryPackets
                            join device in _context.Devices on packet.DeviceNumber equals device.Id into deviceJoin
                            from device in deviceJoin.DefaultIfEmpty()
                            where packet.ReceivedAtUtc >= fromUtc && packet.ReceivedAtUtc <= toUtc
                            select new
                            {
                                Packet = packet,
                                SiteName = device != null ? device.Name : null
                            };

            if (request.DeviceId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Packet.DeviceNumber == request.DeviceId.Value);
            }

            var rows = await baseQuery
                .GroupBy(x => new
                {
                    x.Packet.DeviceNumber,
                    x.Packet.DeviceId,
                    x.SiteName,
                    x.Packet.ReceivedAtUtc.Year,
                    x.Packet.ReceivedAtUtc.Month,
                    x.Packet.ReceivedAtUtc.Day,
                    x.Packet.ReceivedAtUtc.Hour
                })
                .Select(g => new GridStatusReportRowDto
                {
                    DeviceId = g.Key.DeviceNumber,
                    SiteName = !string.IsNullOrWhiteSpace(g.Key.SiteName)
                        ? g.Key.SiteName
                        : (string.IsNullOrWhiteSpace(g.Key.DeviceId) ? "Unknown Site" : g.Key.DeviceId),
                    DateUtc = new DateTime(
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Day,
                        g.Key.Hour,
                        0,
                        0,
                        DateTimeKind.Utc),
                    PacketsCount = g.Count(),
                    AvgLineAVoltage = g.Average(x => (decimal?)x.Packet.LineAVoltage),
                    AvgLineBVoltage = g.Average(x => (decimal?)x.Packet.LineBVoltage),
                    AvgLineCVoltage = g.Average(x => (decimal?)x.Packet.LineCVoltage),
                    AvgLineACurrent = g.Average(x => (decimal?)x.Packet.LineACurrent),
                    AvgLineBCurrent = g.Average(x => (decimal?)x.Packet.LineBCurrent),
                    AvgLineCCurrent = g.Average(x => (decimal?)x.Packet.LineCCurrent),
                    AvgAcFrequency = g.Average(x => (decimal?)x.Packet.AcFrequency),
                    AvgAcInputPowerW = g.Average(x => x.Packet.TotalAcInputPowerW.HasValue ? (decimal?)x.Packet.TotalAcInputPowerW.Value : null),
                    GridAvailablePercent = g.Count(x => x.Packet.MainsAvailable.HasValue) == 0
                        ? 0
                        : Math.Round((decimal)g.Count(x => x.Packet.MainsAvailable == true) * 100m / g.Count(x => x.Packet.MainsAvailable.HasValue), 2)
                })
                .OrderBy(x => x.DateUtc)
                .ThenBy(x => x.SiteName)
                .ToListAsync();

            return new GridStatusReportResponse
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalRecords = rows.Count,
                Records = rows
            };
        }

        private (DateTime FromUtc, DateTime ToUtc) ResolveGridReportRange(GridStatusReportRequest request)
        {
            var toUtc = request.ToUtc ?? DateTime.UtcNow;

            DateTime fromUtc;

            if (request.FromUtc.HasValue)
            {
                fromUtc = request.FromUtc.Value;
            }
            else if (request.TimeRange.HasValue)
            {
                fromUtc = request.TimeRange.Value switch
                {
                    TimeRange.OneDay => toUtc.AddDays(-1),
                    TimeRange.OneWeek => toUtc.AddDays(-7),
                    TimeRange.OneMonth => toUtc.AddMonths(-1),
                    TimeRange.ThreeMonths => toUtc.AddMonths(-3),
                    _ => toUtc.AddDays(-1)
                };
            }
            else
            {
                fromUtc = toUtc.AddDays(-1);
            }

            if (fromUtc > toUtc)
            {
                (fromUtc, toUtc) = (toUtc, fromUtc);
            }

            return (fromUtc, toUtc);
        }

        #endregion

        #region AlarmStatusReport

        public async Task<AlarmStatusReportResponse> GetAlarmStatusReport(AlarmStatusReportRequest request)
        {
            var (fromUtc, toUtc) = ResolveAlarmReportRange(request);

            var baseQuery = from packet in _context.TelecomTelemetryPackets
                            join device in _context.Devices on packet.DeviceNumber equals device.Id into deviceJoin
                            from device in deviceJoin.DefaultIfEmpty()
                            where packet.ReceivedAtUtc >= fromUtc && packet.ReceivedAtUtc <= toUtc
                            select new
                            {
                                Packet = packet,
                                SiteName = device != null ? device.Name : null
                            };

            if (request.DeviceId.HasValue)
            {
                var deviceIdString = request.DeviceId.Value.ToString();
                baseQuery = baseQuery.Where(x =>
                    x.Packet.DeviceNumber == request.DeviceId.Value ||
                    x.Packet.DeviceId == deviceIdString);
            }

            baseQuery = baseQuery.Where(x =>
                (x.Packet.Alarm1Code ?? 0) > 0
                || (x.Packet.Alarm2Code ?? 0) > 0
                || (x.Packet.Alarm3Code ?? 0) > 0);

            var rows = await baseQuery
                .GroupBy(x => new
                {
                    x.Packet.DeviceNumber,
                    x.Packet.DeviceId,
                    x.SiteName,
                    x.Packet.ReceivedAtUtc.Year,
                    x.Packet.ReceivedAtUtc.Month,
                    x.Packet.ReceivedAtUtc.Day,
                    x.Packet.ReceivedAtUtc.Hour
                })
                .Select(g => new AlarmStatusReportRowDto
                {
                    DeviceId = g.Key.DeviceNumber,
                    SiteName = !string.IsNullOrWhiteSpace(g.Key.SiteName)
                        ? g.Key.SiteName
                        : (string.IsNullOrWhiteSpace(g.Key.DeviceId) ? "Unknown Site" : g.Key.DeviceId),
                    DateUtc = new DateTime(
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Day,
                        g.Key.Hour,
                        0,
                        0,
                        DateTimeKind.Utc),
                    PacketsCount = g.Count(),
                    AvgActiveAlarmCount = g.Average(x => (decimal?)(x.Packet.ActiveAlarmCount ?? 0)),
                    AvgAlarm1Code = g.Average(x => (x.Packet.Alarm1Code ?? 0) > 0 ? (decimal?)x.Packet.Alarm1Code : null),
                    AvgAlarm2Code = g.Average(x => (x.Packet.Alarm2Code ?? 0) > 0 ? (decimal?)x.Packet.Alarm2Code : null),
                    AvgAlarm3Code = g.Average(x => (x.Packet.Alarm3Code ?? 0) > 0 ? (decimal?)x.Packet.Alarm3Code : null),
                    PacketsWithActiveAlarms = g.Count(x => (x.Packet.ActiveAlarmCount ?? 0) > 0),
                    CriticalAlarmPackets = g.Count(x =>
                        x.Packet.Alarm1Level == AlarmLevelType.Critical
                        || x.Packet.Alarm2Level == AlarmLevelType.Critical
                        || x.Packet.Alarm3Level == AlarmLevelType.Critical),
                    MajorAlarmPackets = g.Count(x =>
                        x.Packet.Alarm1Level == AlarmLevelType.Major
                        || x.Packet.Alarm2Level == AlarmLevelType.Major
                        || x.Packet.Alarm3Level == AlarmLevelType.Major),
                    MinorAlarmPackets = g.Count(x =>
                        x.Packet.Alarm1Level == AlarmLevelType.Minor
                        || x.Packet.Alarm2Level == AlarmLevelType.Minor
                        || x.Packet.Alarm3Level == AlarmLevelType.Minor),
                    WarningAlarmPackets = g.Count(x =>
                        x.Packet.Alarm1Level == AlarmLevelType.Warning
                        || x.Packet.Alarm2Level == AlarmLevelType.Warning
                        || x.Packet.Alarm3Level == AlarmLevelType.Warning),
                    DoorOpenEvents = g.Count(x => x.Packet.DoorOpenAlarm == true),
                    SmokeEvents = g.Count(x => x.Packet.SmokeAlarm == true),
                    WaterLeakEvents = g.Count(x => x.Packet.WaterLeakAlarm == true),
                    MotionEvents = g.Count(x => x.Packet.MotionAlarm == true)
                })
                .OrderBy(x => x.DateUtc)
                .ThenBy(x => x.SiteName)
                .ToListAsync();

            return new AlarmStatusReportResponse
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalRecords = rows.Count,
                Records = rows
            };
        }

        private (DateTime FromUtc, DateTime ToUtc) ResolveAlarmReportRange(AlarmStatusReportRequest request)
        {
            var toUtc = request.ToUtc ?? DateTime.UtcNow;

            DateTime fromUtc;

            if (request.FromUtc.HasValue)
            {
                fromUtc = request.FromUtc.Value;
            }
            else if (request.TimeRange.HasValue)
            {
                fromUtc = request.TimeRange.Value switch
                {
                    TimeRange.OneDay => toUtc.AddDays(-1),
                    TimeRange.OneWeek => toUtc.AddDays(-7),
                    TimeRange.OneMonth => toUtc.AddMonths(-1),
                    TimeRange.ThreeMonths => toUtc.AddMonths(-3),
                    _ => toUtc.AddDays(-1)
                };
            }
            else
            {
                // Alarm report is often queried without an explicit date range.
                // Use a wider default window so historical alarm activity is returned.
                fromUtc = toUtc.AddMonths(-3);
            }

            if (fromUtc > toUtc)
            {
                (fromUtc, toUtc) = (toUtc, fromUtc);
            }

            return (fromUtc, toUtc);
        }

        #endregion

        #region EnergyConsumptionReport

        public async Task<EnergyConsumptionReportResponse> GetEnergyConsumptionReport(EnergyConsumptionReportRequest request)
        {
            var (fromUtc, toUtc) = ResolveEnergyConsumptionReportRange(request);

            var baseQuery = from packet in _context.TelecomTelemetryPackets
                            join device in _context.Devices on packet.DeviceNumber equals device.Id into deviceJoin
                            from device in deviceJoin.DefaultIfEmpty()
                            where packet.ReceivedAtUtc >= fromUtc && packet.ReceivedAtUtc <= toUtc
                            select new
                            {
                                Packet = packet,
                                SiteName = device != null ? device.Name : null
                            };

            if (request.DeviceId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Packet.DeviceNumber == request.DeviceId.Value);
            }

            var rows = await baseQuery
                .GroupBy(x => new
                {
                    x.Packet.DeviceNumber,
                    x.Packet.DeviceId,
                    x.SiteName,
                    x.Packet.ReceivedAtUtc.Year,
                    x.Packet.ReceivedAtUtc.Month,
                    x.Packet.ReceivedAtUtc.Day,
                    x.Packet.ReceivedAtUtc.Hour
                })
                .Select(g => new EnergyConsumptionReportRowDto
                {
                    DeviceId = g.Key.DeviceNumber,
                    SiteName = !string.IsNullOrWhiteSpace(g.Key.SiteName)
                        ? g.Key.SiteName
                        : (string.IsNullOrWhiteSpace(g.Key.DeviceId) ? "Unknown Site" : g.Key.DeviceId),
                    DateUtc = new DateTime(
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Day,
                        g.Key.Hour,
                        0,
                        0,
                        DateTimeKind.Utc),
                    PacketsCount = g.Count(),
                    AvgTotalAcEnergyWh = g.Average(x => x.Packet.TotalAcEnergyWh.HasValue ? (decimal?)x.Packet.TotalAcEnergyWh.Value : null),
                    AvgTotalDcEnergyWh = g.Average(x => x.Packet.TotalDcEnergyWh.HasValue ? (decimal?)x.Packet.TotalDcEnergyWh.Value : null),
                    AvgSolarEnergyTodayWh = g.Average(x => x.Packet.SolarEnergyTodayWh.HasValue ? (decimal?)x.Packet.SolarEnergyTodayWh.Value : null),
                    AvgAcInputPowerW = g.Average(x => x.Packet.TotalAcInputPowerW.HasValue ? (decimal?)x.Packet.TotalAcInputPowerW.Value : null),
                    AvgDcLoadPowerW = g.Average(x => x.Packet.DcLoadPowerW.HasValue ? (decimal?)x.Packet.DcLoadPowerW.Value : null),
                    AvgTenantLoadW = g.Average(x =>
                        !x.Packet.Tenant1LoadW.HasValue
                        && !x.Packet.Tenant2LoadW.HasValue
                        && !x.Packet.Tenant3LoadW.HasValue
                        && !x.Packet.Tenant4LoadW.HasValue
                            ? (decimal?)null
                            : (x.Packet.Tenant1LoadW.HasValue ? (decimal?)x.Packet.Tenant1LoadW.Value : 0m)
                              + (x.Packet.Tenant2LoadW.HasValue ? (decimal?)x.Packet.Tenant2LoadW.Value : 0m)
                              + (x.Packet.Tenant3LoadW.HasValue ? (decimal?)x.Packet.Tenant3LoadW.Value : 0m)
                              + (x.Packet.Tenant4LoadW.HasValue ? (decimal?)x.Packet.Tenant4LoadW.Value : 0m))
                })
                .OrderBy(x => x.DateUtc)
                .ThenBy(x => x.SiteName)
                .ToListAsync();

            return new EnergyConsumptionReportResponse
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                TotalRecords = rows.Count,
                Records = rows
            };
        }

        private (DateTime FromUtc, DateTime ToUtc) ResolveEnergyConsumptionReportRange(EnergyConsumptionReportRequest request)
        {
            var toUtc = request.ToUtc ?? DateTime.UtcNow;

            DateTime fromUtc;

            if (request.FromUtc.HasValue)
            {
                fromUtc = request.FromUtc.Value;
            }
            else if (request.TimeRange.HasValue)
            {
                fromUtc = request.TimeRange.Value switch
                {
                    TimeRange.OneDay => toUtc.AddDays(-1),
                    TimeRange.OneWeek => toUtc.AddDays(-7),
                    TimeRange.OneMonth => toUtc.AddMonths(-1),
                    TimeRange.ThreeMonths => toUtc.AddMonths(-3),
                    _ => toUtc.AddDays(-1)
                };
            }
            else
            {
                fromUtc = toUtc.AddDays(-1);
            }

            if (fromUtc > toUtc)
            {
                (fromUtc, toUtc) = (toUtc, fromUtc);
            }

            return (fromUtc, toUtc);
        }

        #endregion

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
