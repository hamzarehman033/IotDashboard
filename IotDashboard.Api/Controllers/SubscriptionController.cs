using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using Microsoft.AspNetCore.Mvc;

namespace IotDashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : BaseController<SubscriptionDetailVM>
    {
        public SubscriptionController(ISubscriptionHandler subscriptionHandler)
            : base(subscriptionHandler)
        {
        }
    }
}
