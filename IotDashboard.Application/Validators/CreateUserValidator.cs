using IotDashboard.Application.Dtos;
using IotDashboard.Application.Util;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Application.Validators
{
    internal class CreateUserValidator : AbstractValidator<CreateUserVM>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICustomerRepository _customerRepository;

        public CreateUserValidator(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, ICustomerRepository customerRepository)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _customerRepository = customerRepository;

            RuleFor(x => x.Email).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required")).CustomAsync(CheckEmail);
            RuleFor(x => x.Password).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required"));
            RuleFor(x => x.UserName).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required")).CustomAsync(CheckUserName);
            RuleFor(x => x.Role).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required"));
            RuleFor(x => x.CustomerId).CustomAsync(ValidateCustomerLinking);
            RuleFor(x => x.AssignedCustomerIds).CustomAsync(ValidateAssignedCustomers);
            RuleForEach(x => x.Modules)
                .GreaterThan(0)
                .WithMessage("Module ids must be greater than 0");
        }

        private async Task CheckEmail(string email, ValidationContext<CreateUserVM> context, CancellationToken token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                context.AddFailure(_httpContextAccessor.GetResourceString("validations.email.registerd"));
            }
        }

        private async Task CheckUserName(string userName, ValidationContext<CreateUserVM> context, CancellationToken token)
        {
            var customerId = context.InstanceToValidate.CustomerId;
            var userExists = await _userManager.Users.AnyAsync(
                x => x.CustomerId == customerId && x.UserName == userName,
                token);

            if (userExists)
            {
                context.AddFailure(_httpContextAccessor.GetResourceString("validations.username.registerd"));
            }
        }

        private async Task ValidateCustomerLinking(long? customerId, ValidationContext<CreateUserVM> context, CancellationToken token)
        {
            var role = context.InstanceToValidate.Role;
            
            // SysAdmin role does not require a customer link
            if (role?.Equals(RoleNames.SysAdmin, StringComparison.OrdinalIgnoreCase) == true)
            {
                return;
            }

            // All other roles MUST have a customer linked
            if (!customerId.HasValue || customerId.Value <= 0)
            {
                context.AddFailure("CustomerId", "CustomerId is required for non-SysAdmin users");
                return;
            }

            // Validate that the customer actually exists
            var customer = await _customerRepository.GetByIdAsync(customerId.Value);
            if (customer == null)
            {
                context.AddFailure("CustomerId", "Selected customer does not exist");
            }
        }

        private async Task ValidateAssignedCustomers(List<long> assignedCustomerIds, ValidationContext<CreateUserVM> context, CancellationToken token)
        {
            var role = context.InstanceToValidate.Role;
            if (role?.Equals(RoleNames.Manager, StringComparison.OrdinalIgnoreCase) != true)
            {
                return;
            }

            var distinctCustomerIds = assignedCustomerIds?
                .Where(x => x > 0)
                .Distinct()
                .ToList() ?? new List<long>();

            if (distinctCustomerIds.Count == 0)
            {
                context.AddFailure("AssignedCustomerIds", "Assigned Customers are required for Manager users");
                return;
            }

            var existingCustomerIds = await _customerRepository
                .GetAllAsync(x => distinctCustomerIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(token);

            var missingCustomerIds = distinctCustomerIds.Except(existingCustomerIds).ToList();
            if (missingCustomerIds.Count > 0)
            {
                context.AddFailure("AssignedCustomerIds", $"Assigned customer ids do not exist: {string.Join(", ", missingCustomerIds)}");
            }
        }
    }
}