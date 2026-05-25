using FluentValidation;
using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Validators
{
    public class CustomerDetailVMValidator : AbstractValidator<CustomerDetailVM>
    {
        public CustomerDetailVMValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Customer name is required")
                .MaximumLength(100);

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required")
                .Matches("^[a-z0-9-]+$")
                .WithMessage("Slug can only contain lowercase letters, numbers and hyphens")
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .MaximumLength(50);
        }
    }
}
