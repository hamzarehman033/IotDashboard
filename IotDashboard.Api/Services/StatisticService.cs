using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;
using IotDashboard.Infrastructure.AuditServices;
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
        Task<GraphResponse> GetSiteTotalLoadGraph(GraphRequest request);
        Task<GraphResponse> GetGridVoltageGraph(GraphRequest request);
        Task<GraphResponse> GetTenantLoadTrendsGraph(GraphRequest request);
        Task<GraphResponse> GetBatterySocGraph(GraphRequest request);
        Task<GraphResponse> GetSolarYieldGraph(GraphRequest request);
    }
    public class StatisticService : IStatisticService
    {
        private readonly AppDBContext _context;
        private readonly ICurrentUserService _currentUserService;

        #region dashboard
        #region dashboard-summary
        public StatisticService(AppDBContext dbContext, ICurrentUserService currentUserService)
        {
            _context = dbContext;
            _currentUserService = currentUserService;
        }

        private long GetActiveCustomerId() => _currentUserService.GetCustomerId();

        private IQueryable<Device> CustomerDevices(long customerId) =>
            _context.Devices.Where(x => x.CustomerId == customerId);

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
            var customerId = GetActiveCustomerId();
            if (customerId <= 0)
            {
                return new BatteryStatusReportResponse
                {
                    FromUtc = fromUtc,
                    ToUtc = toUtc,
                    TotalRecords = 0,
                    Records = new List<BatteryStatusReportRowDto>()
                };
            }

            var tenantDeviceIds = new List<long>();
            if (request.TenantId.HasValue)
            {
                tenantDeviceIds = await _context.DeviceTenants
                    .Where(x => x.TenantId == request.TenantId.Value && x.Device.CustomerId == customerId)
                    .Select(x => x.DeviceId)
                    .Distinct()
                    .ToListAsync();
            }

            var baseQuery = from packet in _context.TelecomTelemetryPackets
                            join device in CustomerDevices(customerId) on packet.DeviceNumber equals device.Id
                            where packet.ReceivedAtUtc >= fromUtc && packet.ReceivedAtUtc <= toUtc
                            select new
                            {
                                Packet = packet,
                                SiteName = device.Name
                            };

            if (request.DeviceId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Packet.DeviceNumber == request.DeviceId.Value);
            }

            if (request.TenantId.HasValue)
            {
                baseQuery = baseQuery.Where(x => tenantDeviceIds.Contains(x.Packet.DeviceNumber));
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
            var customerId = GetActiveCustomerId();
            if (customerId <= 0)
            {
                return new SolarStatusReportResponse
                {
                    FromUtc = fromUtc,
                    ToUtc = toUtc,
                    TotalRecords = 0,
                    Records = new List<SolarStatusReportRowDto>()
                };
            }

            var tenantDeviceIds = new List<long>();
            if (request.TenantId.HasValue)
            {
                tenantDeviceIds = await _context.DeviceTenants
                    .Where(x => x.TenantId == request.TenantId.Value && x.Device.CustomerId == customerId)
                    .Select(x => x.DeviceId)
                    .Distinct()
                    .ToListAsync();
            }

            var baseQuery = from packet in _context.TelecomTelemetryPackets
                            join device in CustomerDevices(customerId) on packet.DeviceNumber equals device.Id
                            where packet.ReceivedAtUtc >= fromUtc && packet.ReceivedAtUtc <= toUtc
                            select new
                            {
                                Packet = packet,
                                SiteName = device.Name
                            };

            if (request.DeviceId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Packet.DeviceNumber == request.DeviceId.Value);
            }

            if (request.TenantId.HasValue)
            {
                baseQuery = baseQuery.Where(x => tenantDeviceIds.Contains(x.Packet.DeviceNumber));
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
            var customerId = GetActiveCustomerId();
            if (customerId <= 0)
            {
                return new GridStatusReportResponse
                {
                    FromUtc = fromUtc,
                    ToUtc = toUtc,
                    TotalRecords = 0,
                    Records = new List<GridStatusReportRowDto>()
                };
            }

            var tenantDeviceIds = new List<long>();
            if (request.TenantId.HasValue)
            {
                tenantDeviceIds = await _context.DeviceTenants
                    .Where(x => x.TenantId == request.TenantId.Value && x.Device.CustomerId == customerId)
                    .Select(x => x.DeviceId)
                    .Distinct()
                    .ToListAsync();
            }

            var baseQuery = from packet in _context.TelecomTelemetryPackets
                            join device in CustomerDevices(customerId) on packet.DeviceNumber equals device.Id
                            where packet.ReceivedAtUtc >= fromUtc && packet.ReceivedAtUtc <= toUtc
                            select new
                            {
                                Packet = packet,
                                SiteName = device.Name
                            };

            if (request.DeviceId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Packet.DeviceNumber == request.DeviceId.Value);
            }

            if (request.TenantId.HasValue)
            {
                baseQuery = baseQuery.Where(x => tenantDeviceIds.Contains(x.Packet.DeviceNumber));
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
            var customerId = GetActiveCustomerId();
            if (customerId <= 0)
            {
                return new AlarmStatusReportResponse
                {
                    FromUtc = fromUtc,
                    ToUtc = toUtc,
                    TotalRecords = 0,
                    Records = new List<AlarmStatusReportRowDto>()
                };
            }

            var tenantDeviceIds = new List<long>();
            if (request.TenantId.HasValue)
            {
                tenantDeviceIds = await _context.DeviceTenants
                    .Where(x => x.TenantId == request.TenantId.Value && x.Device.CustomerId == customerId)
                    .Select(x => x.DeviceId)
                    .Distinct()
                    .ToListAsync();
            }

            var baseQuery = from packet in _context.TelecomTelemetryPackets
                            join device in CustomerDevices(customerId) on packet.DeviceNumber equals device.Id
                            where packet.ReceivedAtUtc >= fromUtc && packet.ReceivedAtUtc <= toUtc
                            select new
                            {
                                Packet = packet,
                                SiteName = device.Name
                            };

            if (request.DeviceId.HasValue)
            {
                var deviceIdString = request.DeviceId.Value.ToString();
                baseQuery = baseQuery.Where(x =>
                    x.Packet.DeviceNumber == request.DeviceId.Value ||
                    x.Packet.DeviceId == deviceIdString);
            }

            if (request.TenantId.HasValue)
            {
                baseQuery = baseQuery.Where(x => tenantDeviceIds.Contains(x.Packet.DeviceNumber));
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
            var customerId = GetActiveCustomerId();
            if (customerId <= 0)
            {
                return new EnergyConsumptionReportResponse
                {
                    FromUtc = fromUtc,
                    ToUtc = toUtc,
                    TotalRecords = 0,
                    Records = new List<EnergyConsumptionReportRowDto>()
                };
            }

            var tenantDeviceIds = new List<long>();
            if (request.TenantId.HasValue)
            {
                tenantDeviceIds = await _context.DeviceTenants
                    .Where(x => x.TenantId == request.TenantId.Value && x.Device.CustomerId == customerId)
                    .Select(x => x.DeviceId)
                    .Distinct()
                    .ToListAsync();
            }

            var baseQuery = from packet in _context.TelecomTelemetryPackets
                            join device in CustomerDevices(customerId) on packet.DeviceNumber equals device.Id
                            where packet.ReceivedAtUtc >= fromUtc && packet.ReceivedAtUtc <= toUtc
                            select new
                            {
                                Packet = packet,
                                SiteName = device.Name
                            };

            if (request.DeviceId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Packet.DeviceNumber == request.DeviceId.Value);
            }

            if (request.TenantId.HasValue)
            {
                baseQuery = baseQuery.Where(x => tenantDeviceIds.Contains(x.Packet.DeviceNumber));
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

        #region Graphs

        public async Task<GraphResponse> GetSiteTotalLoadGraph(GraphRequest request)
        {
            var (fromUtc, toUtc, bucket, normalizedTimeframe) = ResolveGraphRange(request);
            var bucketTimeline = BuildBucketTimeline(fromUtc, toUtc, bucket);

            var query = _context.TelecomTelemetryPackets
                .Where(x => x.ReceivedAtUtc >= fromUtc && x.ReceivedAtUtc <= toUtc);
            query = ApplyGraphFilters(query, request);

            var rows = await query
                .Select(x => new { x.ReceivedAtUtc, x.DeviceNumber, x.TotalAcInputPowerW })
                .ToListAsync();

            var bucketValues = rows
                .Where(x => x.TotalAcInputPowerW.HasValue)
                .GroupBy(x => GetBucketStart(x.ReceivedAtUtc, bucket))
                .ToDictionary(
                    g => g.Key,
                    g => (decimal?)g
                        .GroupBy(x => x.DeviceNumber)
                        .Select(deviceGroup => deviceGroup.Average(x => (decimal)x.TotalAcInputPowerW!.Value))
                        .Sum());

            return BuildGraphResponse(request, fromUtc, toUtc, bucket, normalizedTimeframe, new List<GraphSeriesDto>
            {
                BuildSeries("site-total-load", bucketTimeline, bucketValues)
            });
        }

        public async Task<GraphResponse> GetGridVoltageGraph(GraphRequest request)
        {
            var (fromUtc, toUtc, bucket, normalizedTimeframe) = ResolveGraphRange(request);
            var bucketTimeline = BuildBucketTimeline(fromUtc, toUtc, bucket);

            var query = _context.TelecomTelemetryPackets
                .Where(x => x.ReceivedAtUtc >= fromUtc && x.ReceivedAtUtc <= toUtc);
            query = ApplyGraphFilters(query, request);

            var rows = await query
                .Select(x => new
                {
                    x.ReceivedAtUtc,
                    x.LineAVoltage,
                    x.LineBVoltage,
                    x.LineCVoltage
                })
                .ToListAsync();

            var l1Values = rows
                .Where(x => x.LineAVoltage.HasValue)
                .GroupBy(x => GetBucketStart(x.ReceivedAtUtc, bucket))
                .ToDictionary(g => g.Key, g => (decimal?)g.Average(x => x.LineAVoltage!.Value));

            var l2Values = rows
                .Where(x => x.LineBVoltage.HasValue)
                .GroupBy(x => GetBucketStart(x.ReceivedAtUtc, bucket))
                .ToDictionary(g => g.Key, g => (decimal?)g.Average(x => x.LineBVoltage!.Value));

            var l3Values = rows
                .Where(x => x.LineCVoltage.HasValue)
                .GroupBy(x => GetBucketStart(x.ReceivedAtUtc, bucket))
                .ToDictionary(g => g.Key, g => (decimal?)g.Average(x => x.LineCVoltage!.Value));

            return BuildGraphResponse(request, fromUtc, toUtc, bucket, normalizedTimeframe, new List<GraphSeriesDto>
            {
                BuildSeries("L1", bucketTimeline, l1Values),
                BuildSeries("L2", bucketTimeline, l2Values),
                BuildSeries("L3", bucketTimeline, l3Values)
            });
        }

        public async Task<GraphResponse> GetTenantLoadTrendsGraph(GraphRequest request)
        {
            var (fromUtc, toUtc, bucket, normalizedTimeframe) = ResolveGraphRange(request);
            var bucketTimeline = BuildBucketTimeline(fromUtc, toUtc, bucket);

            var query = _context.TelecomTelemetryPackets
                .Where(x => x.ReceivedAtUtc >= fromUtc && x.ReceivedAtUtc <= toUtc);

            if (request.DeviceId.HasValue)
            {
                query = query.Where(x => x.DeviceNumber == request.DeviceId.Value);
            }

            var rows = await query
                .Select(x => new
                {
                    x.ReceivedAtUtc,
                    x.Tenant1LoadW,
                    x.Tenant2LoadW,
                    x.Tenant3LoadW,
                    x.Tenant4LoadW
                })
                .ToListAsync();

            var valuesBySlot = new Dictionary<int, Dictionary<DateTime, decimal?>>()
            {
                [1] = rows.Where(x => x.Tenant1LoadW.HasValue)
                    .GroupBy(x => GetBucketStart(x.ReceivedAtUtc, bucket))
                    .ToDictionary(g => g.Key, g => (decimal?)g.Average(x => (decimal)x.Tenant1LoadW!.Value)),
                [2] = rows.Where(x => x.Tenant2LoadW.HasValue)
                    .GroupBy(x => GetBucketStart(x.ReceivedAtUtc, bucket))
                    .ToDictionary(g => g.Key, g => (decimal?)g.Average(x => (decimal)x.Tenant2LoadW!.Value)),
                [3] = rows.Where(x => x.Tenant3LoadW.HasValue)
                    .GroupBy(x => GetBucketStart(x.ReceivedAtUtc, bucket))
                    .ToDictionary(g => g.Key, g => (decimal?)g.Average(x => (decimal)x.Tenant3LoadW!.Value)),
                [4] = rows.Where(x => x.Tenant4LoadW.HasValue)
                    .GroupBy(x => GetBucketStart(x.ReceivedAtUtc, bucket))
                    .ToDictionary(g => g.Key, g => (decimal?)g.Average(x => (decimal)x.Tenant4LoadW!.Value))
            };

            var series = new List<GraphSeriesDto>();
            var tenantSlots = request.TenantId.HasValue
                ? new[] { (int)request.TenantId.Value }
                : new[] { 1, 2, 3, 4 };

            foreach (var slot in tenantSlots)
            {
                if (valuesBySlot.TryGetValue(slot, out var values))
                {
                    series.Add(BuildSeries($"tenant-{slot}", bucketTimeline, values));
                }
            }

            return BuildGraphResponse(request, fromUtc, toUtc, bucket, normalizedTimeframe, series);
        }

        public async Task<GraphResponse> GetBatterySocGraph(GraphRequest request)
        {
            var (fromUtc, toUtc, bucket, normalizedTimeframe) = ResolveGraphRange(request);
            var bucketTimeline = BuildBucketTimeline(fromUtc, toUtc, bucket);

            var query = _context.TelecomTelemetryPackets
                .Where(x => x.ReceivedAtUtc >= fromUtc && x.ReceivedAtUtc <= toUtc);
            query = ApplyGraphFilters(query, request);

            var rows = await query
                .Select(x => new { x.ReceivedAtUtc, x.BatteryRemainingPercent })
                .ToListAsync();

            var bucketValues = rows
                .Where(x => x.BatteryRemainingPercent.HasValue)
                .GroupBy(x => GetBucketStart(x.ReceivedAtUtc, bucket))
                .ToDictionary(g => g.Key, g => (decimal?)g.Average(x => (decimal)x.BatteryRemainingPercent!.Value));

            return BuildGraphResponse(request, fromUtc, toUtc, bucket, normalizedTimeframe, new List<GraphSeriesDto>
            {
                BuildSeries("battery-soc", bucketTimeline, bucketValues)
            });
        }

        public async Task<GraphResponse> GetSolarYieldGraph(GraphRequest request)
        {
            var (fromUtc, toUtc, bucket, normalizedTimeframe) = ResolveGraphRange(request);
            var bucketTimeline = BuildBucketTimeline(fromUtc, toUtc, bucket);

            var query = _context.TelecomTelemetryPackets
                .Where(x => x.ReceivedAtUtc >= fromUtc && x.ReceivedAtUtc <= toUtc);
            query = ApplyGraphFilters(query, request);

            var rows = await query
                .Select(x => new { x.ReceivedAtUtc, x.SolarEnergyTodayWh })
                .ToListAsync();

            var bucketValues = rows
                .Where(x => x.SolarEnergyTodayWh.HasValue)
                .GroupBy(x => GetBucketStart(x.ReceivedAtUtc, bucket))
                .ToDictionary(g => g.Key, g => (decimal?)g.Average(x => (decimal)x.SolarEnergyTodayWh!.Value));

            return BuildGraphResponse(request, fromUtc, toUtc, bucket, normalizedTimeframe, new List<GraphSeriesDto>
            {
                BuildSeries("solar-yield", bucketTimeline, bucketValues)
            });
        }

        private static (DateTime FromUtc, DateTime ToUtc, GraphBucket Bucket, string NormalizedTimeframe) ResolveGraphRange(GraphRequest request)
        {
            var toUtc = request.ToUtc ?? DateTime.UtcNow;
            var timeframe = ParseGraphTimeframe(request.Timeframe);

            return timeframe switch
            {
                GraphTimeframeOption.TwentyFourHours =>
                    (GetHourStart(toUtc).AddHours(-23), toUtc, GraphBucket.Hour, "24h"),
                GraphTimeframeOption.SevenDays =>
                    (GetDayStart(toUtc).AddDays(-6), toUtc, GraphBucket.Day, "7days"),
                GraphTimeframeOption.ThirtyDays =>
                    (GetDayStart(toUtc).AddDays(-29), toUtc, GraphBucket.Day, "30days"),
                _ => throw new ArgumentOutOfRangeException(nameof(request.Timeframe), request.Timeframe, null)
            };
        }

        private static GraphTimeframeOption ParseGraphTimeframe(string timeframe)
        {
            return timeframe?.Trim().ToLowerInvariant() switch
            {
                "24h" => GraphTimeframeOption.TwentyFourHours,
                "7days" => GraphTimeframeOption.SevenDays,
                "30days" => GraphTimeframeOption.ThirtyDays,
                _ => throw new ArgumentException("Timeframe must be one of: 24h, 7days, 30days.", nameof(timeframe))
            };
        }

        private IQueryable<TelecomTelemetryPacket> ApplyGraphFilters(
            IQueryable<TelecomTelemetryPacket> query,
            GraphRequest request)
        {
            if (request.DeviceId.HasValue)
            {
                query = query.Where(x => x.DeviceNumber == request.DeviceId.Value);
            }

            if (request.TenantId.HasValue)
            {
                query = query.Where(x => x.TenantNumber == (int)request.TenantId.Value);
            }

            return query;
        }

        private static GraphResponse BuildGraphResponse(
            GraphRequest request,
            DateTime fromUtc,
            DateTime toUtc,
            GraphBucket bucket,
            string normalizedTimeframe,
            List<GraphSeriesDto> series)
        {
            return new GraphResponse
            {
                Meta = new GraphMetaDto
                {
                    Timeframe = normalizedTimeframe,
                    FromUtc = fromUtc,
                    ToUtc = toUtc,
                    Bucket = bucket == GraphBucket.Hour ? "hour" : "day",
                    FiltersApplied = new GraphFiltersAppliedDto
                    {
                        DeviceId = request.DeviceId,
                        TenantId = request.TenantId
                    }
                },
                Series = series
            };
        }

        private static GraphSeriesDto BuildSeries(
            string name,
            List<DateTime> bucketTimeline,
            IReadOnlyDictionary<DateTime, decimal?> valuesByBucket)
        {
            return new GraphSeriesDto
            {
                Name = name,
                Points = bucketTimeline.Select(bucket => new GraphPointDto
                {
                    Timestamp = bucket,
                    Value = valuesByBucket.TryGetValue(bucket, out var value) ? value : null
                }).ToList()
            };
        }

        private static List<DateTime> BuildBucketTimeline(DateTime fromUtc, DateTime toUtc, GraphBucket bucket)
        {
            var result = new List<DateTime>();
            var current = GetBucketStart(fromUtc, bucket);
            var end = GetBucketStart(toUtc, bucket);

            while (current <= end)
            {
                result.Add(current);
                current = bucket == GraphBucket.Hour ? current.AddHours(1) : current.AddDays(1);
            }

            return result;
        }

        private static DateTime GetBucketStart(DateTime timestamp, GraphBucket bucket)
        {
            return bucket == GraphBucket.Hour ? GetHourStart(timestamp) : GetDayStart(timestamp);
        }

        private static DateTime GetHourStart(DateTime timestamp)
        {
            return new DateTime(
                timestamp.Year,
                timestamp.Month,
                timestamp.Day,
                timestamp.Hour,
                0,
                0,
                DateTimeKind.Utc);
        }

        private static DateTime GetDayStart(DateTime timestamp)
        {
            return new DateTime(
                timestamp.Year,
                timestamp.Month,
                timestamp.Day,
                0,
                0,
                0,
                DateTimeKind.Utc);
        }

        private enum GraphBucket
        {
            Hour,
            Day
        }

        private enum GraphTimeframeOption
        {
            TwentyFourHours,
            SevenDays,
            ThirtyDays
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
