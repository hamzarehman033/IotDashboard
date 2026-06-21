using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface ISiteHandler : IBaseHandler<SiteVM>
    {
        Task<Response<List<SiteDeviceRowVM>>> GetCombinedAsync();
    }
}
