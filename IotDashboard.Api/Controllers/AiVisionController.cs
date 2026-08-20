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
            [FromQuery] string? timeSpan = "1d",
            [FromQuery] int limit = 100,
            CancellationToken cancellationToken = default)
        {
            var res = await _aiVisionHandler.GetHistoryByDeviceAsync(
                deviceNumber,
                messageType,
                timeSpan,
                limit,
                cancellationToken);

            return res.ToResponse();
        }

        [HttpGet("packet/{id:long}/vision-packet-details")]
        public async Task<IActionResult> GetVisionPacketDetails(
            long id,
            CancellationToken cancellationToken = default)
        {
            var res = await _aiVisionHandler.GetVisionPacketDetails(id, cancellationToken);
            return res.ToResponse();
        }
    }
}
