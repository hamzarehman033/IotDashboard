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
    public class LookupHandler : BaseHandler<LookupVM, Lookup>, ILookupHandler
    {
        private readonly ILookupRepository _repository;

        public LookupHandler(
            ILookupRepository repository,
            IValidator<LookupVM> validator,
            FilterValidator<LookupVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, LookupMapper.Mapper.Value, validator, filterValidator, httpContextAccessor)
        {
            _repository = repository;
        }

        public async Task<Response<IEnumerable<LookupVM>>> GetByCategory(string category)
        {
            var lookups = await _repository.GetByCategory(category);
            var mapped = LookupMapper.Mapper.Value.Map<IEnumerable<LookupVM>>(lookups);
            return new Response<IEnumerable<LookupVM>> 
            { 
                Data = mapped,
                Status = "Success"
            };
        }
    }
}
