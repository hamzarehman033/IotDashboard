namespace IotDashboard.Application.Dtos
{
    public class TenantVM
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public bool IsActive { get; set; }
    }
}