namespace IotDashboard.Application.Dtos
{
    public class ActivityVM
    {
        public long Id { get; set; }
        public long DeviceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Team { get; set; } = string.Empty;
        public int Persons { get; set; }
        public bool IsActive { get; set; }
    }
}
