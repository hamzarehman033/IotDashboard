using AutoMapper;
using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;

namespace IotDashboard.Application.Mappers
{
    public class LocationMapper
    {
        public static Lazy<IMapper> Mapper = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<LocationProfile>()
            );
            return config.CreateMapper();
        });
    }

    public class LocationProfile : Profile
    {
        public LocationProfile() => CreateMap<Location, LocationVM>().ReverseMap();
    }
}
