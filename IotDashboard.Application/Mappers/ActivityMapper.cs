using AutoMapper;
using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;

namespace IotDashboard.Application.Mappers
{
    public class ActivityMapper
    {
        public static Lazy<IMapper> Mapper = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<ActivityProfile>()
            );
            return config.CreateMapper();
        });
    }

    public class ActivityProfile : Profile
    {
        public ActivityProfile() => CreateMap<Activity, ActivityVM>().ReverseMap();
    }
}
