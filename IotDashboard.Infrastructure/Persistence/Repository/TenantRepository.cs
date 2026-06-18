using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;

namespace IotDashboard.Infrastructure.Persistence.Repository
{
    public class TenantRepository : BaseRepository<Tenant>, ITenantRepository
    {
        public TenantRepository(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}