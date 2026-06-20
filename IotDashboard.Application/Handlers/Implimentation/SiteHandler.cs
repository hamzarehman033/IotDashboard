using FluentValidation;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Mappers;
using IotDashboard.Application.Validators;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using IotDashboard.Infrastructure.AuditServices;
using Microsoft.AspNetCore.Http;

namespace IotDashboard.Application.Handlers.Implimentation
{
    public class SiteHandler : BaseHandler<SiteVM, Site>, ISiteHandler
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ICurrentUserService _currentUserService;

        public SiteHandler(
            ISiteRepository siteRepository,
            ILocationRepository locationRepository,
            ICurrentUserService currentUserService,
            IValidator<SiteVM> validator,
            FilterValidator<SiteVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(siteRepository, SiteMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
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
