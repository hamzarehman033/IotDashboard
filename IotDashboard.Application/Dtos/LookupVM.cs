namespace IotDashboard.Application.Dtos
{
    public class LookupVM
    {
        public long Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
