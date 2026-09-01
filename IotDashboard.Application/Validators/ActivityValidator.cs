using FluentValidation;
using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Validators
{
    public class ActivityVMValidator : AbstractValidator<ActivityVM>
    {
        public ActivityVMValidator()
        {
            RuleFor(x => x.DeviceId)
                .GreaterThan(0).WithMessage("Device is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(1000).WithMessage("Description must be at most 1000 characters");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time");

            RuleFor(x => x.Team)
                .NotEmpty().WithMessage("Team is required")
                .MaximumLength(100).WithMessage("Team must be at most 100 characters");

            RuleFor(x => x.Persons)
                .GreaterThanOrEqualTo(0).WithMessage("Persons must be 0 or greater");
        }
    }
}
