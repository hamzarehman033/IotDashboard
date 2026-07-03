using FluentValidation;
using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Validators
{
    public class DeviceVMValidator : AbstractValidator<DeviceVM>
    {
        public DeviceVMValidator()
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

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required");

            RuleFor(x => x.Coordinates)
                .NotEmpty().WithMessage("Coordinates are required")
                .MaximumLength(100).WithMessage("Coordinates must be at most 100 characters");

            RuleFor(x => x.InstallationDate)
                .NotEmpty().WithMessage("Installation date is required");

            RuleFor(x => x.MqttHost)
                .NotEmpty().WithMessage("MQTT host is required")
                .MaximumLength(255).WithMessage("MQTT host must be at most 255 characters");

            RuleFor(x => x.MqttPort)
                .InclusiveBetween(1, 65535).WithMessage("MQTT port must be between 1 and 65535");

            RuleFor(x => x.MqttClientId)
                .NotEmpty().WithMessage("MQTT client ID is required")
                .MaximumLength(100).WithMessage("MQTT client ID must be at most 100 characters");

            RuleFor(x => x.MqttUsername)
                .MaximumLength(100).WithMessage("MQTT username must be at most 100 characters");

            RuleFor(x => x.MqttPassword)
                .NotEmpty().WithMessage("MQTT password is required")
                .MaximumLength(255).WithMessage("MQTT password must be at most 255 characters");

            RuleFor(x => x.KeepAliveSeconds)
                .InclusiveBetween(10, 3600).WithMessage("KeepAliveSeconds must be between 10 and 3600");

            RuleFor(x => x.RmsSubscribeTopic)
                .NotEmpty().WithMessage("RMS subscribe topic is required")
                .MaximumLength(255).WithMessage("RMS subscribe topic must be at most 255 characters");

            RuleFor(x => x.AiSubscribeTopic)
                .NotEmpty().WithMessage("AI subscribe topic is required")
                .MaximumLength(255).WithMessage("AI subscribe topic must be at most 255 characters");

            RuleFor(x => x.PublishTopic)
                .NotEmpty().WithMessage("Publish topic is required")
                .MaximumLength(255).WithMessage("Publish topic must be at most 255 characters");

            RuleFor(x => x.RectifierBrand)
                .MaximumLength(100).WithMessage("Rectifier brand must be at most 100 characters");

            RuleFor(x => x.RectifierQty)
                .GreaterThanOrEqualTo(0).WithMessage("Rectifier quantity must be 0 or greater");

            RuleFor(x => x.RectifierCapacity)
                .MaximumLength(100).WithMessage("Rectifier capacity must be at most 100 characters");

            RuleFor(x => x.BatteryBrand)
                .MaximumLength(100).WithMessage("Battery brand must be at most 100 characters");

            RuleFor(x => x.BatteryQty)
                .GreaterThanOrEqualTo(0).WithMessage("Battery quantity must be 0 or greater");

            RuleFor(x => x.BatteryCapacity)
                .MaximumLength(100).WithMessage("Battery capacity must be at most 100 characters");

            RuleFor(x => x.SolarBrand)
                .MaximumLength(100).WithMessage("Solar brand must be at most 100 characters");

            RuleFor(x => x.SolarQty)
                .GreaterThanOrEqualTo(0).WithMessage("Solar quantity must be 0 or greater");

            RuleFor(x => x.SolarCapacity)
                .MaximumLength(100).WithMessage("Solar capacity must be at most 100 characters");

            RuleFor(x => x.GeneratorBrand)
                .MaximumLength(100).WithMessage("Generator brand must be at most 100 characters");

            RuleFor(x => x.GeneratorQty)
                .GreaterThanOrEqualTo(0).WithMessage("Generator quantity must be 0 or greater");

            RuleFor(x => x.GeneratorCapacity)
                .MaximumLength(100).WithMessage("Generator capacity must be at most 100 characters");

            RuleFor(x => x.RmsSerialNumber)
                .MaximumLength(100).WithMessage("RMS serial number must be at most 100 characters");

            RuleFor(x => x.SimCardNumber)
                .MaximumLength(50).WithMessage("SIM card number must be at most 50 characters");

            RuleFor(x => x.CamerasInstalledCount)
                .GreaterThanOrEqualTo(0).WithMessage("Cameras installed count must be 0 or greater");
        }
    }
}
