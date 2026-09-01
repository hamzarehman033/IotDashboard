using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IotDashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivityController : BaseController<ActivityVM>
    {
        public ActivityController(IActivityHandler activityHandler) : base(activityHandler)
        {
        }
    }
}
