using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Util;
using IotDashboard.Application.Validators;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            model.Slug = await GenerateUniqueSlugAsync(model.Name);
            model.Status = "Active";
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

        public override async Task<Response<CustomerDetailVM>> UpdateAsync(long Id, CustomerDetailVM model)
        {
            if (string.IsNullOrEmpty(model.Slug))
            {
                var existingSlug = await _customerRepository
                    .GetAllAsync(x => x.Id == Id)
                    .Select(x => x.Slug)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(existingSlug))
                {
                    model.Slug = await GenerateUniqueSlugAsync(model.Name);
                }
            }

            return await base.UpdateAsync(Id, model);
        }

        private async Task<string> GenerateUniqueSlugAsync(string name)
        {
            var baseSlug = ToSlug(name);
            var slug = baseSlug;
            var suffix = 1;

            while (await _customerRepository.GetAllAsync(x => x.Slug == slug).AnyAsync())
            {
                slug = $"{baseSlug}-{suffix}";
                suffix++;
            }

            return slug;
        }

        private static string ToSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "customer";

            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var ch in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(ch);
                }
            }

            var cleaned = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            cleaned = Regex.Replace(cleaned, "[^a-z0-9\\s-]", "");
            cleaned = Regex.Replace(cleaned, "\\s+", "-");
            cleaned = Regex.Replace(cleaned, "-+", "-").Trim('-');

            return string.IsNullOrWhiteSpace(cleaned) ? "customer" : cleaned;
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
