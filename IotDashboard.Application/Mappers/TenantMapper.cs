using AutoMapper;
using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;

namespace IotDashboard.Application.Mappers
{
    public class TenantMapper
    {
        public static Lazy<IMapper> Mapper = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<TenantProfile>()
            );
            return config.CreateMapper();
        });
    }

    public class TenantProfile : Profile
    {
        public TenantProfile() => CreateMap<Tenant, TenantVM>().ReverseMap();
    }
}