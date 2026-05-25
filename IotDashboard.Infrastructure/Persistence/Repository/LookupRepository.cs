using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Infrastructure.Persistence.Repository
{
    public class LookupRepository : BaseRepository<Lookup>, ILookupRepository
    {
        private readonly AppDBContext _dbContext;

        public LookupRepository(AppDBContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Lookup>> GetByCategory(string category)
        {
            return await _dbContext.Set<Lookup>()
                .Where(x => x.Category == category && x.IsActive)
                .OrderBy(x => x.Order)
                .ToListAsync();
        }
    }
}
