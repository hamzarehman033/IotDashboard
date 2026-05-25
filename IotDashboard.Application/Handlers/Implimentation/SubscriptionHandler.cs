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
    public class SubscriptionHandler : BaseHandler<SubscriptionDetailVM, Subscription>, ISubscriptionHandler
    {
        public SubscriptionHandler(
            ISubscriptionRepository repo,
            IValidator<SubscriptionDetailVM> validator,
            FilterValidator<SubscriptionDetailVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(repo, SubscriptionMapper.Mapper, validator, filterValidator, httpContextAccessor)
        {
        }
    }
}
