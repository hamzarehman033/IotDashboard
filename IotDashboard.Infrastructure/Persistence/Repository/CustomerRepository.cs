using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;

namespace IotDashboard.Infrastructure.Persistence.Repository
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDBContext dbContext) : base(dbContext)
        {
        }
    }
}
