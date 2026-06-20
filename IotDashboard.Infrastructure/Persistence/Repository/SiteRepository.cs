using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;

namespace IotDashboard.Infrastructure.Persistence.Repository
{
    public class SiteRepository : BaseRepository<Site>, ISiteRepository
    {
        public SiteRepository(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}
