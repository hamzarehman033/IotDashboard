using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface ICustomerHandler : IBaseHandler<CustomerDetailVM>
    {
        Task<Response<bool>> SetSubscriptionStatusAsync(long customerId, bool isActive);
        Task<Response<bool>> DeactivateCustomerAsync(long customerId);
    }
}
