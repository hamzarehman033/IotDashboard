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

            CreateMap<DeviceVM, DeviceUpdateVM>();

            CreateMap<DeviceUpdateVM, Device>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Region, opt => opt.Ignore())
                .ForMember(dest => dest.SubRegion, opt => opt.Ignore())
                .ForMember(dest => dest.Zone, opt => opt.Ignore())
                .ForMember(dest => dest.DeviceTenants, opt => opt.Ignore())
                .ForMember(dest => dest.PowerSources, opt => opt.Ignore())
                .ForMember(dest => dest.RectifierBrand, opt => opt.Ignore())
                .ForMember(dest => dest.RectifierQty, opt => opt.Ignore())
                .ForMember(dest => dest.RectifierCapacity, opt => opt.Ignore())
                .ForMember(dest => dest.BatteryBrand, opt => opt.Ignore())
                .ForMember(dest => dest.BatteryQty, opt => opt.Ignore())
                .ForMember(dest => dest.BatteryCapacity, opt => opt.Ignore())
                .ForMember(dest => dest.SolarBrand, opt => opt.Ignore())
                .ForMember(dest => dest.SolarQty, opt => opt.Ignore())
                .ForMember(dest => dest.SolarCapacity, opt => opt.Ignore())
                .ForMember(dest => dest.GeneratorBrand, opt => opt.Ignore())
                .ForMember(dest => dest.GeneratorQty, opt => opt.Ignore())
                .ForMember(dest => dest.GeneratorCapacity, opt => opt.Ignore())
                .ForMember(dest => dest.RmsSerialNumber, opt => opt.Ignore())
                .ForMember(dest => dest.SimCardNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CamerasInstalledCount, opt => opt.Ignore())
                .ForMember(dest => dest.AiEhsInstalled, opt => opt.Ignore())
                .ForMember(dest => dest.AiSecurityInstalled, opt => opt.Ignore());

            CreateMap<DeviceVM, Device>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Region, opt => opt.Ignore())
                .ForMember(dest => dest.SubRegion, opt => opt.Ignore())
                .ForMember(dest => dest.Zone, opt => opt.Ignore())
                .ForMember(dest => dest.DeviceTenants, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                // Infrastructure fields are managed via PATCH /device/{id}/infrastructure only.
                .ForMember(dest => dest.PowerSources, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.RectifierBrand, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.RectifierQty, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.RectifierCapacity, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.BatteryBrand, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.BatteryQty, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.BatteryCapacity, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.SolarBrand, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.SolarQty, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.SolarCapacity, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.GeneratorBrand, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.GeneratorQty, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.GeneratorCapacity, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.RmsSerialNumber, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.SimCardNumber, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.CamerasInstalledCount, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.AiEhsInstalled, opt => opt.UseDestinationValue())
                .ForMember(dest => dest.AiSecurityInstalled, opt => opt.UseDestinationValue());
        }
    }
}
