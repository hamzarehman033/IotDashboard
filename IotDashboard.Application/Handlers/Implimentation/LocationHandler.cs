using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Mappers;
using IotDashboard.Application.Validators;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using IotDashboard.Infrastructure.AuditServices;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace IotDashboard.Application.Handlers.Implimentation
{
    public class LocationHandler : BaseHandler<LocationVM, Location>, ILocationHandler
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ICurrentUserService _currentUserService;

        public LocationHandler(
            ILocationRepository locationRepository,
            ICurrentUserService currentUserService,
            IValidator<LocationVM> validator,
            FilterValidator<LocationVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(locationRepository, LocationMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
            _locationRepository = locationRepository;
            _currentUserService = currentUserService;
        }

        public async Task<Response<List<LocationTreeVM>>> GetTreeAsync()
        {
            var response = new Response<List<LocationTreeVM>> { Status = _error };
            var customerId = _currentUserService.GetCustomerId();

            if (customerId <= 0)
            {
                response.Message.Add("X-Customer-Id header is required");
                return response;
            }

            var locations = (await _locationRepository.GetByCustomerIdAsync(customerId)).ToList();

            var regions = locations
                .Where(x => x.Level == 1)
                .OrderBy(x => x.Name)
                .Select(region => new LocationTreeVM
                {
                    Id = region.Id,
                    Name = region.Name,
                    Code = region.Code,
                    Type = "Region",
                    Children = locations
                        .Where(sr => sr.Level == 2 && sr.ParentId == region.Id)
                        .OrderBy(sr => sr.Name)
                        .Select(subRegion => new LocationTreeVM
                        {
                            Id = subRegion.Id,
                            Name = subRegion.Name,
                            Code = subRegion.Code,
                            Type = "SubRegion",
                            Children = locations
                                .Where(z => z.Level == 3 && z.ParentId == subRegion.Id)
                                .OrderBy(z => z.Name)
                                .Select(zone => new LocationTreeVM
                                {
                                    Id = zone.Id,
                                    Name = zone.Name,
                                    Code = zone.Code,
                                    Type = "Zone"
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

            response.Status = _success;
            response.Data = regions;
            return response;
        }
    }
}
