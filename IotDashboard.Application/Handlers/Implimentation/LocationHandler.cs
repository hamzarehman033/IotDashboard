using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Mappers;
using IotDashboard.Application.Validators;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace IotDashboard.Application.Handlers.Implimentation
{
    public class LocationHandler : BaseHandler<LocationVM, Location>, ILocationHandler
    {
        public LocationHandler(
            ILocationRepository locationRepository,
            IValidator<LocationVM> validator,
            FilterValidator<LocationVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(locationRepository, LocationMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
        }
    }
}
