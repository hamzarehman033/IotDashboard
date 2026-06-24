using IotDashboard.Api.Util;
using IotDashboard.Api.Services;
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
        private readonly ITelemetryPersistenceService _telemetryPersistenceService;

        public SiteController(
            ISiteHandler siteHandler,
            ITelemetryPersistenceService telemetryPersistenceService) : base(siteHandler)
        {
            _siteHandler = siteHandler;
            _telemetryPersistenceService = telemetryPersistenceService;
        }

        [HttpGet("combined")]
        public async Task<IActionResult> GetCombined()
        {
            var res = await _siteHandler.GetCombinedAsync();
            return res.ToResponse();
        }

        [HttpGet("{siteId}/latest-status")]
        public async Task<IActionResult> GetLatestStatusBySite(string siteId, CancellationToken cancellationToken)
        {
            var data = await _telemetryPersistenceService.GetLatestBySiteAsync(siteId, cancellationToken);
            return Ok(data);
        }
    }
}
