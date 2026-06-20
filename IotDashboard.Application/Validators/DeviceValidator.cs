using FluentValidation;
using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Validators
{
    public class DeviceVMValidator : AbstractValidator<DeviceVM>
    {
        public DeviceVMValidator()
        {
            RuleFor(x => x.SiteId)
                .GreaterThan(0).WithMessage("Site is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .Length(1, 50).WithMessage("Code must be between 1 and 50 characters");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .MaximumLength(50).WithMessage("Status must be at most 50 characters");

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
        }
    }
}
