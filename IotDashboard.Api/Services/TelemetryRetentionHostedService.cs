using IotDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IotDashboard.Api.Services
{
    public class TelemetryRetentionHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptionsMonitor<TelemetryRetentionOptions> _options;
        private readonly ILogger<TelemetryRetentionHostedService> _logger;

        public TelemetryRetentionHostedService(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<TelemetryRetentionOptions> options,
            ILogger<TelemetryRetentionHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Telemetry retention service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PurgeExpiredPacketsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Telemetry retention purge failed.");
                }

                var intervalHours = Math.Max(1, _options.CurrentValue.IntervalHours);
                try
                {
                    await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("Telemetry retention service stopped.");
        }

        private async Task PurgeExpiredPacketsAsync(CancellationToken cancellationToken)
        {
            var options = _options.CurrentValue;
            var retentionMonths = Math.Max(1, options.RetentionMonths);
            var batchSize = Math.Max(1, options.BatchSize);
            var cutoffUtc = DateTime.UtcNow.AddMonths(-retentionMonths);

            _logger.LogInformation(
                "Purging TelecomTelemetryPackets older than {CutoffUtc:o} (retention {RetentionMonths} months).",
                cutoffUtc,
                retentionMonths);

            var totalDeleted = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();

                var ids = await dbContext.TelecomTelemetryPackets
                    .AsNoTracking()
                    .Where(x => x.ReceivedAtUtc < cutoffUtc)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .Take(batchSize)
                    .ToListAsync(cancellationToken);

                if (ids.Count == 0)
                {
                    break;
                }

                var deleted = await dbContext.TelecomTelemetryPackets
                    .Where(x => ids.Contains(x.Id))
                    .ExecuteDeleteAsync(cancellationToken);

                totalDeleted += deleted;

                if (deleted == 0)
                {
                    break;
                }
            }

            _logger.LogInformation(
                "Telemetry retention purge completed. Deleted {DeletedCount} TelecomTelemetryPackets.",
                totalDeleted);
        }
    }
}
