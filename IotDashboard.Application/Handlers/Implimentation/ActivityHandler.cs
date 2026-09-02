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
        private readonly IActivityDeviceNotifier _activityDeviceNotifier;

        public ActivityHandler(
            IActivityRepository repo,
            IDeviceRepository deviceRepository,
            IActivityDeviceNotifier activityDeviceNotifier,
            IValidator<ActivityVM> validator,
            FilterValidator<ActivityVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(repo, ActivityMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
            _deviceRepository = deviceRepository;
            _activityDeviceNotifier = activityDeviceNotifier;
        }

        public override async Task<Response<ActivityVM>> CreateAsync(ActivityVM model)
        {
            var deviceError = await ValidateDeviceAsync(model.DeviceId);
            if (deviceError != null)
            {
                return deviceError;
            }

            var response = await base.CreateAsync(model);
            if (response.Status == _success && response.Data != null)
            {
                await _activityDeviceNotifier.NotifyAddedAsync(response.Data);
            }

            return response;
        }

        public override async Task<Response<ActivityVM>> UpdateAsync(long id, ActivityVM model)
        {
            var deviceError = await ValidateDeviceAsync(model.DeviceId);
            if (deviceError != null)
            {
                return deviceError;
            }

            var response = await base.UpdateAsync(id, model);
            if (response.Status == _success && response.Data != null)
            {
                await _activityDeviceNotifier.NotifyUpdatedAsync(response.Data);
            }

            return response;
        }

        public override async Task<Response<ActivityVM>> DeleteAsync(long Id)
        {
            var existing = await _repo.GetByIdAsync(Id);
            var response = await base.DeleteAsync(Id);
            if (response.Status == _success && existing != null)
            {
                await _activityDeviceNotifier.NotifyDeletedAsync(Id, existing.DeviceId);
            }

            return response;
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
