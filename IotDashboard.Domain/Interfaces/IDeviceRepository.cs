using IotDashboard.Domain.Entities;

namespace IotDashboard.Domain.Interfaces
{
    public interface IDeviceRepository : IBaseRepository<Device>
    {
        Task<bool> ExistsBySiteIdAsync(long siteId, long? excludeId = null);
        Task<bool> ExistsByCodeAsync(long customerId, string code, long? excludeId = null);
    }
}
