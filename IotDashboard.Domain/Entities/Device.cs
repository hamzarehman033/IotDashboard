namespace IotDashboard.Domain.Entities
{
    public class Device : BaseEntity
    {
        public long CustomerId { get; set; }
        public long SiteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string MqttHost { get; set; } = string.Empty;
        public int MqttPort { get; set; } = 1883;
        public string MqttClientId { get; set; } = string.Empty;
        public string MqttUsername { get; set; } = string.Empty;
        public string MqttPassword { get; set; } = string.Empty;
        public bool UseTls { get; set; }
        public int KeepAliveSeconds { get; set; } = 60;
        public string RmsSubscribeTopic { get; set; } = string.Empty;
        public string AiSubscribeTopic { get; set; } = string.Empty;

        public Customer Customer { get; set; } = null!;
        public Site Site { get; set; } = null!;
    }
}
