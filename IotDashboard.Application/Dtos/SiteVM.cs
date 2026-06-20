namespace IotDashboard.Application.Dtos
{
    public class SiteVM
    {
        public long Id { get; set; }
        public long RegionId { get; set; }
        public long SubRegionId { get; set; }
        public long ZoneId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string Address { get; set; } = string.Empty;
        public string Coordinates { get; set; } = string.Empty;
    }
}
