namespace IotDashboard.Domain.Entities
{
    public class Activity : BaseEntity
    {
        public long CustomerId { get; set; }
        public long DeviceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Team { get; set; } = string.Empty;
        public int Persons { get; set; }

        public Customer Customer { get; set; } = null!;
        public Device Device { get; set; } = null!;
    }
}
