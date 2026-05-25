using IotDashboard.Domain.Entities;

namespace IotDashboard.Domain.Interfaces
{
    public interface ILocationRepository : IBaseRepository<Location>
    {
        Task<IEnumerable<Location>> GetByCustomerIdAsync(long customerId);
        Task<IEnumerable<Location>> GetByParentIdAsync(long parentId);
    }
}
