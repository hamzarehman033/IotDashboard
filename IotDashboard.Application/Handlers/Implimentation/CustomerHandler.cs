using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Util;
using IotDashboard.Application.Validators;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IotDashboard.Application.Mappers;

namespace IotDashboard.Application.Handlers.Implimentation
{
    public class CustomerHandler : BaseHandler<CustomerDetailVM, Customer>, ICustomerHandler
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerHandler(
            ICustomerRepository customerRepository,
            IValidator<CustomerDetailVM> validator,
            FilterValidator<CustomerDetailVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(customerRepository, CustomerMapper.Mapper, validator, filterValidator, httpContextAccessor)
        {
            _customerRepository = customerRepository;
        }
     
        public async Task<Response<bool>> SetSubscriptionStatusAsync(long customerId, bool isActive)
        {
            Response<bool> response = new Response<bool> { Status = _error };
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
            {
                response.Message.Add("Customer not found");
                return response;
            }

            customer.SubscriptionActive = isActive;
            customer.Status = isActive ? "Active" : "Inactive";

            await _customerRepository.UpdateAsync(customer);

            response.Status = _success;
            response.Data = true;
            return response;
        }

        public async Task<Response<bool>> DeactivateCustomerAsync(long customerId)
        {
            Response<bool> response = new Response<bool> { Status = _error };
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
            {
                response.Message.Add("Customer not found");
                return response;
            }

            customer.IsActive = false;
            customer.SubscriptionActive = false;
            customer.Status = "Inactive";

            await _customerRepository.UpdateAsync(customer);

            response.Status = _success;
            response.Data = true;
            return response;
        }
    }
}
