using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Infrastructure.Persistence.Repository
{
    public class LocationRepository : BaseRepository<Location>, ILocationRepository
    {
        private readonly AppDBContext _dbContext;

        public LocationRepository(AppDBContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Location>> GetByCustomerIdAsync(long customerId)
        {
            return await _dbContext.Set<Location>()
                .Where(x => x.CustomerId == customerId && x.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Location>> GetByParentIdAsync(long parentId)
        {
            return await _dbContext.Set<Location>()
                .Where(x => x.ParentId == parentId && x.IsActive)
                .ToListAsync();
        }
    }
}
