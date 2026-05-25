namespace IotDashboard.Domain.Entities
{
    public class Lookup : BaseEntity
    {
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Description { get; set; }
    }
}
