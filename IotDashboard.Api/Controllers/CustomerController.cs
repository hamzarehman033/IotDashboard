using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Api.Util;
using IotDashboard.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IotDashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : BaseController<CustomerDetailVM>
    {
        private readonly ICustomerHandler _customerHandler;

        public CustomerController(ICustomerHandler customerHandler) : base(customerHandler)
        {
            _customerHandler = customerHandler;
        }

        [Authorize(Roles = RoleNames.SysAdmin)]
        [HttpPut("{id}/subscription/{isActive}")]
        public async Task<IActionResult> SetSubscription(long id, bool isActive)
        {
            var data = await _customerHandler.SetSubscriptionStatusAsync(id, isActive);
            return data.ToResponse();
        }

        [Authorize(Roles = RoleNames.SysAdmin)]
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateCustomer(long id)
        {
            var data = await _customerHandler.DeactivateCustomerAsync(id);
            return data.ToResponse();
        }
    }
}
