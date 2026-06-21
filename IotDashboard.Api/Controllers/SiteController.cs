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
    public class SiteController : BaseController<SiteVM>
    {
        private readonly ISiteHandler _siteHandler;

        public SiteController(ISiteHandler siteHandler) : base(siteHandler)
        {
            _siteHandler = siteHandler;
        }

        [HttpGet("combined")]
        public async Task<IActionResult> GetCombined()
        {
            var res = await _siteHandler.GetCombinedAsync();
            return res.ToResponse();
        }
    }
}
