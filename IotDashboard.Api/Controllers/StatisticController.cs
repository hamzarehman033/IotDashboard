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
        public async Task<IActionResult> GetStats(
            StatisticsFilterRequest request)
        {
            var result = await _statisticService.GetDashboardStats(request);
            return Ok(result);
        }
    }
}
