using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Api.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IotDashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationController : BaseController<LocationVM>
    {
        private readonly ILocationHandler _locationHandler;

        public LocationController(ILocationHandler locationHandler) : base(locationHandler)
        {
            _locationHandler = locationHandler;
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetTree()
        {
            var res = await _locationHandler.GetTreeAsync();
            return res.ToResponse();
        }
    }
}
