using IotDashboard.Api.Services;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IotDashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticService _statisticService;
        private readonly IReportDownloadService _reportDownloadService;

        public StatisticController(
            IStatisticService statisticService,
            IReportDownloadService reportDownloadService)
        {
            _statisticService = statisticService;
            _reportDownloadService = reportDownloadService;
        }

        [AllowAnonymous]
        [HttpPost("dashboard-summary")]
        public async Task<IActionResult> GetSummary(
            DashboardSummaryRequest request)
        {
            var result = await _statisticService.GetSummary(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("weekly-alerts")]
        public async Task<IActionResult> GetWeeklyAlerts()
        {
            var result = await _statisticService.GetWeeklyAlerts();
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("recent-sites")]
        public async Task<IActionResult> GetRecentSites([FromBody] DashboardFilterRequest request)
        {
            var result = await _statisticService.GetRecentSites(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("TelemetryEnvironmentCounts")]
        public async Task<IActionResult> TelemetryEnvironmentCounts(
    EnvironmentStatsRequest request)
        {
            var result = await _statisticService.TelemetryEnvironmentCounts(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("TelemetryGetHourlyTempHumidityStats")]
        public async Task<IActionResult> TelemetryGetHourlyTempHumidityStats(
    [FromBody] HourlyEnvironmentRequest request)
        {
            var result = await _statisticService.TelemetryGetHourlyTempHumidityStats(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("GetTop5DevicesByActivityInLastHour")]
        public async Task<IActionResult> GetTopSensors()
        {
            var result = await _statisticService.GetTop5DevicesByActivityInLastHour();
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("GetRecentAnomalies")]
        public async Task<IActionResult> GetRecentAnomalies()
        {
            var result = await _statisticService.GetRecentAnomalies();
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("battery-status-report")]
        public async Task<IActionResult> GetBatteryStatusReport([FromBody] BatteryStatusReportRequest request)
        {
            if (request.TenantId.HasValue && request.TenantId.Value <= 0)
            {
                return BadRequest("If provided, TenantId must be greater than 0.");
            }

            if (request.DeviceId.HasValue && request.DeviceId.Value <= 0)
            {
                return BadRequest("If provided, DeviceId must be greater than 0.");
            }

            var result = await _statisticService.GetBatteryStatusReport(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("solar-status-report")]
        public async Task<IActionResult> GetSolarStatusReport([FromBody] SolarStatusReportRequest request)
        {
            if (request.TenantId.HasValue && request.TenantId.Value <= 0)
            {
                return BadRequest("If provided, TenantId must be greater than 0.");
            }

            if (request.DeviceId.HasValue && request.DeviceId.Value <= 0)
            {
                return BadRequest("If provided, DeviceId must be greater than 0.");
            }

            var result = await _statisticService.GetSolarStatusReport(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("grid-status-report")]
        public async Task<IActionResult> GetGridStatusReport([FromBody] GridStatusReportRequest request)
        {
            if (request.TenantId.HasValue && request.TenantId.Value <= 0)
            {
                return BadRequest("If provided, TenantId must be greater than 0.");
            }

            if (request.DeviceId.HasValue && request.DeviceId.Value <= 0)
            {
                return BadRequest("If provided, DeviceId must be greater than 0.");
            }

            var result = await _statisticService.GetGridStatusReport(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("alarm-status-report")]
        public async Task<IActionResult> GetAlarmStatusReport([FromBody] AlarmStatusReportRequest request)
        {
            if (request.TenantId.HasValue && request.TenantId.Value <= 0)
            {
                return BadRequest("If provided, TenantId must be greater than 0.");
            }

            if (request.DeviceId.HasValue && request.DeviceId.Value <= 0)
            {
                return BadRequest("If provided, DeviceId must be greater than 0.");
            }

            var result = await _statisticService.GetAlarmStatusReport(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("energy-consumption-report")]
        public async Task<IActionResult> GetEnergyConsumptionReport([FromBody] EnergyConsumptionReportRequest request)
        {
            if (request.TenantId.HasValue && request.TenantId.Value <= 0)
            {
                return BadRequest("If provided, TenantId must be greater than 0.");
            }

            if (request.DeviceId.HasValue && request.DeviceId.Value <= 0)
            {
                return BadRequest("If provided, DeviceId must be greater than 0.");
            }

            var result = await _statisticService.GetEnergyConsumptionReport(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("graphs/site-total-load")]
        public async Task<IActionResult> GetSiteTotalLoadGraph([FromBody] GraphRequest request)
        {
            var validation = ValidateGraphRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            try
            {
                var result = await _statisticService.GetSiteTotalLoadGraph(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("graphs/grid-voltage")]
        public async Task<IActionResult> GetGridVoltageGraph([FromBody] GraphRequest request)
        {
            var validation = ValidateGraphRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            try
            {
                var result = await _statisticService.GetGridVoltageGraph(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("graphs/tenant-load-trends")]
        public async Task<IActionResult> GetTenantLoadTrendsGraph([FromBody] GraphRequest request)
        {
            var validation = ValidateGraphRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            if (request.TenantId.HasValue && (request.TenantId.Value < 1 || request.TenantId.Value > 4))
            {
                return BadRequest("For tenant-load-trends, TenantId must be between 1 and 4.");
            }

            try
            {
                var result = await _statisticService.GetTenantLoadTrendsGraph(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("graphs/battery-soc")]
        public async Task<IActionResult> GetBatterySocGraph([FromBody] GraphRequest request)
        {
            var validation = ValidateGraphRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            try
            {
                var result = await _statisticService.GetBatterySocGraph(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("graphs/solar-yield")]
        public async Task<IActionResult> GetSolarYieldGraph([FromBody] GraphRequest request)
        {
            var validation = ValidateGraphRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            try
            {
                var result = await _statisticService.GetSolarYieldGraph(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("reports/download")]
        public async Task<IActionResult> DownloadReport([FromBody] ReportDownloadRequest request)
        {
            if (request.TenantId.HasValue && request.TenantId.Value <= 0)
            {
                return BadRequest("If provided, TenantId must be greater than 0.");
            }

            if (request.DeviceId.HasValue && request.DeviceId.Value <= 0)
            {
                return BadRequest("If provided, DeviceId must be greater than 0.");
            }

            try
            {
                var file = await _reportDownloadService.DownloadAsync(request);
                return File(file.Content, file.ContentType, file.FileName);
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private IActionResult? ValidateGraphRequest(GraphRequest request)
        {
            if (request is null)
            {
                return BadRequest("Request payload is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Timeframe))
            {
                return BadRequest("Timeframe is required and must be one of: 24h, 7days, 30days.");
            }

            if (request.DeviceId.HasValue && request.DeviceId.Value <= 0)
            {
                return BadRequest("If provided, DeviceId must be greater than 0.");
            }

            if (request.TenantId.HasValue && request.TenantId.Value <= 0)
            {
                return BadRequest("If provided, TenantId must be greater than 0.");
            }

            return null;
        }
    }
}
