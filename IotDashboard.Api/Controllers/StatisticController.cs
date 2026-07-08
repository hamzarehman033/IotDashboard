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

        public StatisticController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
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
            if (request.DeviceId.HasValue && request.DeviceId.Value <= 0)
            {
                return BadRequest("If provided, DeviceId must be greater than 0.");
            }

            var result = await _statisticService.GetEnergyConsumptionReport(request);
            return Ok(result);
        }
    }
}
