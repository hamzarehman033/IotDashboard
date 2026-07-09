namespace IotDashboard.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public long CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";

        public Customer Customer { get; set; } = null!;
        public ICollection<DeviceTenant> DeviceTenants { get; set; } = new List<DeviceTenant>();
    }
}