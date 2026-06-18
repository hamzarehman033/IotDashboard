using FluentValidation;
using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Validators
{
    public class TenantVMValidator : AbstractValidator<TenantVM>
    {
        public TenantVMValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .Length(1, 50).WithMessage("Code must be between 1 and 50 characters");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .MaximumLength(50);
        }
    }
}