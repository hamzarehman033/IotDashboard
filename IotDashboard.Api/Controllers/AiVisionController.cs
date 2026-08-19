using IotDashboard.Api.Util;
using IotDashboard.Application.Handlers.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IotDashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AiVisionController : ControllerBase
    {
        private readonly IAiVisionHandler _aiVisionHandler;

        public AiVisionController(IAiVisionHandler aiVisionHandler)
        {
            _aiVisionHandler = aiVisionHandler;
        }

        [HttpGet("device/{deviceNumber:int}/history")]
        public async Task<IActionResult> GetHistoryByDevice(
            int deviceNumber,
            [FromQuery] byte? messageType,
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] int limit = 100,
            CancellationToken cancellationToken = default)
        {
            var res = await _aiVisionHandler.GetHistoryByDeviceAsync(
                deviceNumber,
                messageType,
                fromUtc,
                toUtc,
                limit,
                cancellationToken);

            return res.ToResponse();
        }
    }
}
