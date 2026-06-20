namespace IotDashboard.Application.Dtos
{
    public class LocationTreeVM
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<LocationTreeVM> Children { get; set; } = new();
    }
}
