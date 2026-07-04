namespace IotDashboard.Application.Dtos
{
    public class CustomerDetailVM
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string Logo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool SubscriptionActive { get; set; }
        public string Status { get; set; } = "Inactive";
        public bool IsActive { get; set; }
    }
}
