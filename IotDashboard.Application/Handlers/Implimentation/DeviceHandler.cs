using FluentValidation;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Mappers;
using IotDashboard.Application.Validators;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using IotDashboard.Infrastructure.AuditServices;
using IotDashboard.Infrastructure.ExternalServices.Mqtt;
using IotDashboard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using FluentValidation.Results;
using IotDashboard.Application.Util;

namespace IotDashboard.Application.Handlers.Implimentation
{
    public class DeviceHandler : BaseHandler<DeviceVM, Device>, IDeviceHandler
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IMqttClientService _mqttClientService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DeviceInfrastructurePatchVM> _infrastructurePatchValidator;
        private readonly AppDBContext _dbContext;

        public DeviceHandler(
            IDeviceRepository deviceRepository,
            ILocationRepository locationRepository,
            IMqttClientService mqttClientService,
            ICurrentUserService currentUserService,
            AppDBContext dbContext,
            IValidator<DeviceVM> validator,
            IValidator<DeviceInfrastructurePatchVM> infrastructurePatchValidator,
            FilterValidator<DeviceVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(deviceRepository, DeviceMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
            _deviceRepository = deviceRepository;
            _locationRepository = locationRepository;
            _mqttClientService = mqttClientService;
            _currentUserService = currentUserService;
            _dbContext = dbContext;
            _infrastructurePatchValidator = infrastructurePatchValidator;
        }

        public override async Task<Response<PagerModel<DeviceVM>>> GetAllAsync(int pageSize = 10, int currentPage = 1, IEnumerable<FilterVM> filters = null)
        {
            var response = await base.GetAllAsync(pageSize, currentPage, filters);

            if (response.Data?.PageData == null || !response.Data.PageData.Any())
            {
                return response;
            }

            var locationIds = response.Data.PageData
                .SelectMany(x => new[] { x.RegionId, x.SubRegionId, x.ZoneId })
                .Distinct()
                .ToList();

            var locationById = await _locationRepository.GetAllAsync()
                .Where(x => locationIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            foreach (var device in response.Data.PageData)
            {
                device.RegionName = locationById.TryGetValue(device.RegionId, out var regionName)
                    ? regionName
                    : string.Empty;
                device.SubRegionName = locationById.TryGetValue(device.SubRegionId, out var subRegionName)
                    ? subRegionName
                    : string.Empty;
                device.ZoneName = locationById.TryGetValue(device.ZoneId, out var zoneName)
                    ? zoneName
                    : string.Empty;
            }

            var deviceIds = response.Data.PageData.Select(x => x.Id).ToList();
            var tenantLinks = await _deviceRepository
                .GetAllAsync()
                .IgnoreQueryFilters()
                .Where(x => deviceIds.Contains(x.Id))
                .SelectMany(x => x.DeviceTenants.Select(dt => new { x.Id, dt.TenantId }))
                .ToListAsync();

            var tenantMap = tenantLinks
                .GroupBy(x => x.Id)
                .ToDictionary(x => x.Key, x => x.Select(y => y.TenantId).Distinct().ToList());

            foreach (var device in response.Data.PageData)
            {
                device.TenantIds = tenantMap.TryGetValue(device.Id, out var tenants)
                    ? tenants
                    : new List<long>();
            }

            return response;
        }

        public override async Task<Response<DeviceVM>> GetByIdAsync(long Id)
        {
            var response = await base.GetByIdAsync(Id);

            if (response.Data == null)
            {
                return response;
            }

            var locationIds = new[]
            {
                response.Data.RegionId,
                response.Data.SubRegionId,
                response.Data.ZoneId
            }
            .Distinct()
            .ToList();

            var locationById = await _locationRepository.GetAllAsync()
                .Where(x => locationIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            response.Data.RegionName = locationById.TryGetValue(response.Data.RegionId, out var regionName)
                ? regionName
                : string.Empty;
            response.Data.SubRegionName = locationById.TryGetValue(response.Data.SubRegionId, out var subRegionName)
                ? subRegionName
                : string.Empty;
            response.Data.ZoneName = locationById.TryGetValue(response.Data.ZoneId, out var zoneName)
                ? zoneName
                : string.Empty;

            response.Data.TenantIds = await _deviceRepository
                .GetAllAsync()
                .IgnoreQueryFilters()
                .Where(x => x.Id == Id)
                .SelectMany(x => x.DeviceTenants.Select(dt => dt.TenantId))
                .Distinct()
                .ToListAsync();

            return response;
        }

        public override async Task<Response<DeviceVM>> CreateAsync(DeviceVM model)
        {
            var customerId = _currentUserService.GetCustomerId();
            if (customerId <= 0)
            {
                return ErrorResponse("X-Customer-Id header is required");
            }

            var response = await base.CreateAsync(model);
            if (response.Data == null)
            {
                return response;
            }

            var syncResult = await SyncDeviceTenantsAsync(response.Data.Id, model.TenantIds);
            if (!string.IsNullOrEmpty(syncResult))
            {
                return ErrorResponse(syncResult);
            }

            response.Data.TenantIds = await GetDeviceTenantIdsAsync(response.Data.Id);
            return response;
        }

        public override async Task<Response<DeviceVM>> UpdateAsync(long id, DeviceVM model)
        {
            model.Id = id;
            var customerId = _currentUserService.GetCustomerId();
            if (customerId <= 0)
            {
                return ErrorResponse("X-Customer-Id header is required");
            }

            var response = await base.UpdateAsync(id, model);
            if (response.Data == null)
            {
                return response;
            }

            var syncResult = await SyncDeviceTenantsAsync(id, model.TenantIds);
            if (!string.IsNullOrEmpty(syncResult))
            {
                return ErrorResponse(syncResult);
            }

            response.Data.TenantIds = await GetDeviceTenantIdsAsync(id);
            return response;
        }

        public async Task<Response<DeviceVM>> PatchInfrastructureByDeviceIdAsync(long deviceId, DeviceInfrastructurePatchVM model)
        {
            var customerId = _currentUserService.GetCustomerId();
            if (customerId <= 0)
            {
                return ErrorResponse("X-Customer-Id header is required");
            }

            ValidationResult validationResult = await _infrastructurePatchValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                return new Response<DeviceVM>
                {
                    Status = _error,
                    Message = validationResult.ToErrorMessage()
                };
            }

            var device = await _deviceRepository
                .GetAllAsync()
                .FirstOrDefaultAsync(x => x.Id == deviceId);

            if (device == null)
            {
                return ErrorResponse("Device not found for the provided device id");
            }

            if (model.PowerSources is not null)
            {
                device.PowerSources = model.PowerSources
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            if (model.RectifierBrand is not null) device.RectifierBrand = model.RectifierBrand;
            if (model.RectifierQty.HasValue) device.RectifierQty = model.RectifierQty.Value;
            if (model.RectifierCapacity is not null) device.RectifierCapacity = model.RectifierCapacity;
            if (model.BatteryBrand is not null) device.BatteryBrand = model.BatteryBrand;
            if (model.BatteryQty.HasValue) device.BatteryQty = model.BatteryQty.Value;
            if (model.BatteryCapacity is not null) device.BatteryCapacity = model.BatteryCapacity;
            if (model.SolarBrand is not null) device.SolarBrand = model.SolarBrand;
            if (model.SolarQty.HasValue) device.SolarQty = model.SolarQty.Value;
            if (model.SolarCapacity is not null) device.SolarCapacity = model.SolarCapacity;
            if (model.GeneratorBrand is not null) device.GeneratorBrand = model.GeneratorBrand;
            if (model.GeneratorQty.HasValue) device.GeneratorQty = model.GeneratorQty.Value;
            if (model.GeneratorCapacity is not null) device.GeneratorCapacity = model.GeneratorCapacity;
            if (model.RmsSerialNumber is not null) device.RmsSerialNumber = model.RmsSerialNumber;
            if (model.SimCardNumber is not null) device.SimCardNumber = model.SimCardNumber;
            if (model.CamerasInstalledCount.HasValue) device.CamerasInstalledCount = model.CamerasInstalledCount.Value;
            if (model.AiEhsInstalled.HasValue) device.AiEhsInstalled = model.AiEhsInstalled.Value;
            if (model.AiSecurityInstalled.HasValue) device.AiSecurityInstalled = model.AiSecurityInstalled.Value;

            var updated = await _deviceRepository.UpdateAsync(device);

            return new Response<DeviceVM>
            {
                Status = _success,
                Data = DeviceMapper.Mapper.Value.Map<DeviceVM>(updated)
            };
        }

        public async Task<Response<bool>> SubscribeMqttAsync(long deviceId)
        {
            var response = new Response<bool> { Status = _error };
            var customerId = _currentUserService.GetCustomerId();
            if (customerId <= 0)
            {
                response.Message.Add("X-Customer-Id header is required");
                return response;
            }

            var device = await _deviceRepository
                .GetAllAsync()
                .FirstOrDefaultAsync(x => x.Id == deviceId);

            if (device == null)
            {
                response.Message.Add("Device not found for the provided device id");
                return response;
            }

            if (string.IsNullOrWhiteSpace(device.MqttHost) || string.IsNullOrWhiteSpace(device.MqttClientId))
            {
                response.Message.Add("Device MQTT configuration is incomplete");
                return response;
            }

            var topics = GetConfiguredTopics(device);
            if (topics.Count == 0)
            {
                response.Message.Add("No MQTT topics configured for this device");
                return response;
            }

            await _mqttClientService.ConnectAsync(
                (int)device.Id,
                device.MqttHost,
                device.MqttPort,
                device.MqttClientId,
                device.MqttUsername,
                device.MqttPassword,
                device.UseTls,
                device.KeepAliveSeconds);

            await _mqttClientService.SubscribeToTopicsAsync((int)device.Id, topics.ToArray());

            response.Status = _success;
            response.Data = true;
            response.Message.Add("Device subscribed to MQTT topics successfully");
            return response;
        }

        public async Task<Response<bool>> UnsubscribeMqttAsync(long deviceId)
        {
            var response = new Response<bool> { Status = _error };
            var customerId = _currentUserService.GetCustomerId();
            if (customerId <= 0)
            {
                response.Message.Add("X-Customer-Id header is required");
                return response;
            }

            var device = await _deviceRepository
                .GetAllAsync()
                .FirstOrDefaultAsync(x => x.Id == deviceId);

            if (device == null)
            {
                response.Message.Add("Device not found for the provided device id");
                return response;
            }

            var topics = GetConfiguredTopics(device);
            if (topics.Count > 0)
            {
                await _mqttClientService.UnsubscribeFromTopicsAsync((int)device.Id, topics.ToArray());
            }

            await _mqttClientService.DisconnectAsync((int)device.Id);

            response.Status = _success;
            response.Data = true;
            response.Message.Add("Device unsubscribed from MQTT topics successfully");
            return response;
        }

        private static List<string> GetConfiguredTopics(Device device)
        {
            return new[] { device.RmsSubscribeTopic, device.AiSubscribeTopic }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private async Task<List<long>> GetDeviceTenantIdsAsync(long deviceId)
        {
            return await _deviceRepository
                .GetAllAsync()
                .IgnoreQueryFilters()
                .Where(x => x.Id == deviceId)
                .SelectMany(x => x.DeviceTenants.Select(dt => dt.TenantId))
                .Distinct()
                .ToListAsync();
        }

        private async Task<string?> SyncDeviceTenantsAsync(long deviceId, IEnumerable<long>? tenantIds)
        {
            var customerId = _currentUserService.GetCustomerId();
            var requestedTenantIds = (tenantIds ?? Enumerable.Empty<long>())
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var existingTenantIds = await _deviceRepository
                .GetAllAsync()
                .IgnoreQueryFilters()
                .Where(x => x.Id == deviceId)
                .SelectMany(x => x.DeviceTenants.Select(dt => dt.TenantId))
                .Distinct()
                .ToListAsync();

            var matchedTenantIds = await _dbContext.Tenants
                .Where(x => x.CustomerId == customerId && requestedTenantIds.Contains(x.Id) && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync();

            if (matchedTenantIds.Count != requestedTenantIds.Count)
            {
                return "One or more tenant ids are invalid for this customer";
            }

            var toRemove = existingTenantIds.Except(requestedTenantIds).ToList();
            var toAdd = requestedTenantIds.Except(existingTenantIds).ToList();

            if (toRemove.Count > 0)
            {
                var removeLinks = await _dbContext.DeviceTenants
                    .Where(x => x.DeviceId == deviceId && toRemove.Contains(x.TenantId))
                    .ToListAsync();
                _dbContext.DeviceTenants.RemoveRange(removeLinks);
            }

            if (toAdd.Count > 0)
            {
                var addLinks = toAdd.Select(tenantId => new DeviceTenant
                {
                    DeviceId = deviceId,
                    TenantId = tenantId
                });

                await _dbContext.DeviceTenants.AddRangeAsync(addLinks);
            }

            if (toRemove.Count > 0 || toAdd.Count > 0)
            {
                await _dbContext.SaveChangesAsync();
            }

            return null;
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
