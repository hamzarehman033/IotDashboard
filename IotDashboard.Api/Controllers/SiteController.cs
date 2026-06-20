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
        public SiteController(ISiteHandler siteHandler) : base(siteHandler)
        {
        }
    }
}
