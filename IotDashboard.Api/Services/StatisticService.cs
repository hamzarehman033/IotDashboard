using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;
using IotDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Api.Services
{
    public interface IStatisticService
    {
        Task<DashboardStatsResponse> GetDashboardStats(StatisticsFilterRequest request);
    }
    public class StatisticService : IStatisticService
    {
        private readonly AppDBContext _context;

        public StatisticService(AppDBContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<DashboardStatsResponse> GetDashboardStats(
              StatisticsFilterRequest request)
        {
            var (currentStart, currentEnd) = GetDateRange(request.TimeRange);
            var duration = currentEnd - currentStart;

            var previousStart = currentStart - duration;
            var previousEnd = currentStart;

            var currentQuery = ApplyFilters(
                _context.TelecomTelemetryPackets.AsQueryable(),
                request,
                currentStart,
                currentEnd);

            var previousQuery = ApplyFilters(
                _context.TelecomTelemetryPackets.AsQueryable(),
                request,
                previousStart,
                previousEnd);

            var currentStats = await CalculateStats(currentQuery, duration);
            var previousStats = await CalculateStats(previousQuery, duration);

            return new DashboardStatsResponse
            {
                TotalSites = Compare(currentStats.TotalSites, previousStats.TotalSites),
                OnlineOnce = Compare(currentStats.OnlineOnce, previousStats.OnlineOnce),
                ActiveAlerts = Compare(currentStats.ActiveAlerts, previousStats.ActiveAlerts),
                MessagesPerMinute = Compare(currentStats.MessagesPerMinute, previousStats.MessagesPerMinute)
            };
        }

        private IQueryable<TelecomTelemetryPacket> ApplyFilters(
            IQueryable<TelecomTelemetryPacket> query,
            StatisticsFilterRequest request,
            DateTime start,
            DateTime end)
        {
            query = query.Where(x =>
                x.ReceivedAtUtc >= start &&
                x.ReceivedAtUtc <= end);

            if (request.RegionId.HasValue)
                query = query.Where(x => x.RegionId == request.RegionId.Value);

            if (request.SubRegionId.HasValue)
                query = query.Where(x => x.SubRegionId == request.SubRegionId.Value);

            if (request.ZoneId.HasValue)
                query = query.Where(x => x.ZoneId == request.ZoneId.Value);

            if (!string.IsNullOrWhiteSpace(request.DeviceId))
                query = query.Where(x => x.DeviceId == request.DeviceId);

            // Type filter (adjust when your actual type column exists)
            /*
            if (!string.IsNullOrWhiteSpace(request.Type))
                query = query.Where(x => x.Type == request.Type);
            */

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                switch (request.Status.ToLower())
                {
                    case "online":
                        query = query.Where(x => x.MainsAvailable == true);
                        break;

                    case "offline":
                        query = query.Where(x => x.MainsAvailable == false);
                        break;

                    case "alert":
                        query = query.Where(x => x.ActiveAlarmCount > 0);
                        break;
                }
            }

            return query;
        }

        private async Task<StatsResult> CalculateStats(
            IQueryable<TelecomTelemetryPacket> query,
            TimeSpan duration)
        {
            var totalSites = await query
                .Select(x => x.SiteId)
                .Distinct()
                .CountAsync();

            var onlineOnce = await query
                .Select(x => x.SiteId)
                .Distinct()
                .CountAsync();

            var activeAlerts = await query
                .Where(x => x.ActiveAlarmCount > 0)
                .CountAsync();

            var totalMessages = await query.CountAsync();

            var msgsPerMinute = duration.TotalMinutes == 0
                ? 0
                : totalMessages / duration.TotalMinutes;

            return new StatsResult
            {
                TotalSites = totalSites,
                OnlineOnce = onlineOnce,
                ActiveAlerts = activeAlerts,
                MessagesPerMinute = msgsPerMinute
            };
        }

        private MetricDto Compare(double current, double previous)
        {
            var diff = current - previous;

            return new MetricDto
            {
                Current = current,
                Previous = previous,
                Difference = diff,
                Percentage = previous == 0 ? 100 : Math.Round((diff / previous) * 100, 2)
            };
        }

        private (DateTime start, DateTime end) GetDateRange(TimeRange range)
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

    }
}
