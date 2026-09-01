using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;

namespace IotDashboard.Infrastructure.Persistence.Repository
{
    public class ActivityRepository : BaseRepository<Activity>, IActivityRepository
    {
        public ActivityRepository(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}
