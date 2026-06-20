namespace IotDashboard.Domain.Entities
{
    public class Site : BaseEntity
    {
        public long CustomerId { get; set; }
        public long RegionId { get; set; }
        public long SubRegionId { get; set; }
        public long ZoneId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string Address { get; set; } = string.Empty;
        public string Coordinates { get; set; } = string.Empty;

        public Customer Customer { get; set; } = null!;
        public Location Region { get; set; } = null!;
        public Location SubRegion { get; set; } = null!;
        public Location Zone { get; set; } = null!;
    }
}
