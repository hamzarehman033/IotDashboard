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
    public class TenantHandler : BaseHandler<TenantVM, Tenant>, ITenantHandler
    {
        public TenantHandler(
            ITenantRepository repo,
            IValidator<TenantVM> validator,
            FilterValidator<TenantVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(repo, TenantMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
        }
    }
}