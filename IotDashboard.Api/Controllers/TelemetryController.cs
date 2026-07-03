using IotDashboard.Api.Util;
using IotDashboard.Application.Handlers.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IotDashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryHandler _telemetryHandler;

        public TelemetryController(ITelemetryHandler telemetryHandler)
        {
            _telemetryHandler = telemetryHandler;
        }

        [HttpGet("device/{deviceId}/latest")]
        public async Task<IActionResult> GetLatestByDevice(string deviceId, CancellationToken cancellationToken)
        {
            var res = await _telemetryHandler.GetLatestByDeviceAsync(deviceId, cancellationToken);
            return res.ToResponse();
        }

        [HttpGet("device/{deviceId}/history")]
        public async Task<IActionResult> GetHistoryByDevice(
            string deviceId,
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] int limit = 100,
            CancellationToken cancellationToken = default)
        {
            var res = await _telemetryHandler.GetHistoryByDeviceAsync(
                deviceId,
                fromUtc,
                toUtc,
                limit,
                cancellationToken);

            return res.ToResponse();
        }

    }
}
