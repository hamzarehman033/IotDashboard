namespace IotDashboard.Application.Dtos.Configs
{
    public class CameraConfigs
    {
        public const string SectionName = "Cameras";

        // Device protocol limit.
        public const int MaxSupportedPerDevice = 16;

        // Set in appsettings (Cameras:MaxPerDevice).
        public int MaxPerDevice { get; set; } = 4;

        public int EffectiveMaxPerDevice => Math.Clamp(MaxPerDevice, 1, MaxSupportedPerDevice);
    }
}
