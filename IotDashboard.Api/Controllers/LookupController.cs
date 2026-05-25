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
    public class LookupController : BaseController<LookupVM>
    {
        private readonly ILookupHandler _handler;

        public LookupController(ILookupHandler handler) : base(handler)
        {
            _handler = handler;
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var result = await _handler.GetByCategory(category);
            return Ok(result);
        }
    }
}
