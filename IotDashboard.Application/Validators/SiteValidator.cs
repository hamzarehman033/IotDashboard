using FluentValidation;
using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Validators
{
    public class SiteVMValidator : AbstractValidator<SiteVM>
    {
        public SiteVMValidator()
        {
            RuleFor(x => x.RegionId)
                .GreaterThan(0).WithMessage("Region is required");

            RuleFor(x => x.SubRegionId)
                .GreaterThan(0).WithMessage("SubRegion is required");

            RuleFor(x => x.ZoneId)
                .GreaterThan(0).WithMessage("Zone is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .Length(1, 50).WithMessage("Code must be between 1 and 50 characters");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .MaximumLength(50).WithMessage("Status must be at most 50 characters");

            RuleFor(x => x.Coordinates)
                .NotEmpty().WithMessage("Coordinates are required")
                .MaximumLength(100).WithMessage("Coordinates must be at most 100 characters");
        }
    }
}
