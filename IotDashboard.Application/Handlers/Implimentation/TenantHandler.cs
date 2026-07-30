using FluentValidation;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Mappers;
using IotDashboard.Application.Validators;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Application.Handlers.Implimentation
{
    public class TenantHandler : BaseHandler<TenantVM, Tenant>, ITenantHandler
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IDeviceRepository _deviceRepository;

        public TenantHandler(
            ITenantRepository repo,
            IDeviceRepository deviceRepository,
            IValidator<TenantVM> validator,
            FilterValidator<TenantVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(repo, TenantMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
            _tenantRepository = repo;
            _deviceRepository = deviceRepository;
        }

        public override async Task<Response<TenantVM>> DeleteAsync(long Id)
        {
            var response = new Response<TenantVM> { Status = _error };

            var tenant = await _tenantRepository.GetByIdAsync(Id);
            if (tenant == null || !tenant.IsActive)
            {
                response.Message.Add("Tenant not found");
                return response;
            }

            var hasLinkedSites = await _deviceRepository
                .GetAllAsync()
                .IgnoreQueryFilters()
                .AnyAsync(x => x.DeviceTenants.Any(dt => dt.TenantId == Id));

            if (hasLinkedSites)
            {
                response.Message.Add("Cannot delete tenant because it is linked with sites.");
                return response;
            }

            return await base.DeleteAsync(Id);
        }
    }
}
