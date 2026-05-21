using IotDashboard.Application.Dtos;
using IotDashboard.Application.Util;
using IotDashboard.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace IotDashboard.Application.Validators
{
    internal class CreateUserValidator : AbstractValidator<CreateUserVM>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateUserValidator(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;

            RuleFor(x => x.Email).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required")).CustomAsync(CheckEmail);
            RuleFor(x => x.Password).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required"));
            RuleFor(x => x.UserName).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required")).CustomAsync(CheckUserName);
            RuleFor(x => x.Role).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required"));
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
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                context.AddFailure(_httpContextAccessor.GetResourceString("validations.username.registerd"));
            }
        }
    }
}