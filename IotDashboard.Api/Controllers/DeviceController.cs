using IotDashboard.Api.Util;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IotDashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeviceController : BaseController<DeviceVM>
    {
        private readonly IDeviceHandler _deviceHandler;

        public DeviceController(IDeviceHandler deviceHandler) : base(deviceHandler)
        {
            _deviceHandler = deviceHandler;
        }

        [HttpPost("{deviceId}/mqtt/subscribe")]
        public async Task<IActionResult> SubscribeMqtt(long deviceId)
        {
            var res = await _deviceHandler.SubscribeMqttAsync(deviceId);
            return res.ToResponse();
        }

        [HttpPost("{deviceId}/mqtt/unsubscribe")]
        public async Task<IActionResult> UnsubscribeMqtt(long deviceId)
        {
            var res = await _deviceHandler.UnsubscribeMqttAsync(deviceId);
            return res.ToResponse();
        }

        [HttpPatch("{deviceId}/infrastructure")]
        public async Task<IActionResult> PatchInfrastructure(long deviceId, [FromBody] DeviceInfrastructurePatchVM model)
        {
            var res = await _deviceHandler.PatchInfrastructureByDeviceIdAsync(deviceId, model);
            return res.ToResponse();
        }
    }
}
