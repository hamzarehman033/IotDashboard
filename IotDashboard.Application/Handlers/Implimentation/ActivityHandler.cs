using FluentValidation;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Mappers;
using IotDashboard.Application.Validators;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IotDashboard.Application.Handlers.Implimentation
{
    public class ActivityHandler : BaseHandler<ActivityVM, Activity>, IActivityHandler
    {
        private readonly IDeviceRepository _deviceRepository;

        public ActivityHandler(
            IActivityRepository repo,
            IDeviceRepository deviceRepository,
            IValidator<ActivityVM> validator,
            FilterValidator<ActivityVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(repo, ActivityMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
            _deviceRepository = deviceRepository;
        }

        public override async Task<Response<ActivityVM>> CreateAsync(ActivityVM model)
        {
            var deviceError = await ValidateDeviceAsync(model.DeviceId);
            if (deviceError != null)
            {
                return deviceError;
            }

            return await base.CreateAsync(model);
        }

        public override async Task<Response<ActivityVM>> UpdateAsync(long id, ActivityVM model)
        {
            var deviceError = await ValidateDeviceAsync(model.DeviceId);
            if (deviceError != null)
            {
                return deviceError;
            }

            return await base.UpdateAsync(id, model);
        }

        private async Task<Response<ActivityVM>?> ValidateDeviceAsync(long deviceId)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null || !device.IsActive)
            {
                return new Response<ActivityVM>
                {
                    Status = _error,
                    Message = { "Device not found" }
                };
            }

            return null;
        }
    }
}
