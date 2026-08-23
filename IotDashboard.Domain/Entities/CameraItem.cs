namespace IotDashboard.Domain.Entities
{
    public class CameraItem
    {
        public byte CameraIndex { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
    }
}
