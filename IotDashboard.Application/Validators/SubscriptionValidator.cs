using FluentValidation;
using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Validators
{
    public class SubscriptionDetailVMValidator : AbstractValidator<SubscriptionDetailVM>
    {
        public SubscriptionDetailVMValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("CustomerId is required");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .MaximumLength(50);

            RuleFor(x => x.StartsAt)
                .NotEmpty().WithMessage("StartsAt is required");

            RuleFor(x => x.EndsAt)
                .NotEmpty().WithMessage("EndsAt is required")
                .GreaterThanOrEqualTo(x => x.StartsAt)
                .WithMessage("EndsAt must be greater than or equal to StartsAt");

            RuleFor(x => x.Notes)
                .MaximumLength(1000);
        }
    }
}
