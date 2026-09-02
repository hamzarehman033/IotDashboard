using IotDashboard.Application.Dtos;

namespace IotDashboard.Application.Handlers.Interface
{
    public interface IActivityDeviceNotifier
    {
        Task NotifyAddedAsync(ActivityVM activity);
        Task NotifyUpdatedAsync(ActivityVM activity);
        Task NotifyDeletedAsync(long activityId, long deviceId);
    }
}
