using IotDashboard.Application.Dtos;
using IotDashboard.Application.Util;
using IotDashboard.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDashboard.Application.Validators
{
    internal class RegisterValidator : AbstractValidator<RegisterVM>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RegisterValidator(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            RuleFor(x => x.Email).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required")).CustomAsync(CheckEmail);
            RuleFor(x => x.Password).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required"));
            RuleFor(x => x.UserName).NotEmpty().WithMessage(_httpContextAccessor.GetResourceString("validations.required")).CustomAsync(CheckUserName);
        }

        private async Task CheckEmail(string email, ValidationContext<RegisterVM> context,
            CancellationToken token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
                context.AddFailure(_httpContextAccessor.GetResourceString("validations.email.registerd"));
        }

        private async Task CheckUserName(string name, ValidationContext<RegisterVM> context,
           CancellationToken token)
        {
            var userExists = await _userManager.Users.AnyAsync(
                x => x.CustomerId == null && x.UserName == name,
                token);

            if (userExists)
                context.AddFailure(_httpContextAccessor.GetResourceString("validations.username.registerd"));
        }
    }
}
