namespace IotDashboard.Domain.Entities
{
    public class DeviceTenant
    {
        public long DeviceId { get; set; }
        public long TenantId { get; set; }

        public Device Device { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;
    }
}
