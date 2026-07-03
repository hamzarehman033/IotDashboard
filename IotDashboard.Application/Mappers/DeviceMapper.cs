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
        public DeviceProfile()
        {
            CreateMap<Device, DeviceVM>();

            CreateMap<DeviceVM, Device>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Region, opt => opt.Ignore())
                .ForMember(dest => dest.SubRegion, opt => opt.Ignore())
                .ForMember(dest => dest.Zone, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore());
        }
    }
}
