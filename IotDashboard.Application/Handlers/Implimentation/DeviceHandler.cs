using FluentValidation;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Mappers;
using IotDashboard.Application.Validators;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using IotDashboard.Infrastructure.AuditServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Application.Handlers.Implimentation
{
    public class DeviceHandler : BaseHandler<DeviceVM, Device>, IDeviceHandler
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeviceHandler(
            IDeviceRepository deviceRepository,
            ISiteRepository siteRepository,
            ICurrentUserService currentUserService,
            IValidator<DeviceVM> validator,
            FilterValidator<DeviceVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(deviceRepository, DeviceMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
            _deviceRepository = deviceRepository;
            _siteRepository = siteRepository;
            _currentUserService = currentUserService;
        }

        public override async Task<Response<DeviceVM>> CreateAsync(DeviceVM model)
        {
            var customerId = _currentUserService.GetCustomerId();
            if (customerId <= 0)
            {
                return ErrorResponse("X-Customer-Id header is required");
            }

            return await base.CreateAsync(model);
        }

        public override async Task<Response<DeviceVM>> UpdateAsync(long id, DeviceVM model)
        {
            model.Id = id;
            var customerId = _currentUserService.GetCustomerId();
            if (customerId <= 0)
            {
                return ErrorResponse("X-Customer-Id header is required");
            }

            return await base.UpdateAsync(id, model);
        }

        private Response<DeviceVM> ErrorResponse(string message)
        {
            return new Response<DeviceVM>
            {
                Status = _error,
                Message = new List<string> { message }
            };
        }
    }
}
