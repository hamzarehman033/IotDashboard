namespace IotDashboard.Application.Dtos.Configs
{
    public class GeminiConfigs
    {
        public const string SectionName = "Gemini";

        public string ApiKey { get; set; } = string.Empty;
        public string ModelId { get; set; } = "gemini-3.5-flash";
        /// <summary>Used when primary model returns 503/high demand.</summary>
        public string FallbackModelId { get; set; } = "gemini-3.1-flash-lite";
        public int MaxTokens { get; set; } = 600;
        public int MaxMessageLength { get; set; } = 1000;
    }
}
