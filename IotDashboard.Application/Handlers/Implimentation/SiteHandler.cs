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
    public class SiteHandler : BaseHandler<SiteVM, Site>, ISiteHandler
    {
        private readonly ISiteRepository _siteRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ICurrentUserService _currentUserService;

        public SiteHandler(
            ISiteRepository siteRepository,
            IDeviceRepository deviceRepository,
            ILocationRepository locationRepository,
            ICurrentUserService currentUserService,
            IValidator<SiteVM> validator,
            FilterValidator<SiteVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(siteRepository, SiteMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
            _siteRepository = siteRepository;
            _deviceRepository = deviceRepository;
            _locationRepository = locationRepository;
            _currentUserService = currentUserService;
        }

        public override async Task<Response<SiteVM>> CreateAsync(SiteVM model)
        {
            var customerId = _currentUserService.GetCustomerId();
            if (customerId <= 0)
            {
                return ErrorResponse("X-Customer-Id header is required");
            }
            return await base.CreateAsync(model);
        }

        public override async Task<Response<SiteVM>> UpdateAsync(long id, SiteVM model)
        {
            model.Id = id;
            var customerId = _currentUserService.GetCustomerId();
            if (customerId <= 0)
            {
                return ErrorResponse("X-Customer-Id header is required");
            }
            return await base.UpdateAsync(id, model);
        }

        public async Task<Response<List<SiteDeviceRowVM>>> GetCombinedAsync()
        {
            var response = new Response<List<SiteDeviceRowVM>> { Status = _error };

            var customerId = _currentUserService.GetCustomerId();
            if (customerId <= 0)
            {
                response.Message.Add("X-Customer-Id header is required");
                return response;
            }

            var sites = await _siteRepository.GetAllAsync()
                .OrderBy(x => x.Name)
                .ToListAsync();

            var devices = await _deviceRepository.GetAllAsync().ToListAsync();
            var deviceBySiteId = devices.ToDictionary(x => x.SiteId, x => x);
            var locations = await _locationRepository.GetAllAsync().ToListAsync();
            var locationById = locations.ToDictionary(x => x.Id, x => x.Name);

            response.Data = sites.Select(site =>
            {
                deviceBySiteId.TryGetValue(site.Id, out var device);
                return new SiteDeviceRowVM
                {
                    SiteId = site.Id,
                    SiteName = site.Name,
                    SiteCode = site.Code,
                    SiteStatus = site.Status,
                    RegionId = site.RegionId,
                    RegionName = locationById.TryGetValue(site.RegionId, out var regionName) ? regionName : string.Empty,
                    SubRegionId = site.SubRegionId,
                    SubRegionName = locationById.TryGetValue(site.SubRegionId, out var subRegionName) ? subRegionName : string.Empty,
                    ZoneId = site.ZoneId,
                    ZoneName = locationById.TryGetValue(site.ZoneId, out var zoneName) ? zoneName : string.Empty,
                    Address = site.Address,
                    Coordinates = site.Coordinates,
                    DeviceId = device?.Id,
                    DeviceName = device?.Name,
                    DeviceCode = device?.Code,
                    DeviceStatus = device?.Status,
                    DeviceInstallationDate = device?.InstallationDate,
                    MqttHost = device?.MqttHost,
                    MqttPort = device?.MqttPort,
                    MqttClientId = device?.MqttClientId,
                    UseTls = device?.UseTls,
                    KeepAliveSeconds = device?.KeepAliveSeconds,
                    RmsSubscribeTopic = device?.RmsSubscribeTopic,
                    AiSubscribeTopic = device?.AiSubscribeTopic,
                    RectifierBrand = device?.RectifierBrand,
                    RectifierQty = device?.RectifierQty,
                    RectifierCapacity = device?.RectifierCapacity,
                    BatteryBrand = device?.BatteryBrand,
                    BatteryQty = device?.BatteryQty,
                    BatteryCapacity = device?.BatteryCapacity,
                    SolarBrand = device?.SolarBrand,
                    SolarQty = device?.SolarQty,
                    SolarCapacity = device?.SolarCapacity,
                    GeneratorBrand = device?.GeneratorBrand,
                    GeneratorQty = device?.GeneratorQty,
                    GeneratorCapacity = device?.GeneratorCapacity,
                    RmsSerialNumber = device?.RmsSerialNumber,
                    SimCardNumber = device?.SimCardNumber,
                    CamerasInstalledCount = device?.CamerasInstalledCount,
                    AiEhsInstalled = device?.AiEhsInstalled,
                    AiSecurityInstalled = device?.AiSecurityInstalled
                };
            }).ToList();

            response.Status = _success;
            return response;
        }

        private Response<SiteVM> ErrorResponse(string message)
        {
            return new Response<SiteVM>
            {
                Status = _error,
                Message = new List<string> { message }
            };
        }
    }
}
