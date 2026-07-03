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
    }
}
