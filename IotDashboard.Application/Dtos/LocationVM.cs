namespace IotDashboard.Application.Dtos
{
    public class LocationVM
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public long? ParentId { get; set; }
        public int Level { get; set; } // 1=Region, 2=SubRegion, 3=Zone
        public bool IsActive { get; set; }

        public string Type => Level switch
        {
            1 => "Region",
            2 => "SubRegion",
            3 => "Zone",
            _ => "Unknown"
        };
    }
}
