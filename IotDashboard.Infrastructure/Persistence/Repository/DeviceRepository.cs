using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Infrastructure.Persistence.Repository
{
    public class DeviceRepository : BaseRepository<Device>, IDeviceRepository
    {
        private readonly AppDBContext _dbContext;

        public DeviceRepository(AppDBContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsByCodeAsync(long customerId, string code, long? excludeId = null)
        {
            var query = _dbContext.Set<Device>().AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync(x => x.CustomerId == customerId && x.Code == code);
        }
    }
}
