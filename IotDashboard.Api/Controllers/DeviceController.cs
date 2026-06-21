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

        [HttpPatch("{deviceId}/infrastructure")]
        public async Task<IActionResult> PatchInfrastructure(long deviceId, [FromBody] DeviceInfrastructurePatchVM model)
        {
            var res = await _deviceHandler.PatchInfrastructureByDeviceIdAsync(deviceId, model);
            return res.ToResponse();
        }
    }
}
