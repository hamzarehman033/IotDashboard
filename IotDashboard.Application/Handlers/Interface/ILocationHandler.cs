using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface ILocationHandler : IBaseHandler<LocationVM>
    {
        Task<Response<List<LocationTreeVM>>> GetTreeAsync();
    }
}
