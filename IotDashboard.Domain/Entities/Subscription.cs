namespace IotDashboard.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public long CustomerId { get; set; }
        public string Status { get; set; } = "Inactive";
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public string Notes { get; set; } = string.Empty;

        public Customer Customer { get; set; } = null!;
    }
}
