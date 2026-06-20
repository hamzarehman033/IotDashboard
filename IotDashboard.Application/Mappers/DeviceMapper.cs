using AutoMapper;
using IotDashboard.Application.Dtos;
using IotDashboard.Domain.Entities;

namespace IotDashboard.Application.Mappers
{
    public class DeviceMapper
    {
        public static Lazy<IMapper> Mapper = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<DeviceProfile>()
            );
            return config.CreateMapper();
        });
    }

    public class DeviceProfile : Profile
    {
        public DeviceProfile() => CreateMap<Device, DeviceVM>().ReverseMap();
    }
}
