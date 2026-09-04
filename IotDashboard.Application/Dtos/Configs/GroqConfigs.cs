namespace IotDashboard.Application.Dtos.Configs
{
    public class GroqConfigs
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ModelId { get; set; } = "openai/gpt-oss-20b";
        public int MaxTokens { get; set; } = 600;
        public int MaxMessageLength { get; set; } = 1000;
    }
}
