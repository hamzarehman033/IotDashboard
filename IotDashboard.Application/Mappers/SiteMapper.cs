using AutoMapper;
using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;

namespace IotDashboard.Application.Mappers
{
    public class SiteMapper
    {
        public static Lazy<IMapper> Mapper = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<SiteProfile>()
            );
            return config.CreateMapper();
        });
    }

    public class SiteProfile : Profile
    {
        public SiteProfile() => CreateMap<Site, SiteVM>().ReverseMap();
    }
}
