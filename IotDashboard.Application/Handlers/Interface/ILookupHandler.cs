using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface ILookupHandler : IBaseHandler<LookupVM>
    {
        Task<Response<IEnumerable<LookupVM>>> GetByCategory(string category);
    }
}
