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
        private readonly ISubscriptionRepository _subscriptionRepository;

        public CustomerHandler(
            ICustomerRepository customerRepository,
            ISubscriptionRepository subscriptionRepository,
            IValidator<CustomerDetailVM> validator,
            FilterValidator<CustomerDetailVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(customerRepository, CustomerMapper.Mapper, validator, filterValidator, httpContextAccessor)
        {
            _customerRepository = customerRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public override async Task<Response<CustomerDetailVM>> CreateAsync(CustomerDetailVM model)
        {
            var response = await base.CreateAsync(model);

            if (response.Status == _success && response.Data != null)
            {
                // Retrieve the created customer to get its ID
                var customer = await _customerRepository.GetByIdAsync(response.Data.Id);
                
                if (customer != null)
                {
                    // Create a default subscription for the new customer
                    var defaultSubscription = new Subscription
                    {
                        CustomerId = customer.Id,
                        Status = "Active",
                        StartsAt = DateTime.UtcNow,
                        EndsAt = DateTime.UtcNow.AddYears(1),
                        Notes = "Auto-created default subscription",
                        IsActive = true,
                        CreatedBy = customer.CreatedBy,
                        CreatedOn = DateTime.UtcNow
                    };

                    await _subscriptionRepository.CreateAsync(defaultSubscription);
                }
            }

            return response;
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
