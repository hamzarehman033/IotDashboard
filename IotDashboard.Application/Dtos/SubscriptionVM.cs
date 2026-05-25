namespace IotDashboard.Application.Dtos
{
    public class SubscriptionDetailVM
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string Status { get; set; } = "Inactive";
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
