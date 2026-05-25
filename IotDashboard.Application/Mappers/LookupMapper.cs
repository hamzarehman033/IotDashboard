using AutoMapper;
using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;

namespace IotDashboard.Application.Mappers
{
    public class LookupMapper
    {
        public static Lazy<IMapper> Mapper = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<LookupProfile>()
            );
            return config.CreateMapper();
        });
    }

    public class LookupProfile : Profile
    {
        public LookupProfile() => CreateMap<Lookup, LookupVM>().ReverseMap();
    }
}
