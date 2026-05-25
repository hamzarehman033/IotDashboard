using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;

namespace IotDashboard.Infrastructure.Persistence.Repository
{
    public class SubscriptionRepository : BaseRepository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}
