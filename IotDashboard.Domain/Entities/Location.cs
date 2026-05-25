namespace IotDashboard.Domain.Entities
{
    public class Location : BaseEntity
    {
        public long CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public long? ParentId { get; set; }
        public int Level { get; set; } // 1=Region, 2=SubRegion, 3=Zone

        public Customer Customer { get; set; } = null!;
        public Location? Parent { get; set; }
        public ICollection<Location> Children { get; set; } = new List<Location>();
    }
}
