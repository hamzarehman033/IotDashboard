using IotDashboard.Application.Dtos;
using FluentValidation;

namespace IotDashboard.Application.Validators
{
    public class LocationVMValidator : AbstractValidator<LocationVM>
    {
        public LocationVMValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .Length(1, 50).WithMessage("Code must be between 1 and 50 characters");

            RuleFor(x => x.Level)
                .InclusiveBetween(1, 3).WithMessage("Level must be 1 (Region), 2 (SubRegion), or 3 (Zone)");

            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("CustomerId is required");
        }
    }
}
