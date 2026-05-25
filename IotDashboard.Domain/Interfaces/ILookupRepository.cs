using IotDashboard.Domain.Entities;

namespace IotDashboard.Domain.Interfaces
{
    public interface ILookupRepository : IBaseRepository<Lookup>
    {
        Task<IEnumerable<Lookup>> GetByCategory(string category);
    }
}
