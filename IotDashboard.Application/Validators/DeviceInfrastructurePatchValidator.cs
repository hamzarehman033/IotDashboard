using FluentValidation;
using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Validators
{
    public class DeviceInfrastructurePatchValidator : AbstractValidator<DeviceInfrastructurePatchVM>
    {
        private static readonly string[] AllowedPowerSources = { "CP", "Battery", "Solar", "Generator" };

        public DeviceInfrastructurePatchValidator()
        {
            RuleForEach(x => x.PowerSources)
                .Must(x => AllowedPowerSources.Contains(x, StringComparer.OrdinalIgnoreCase))
                .When(x => x.PowerSources is not null)
                .WithMessage("Power source must be one of: CP, Battery, Solar, Generator");

            RuleFor(x => x.RectifierBrand)
                .MaximumLength(100).WithMessage("Rectifier brand must be at most 100 characters");

            RuleFor(x => x.RectifierQty)
                .GreaterThanOrEqualTo(0).When(x => x.RectifierQty.HasValue)
                .WithMessage("Rectifier quantity must be 0 or greater");

            RuleFor(x => x.RectifierCapacity)
                .MaximumLength(100).WithMessage("Rectifier capacity must be at most 100 characters");

            RuleFor(x => x.BatteryBrand)
                .MaximumLength(100).WithMessage("Battery brand must be at most 100 characters");

            RuleFor(x => x.BatteryQty)
                .GreaterThanOrEqualTo(0).When(x => x.BatteryQty.HasValue)
                .WithMessage("Battery quantity must be 0 or greater");

            RuleFor(x => x.BatteryCapacity)
                .MaximumLength(100).WithMessage("Battery capacity must be at most 100 characters");

            RuleFor(x => x.SolarBrand)
                .MaximumLength(100).WithMessage("Solar brand must be at most 100 characters");

            RuleFor(x => x.SolarQty)
                .GreaterThanOrEqualTo(0).When(x => x.SolarQty.HasValue)
                .WithMessage("Solar quantity must be 0 or greater");

            RuleFor(x => x.SolarCapacity)
                .MaximumLength(100).WithMessage("Solar capacity must be at most 100 characters");

            RuleFor(x => x.GeneratorBrand)
                .MaximumLength(100).WithMessage("Generator brand must be at most 100 characters");

            RuleFor(x => x.GeneratorQty)
                .GreaterThanOrEqualTo(0).When(x => x.GeneratorQty.HasValue)
                .WithMessage("Generator quantity must be 0 or greater");

            RuleFor(x => x.GeneratorCapacity)
                .MaximumLength(100).WithMessage("Generator capacity must be at most 100 characters");

            RuleFor(x => x.RmsSerialNumber)
                .MaximumLength(100).WithMessage("RMS serial number must be at most 100 characters");

            RuleFor(x => x.SimCardNumber)
                .MaximumLength(50).WithMessage("SIM card number must be at most 50 characters");

            RuleFor(x => x.CamerasInstalledCount)
                .GreaterThanOrEqualTo(0).When(x => x.CamerasInstalledCount.HasValue)
                .WithMessage("Cameras installed count must be 0 or greater");
        }
    }
}