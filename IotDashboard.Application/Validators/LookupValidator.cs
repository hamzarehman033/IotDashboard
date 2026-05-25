using IotDashboard.Application.Dtos;
using FluentValidation;

namespace IotDashboard.Application.Validators
{
    public class LookupVMValidator : AbstractValidator<LookupVM>
    {
        public LookupVMValidator()
        {
            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .Length(2, 50).WithMessage("Category must be between 2 and 50 characters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(1, 100).WithMessage("Name must be between 1 and 100 characters");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Value is required");

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0).WithMessage("Order must be 0 or greater");
        }
    }
}
