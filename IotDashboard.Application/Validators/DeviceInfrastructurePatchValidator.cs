using FluentValidation;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Dtos.Configs;
using Microsoft.Extensions.Options;

namespace IotDashboard.Application.Validators
{
    public class DeviceInfrastructurePatchValidator : AbstractValidator<DeviceInfrastructurePatchVM>
    {
        private static readonly string[] AllowedPowerSources = { "CP", "Battery", "Solar", "Generator" };

        public DeviceInfrastructurePatchValidator(IOptions<CameraConfigs> cameraOptions)
        {
            var maxCameras = cameraOptions.Value.EffectiveMaxPerDevice;

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
                .GreaterThanOrEqualTo(0).WithMessage("Cameras installed count must be 0 or greater")
                .LessThanOrEqualTo(maxCameras).WithMessage($"A device can have at most {maxCameras} cameras")
                .When(x => x.CamerasInstalledCount.HasValue);

            RuleFor(x => x.Cameras!.Count)
                .LessThanOrEqualTo(maxCameras).When(x => x.Cameras is not null)
                .WithMessage($"A device can have at most {maxCameras} cameras");

            RuleForEach(x => x.Cameras)
                .NotNull().WithMessage("Camera entries must not be empty")
                .When(x => x.Cameras is not null);

            RuleFor(x => x.Cameras)
                .Must(cameras =>
                {
                    var indexes = cameras!.Where(c => c is not null).Select(c => c.CameraIndex).ToList();
                    return indexes.Distinct().Count() == indexes.Count;
                })
                .When(x => x.Cameras is not null)
                .WithMessage("Each camera must have a unique camera index");

            RuleForEach(x => x.Cameras).ChildRules(camera =>
            {
                camera.RuleFor(c => (int)c.CameraIndex)
                    .InclusiveBetween(1, maxCameras)
                    .WithMessage($"Camera index must be between 1 and {maxCameras}");

                camera.RuleFor(c => c.Name)
                    .MaximumLength(100).WithMessage("Camera name must be at most 100 characters");
            }).When(x => x.Cameras is not null);
        }
    }
}