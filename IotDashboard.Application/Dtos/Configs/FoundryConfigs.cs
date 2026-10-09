namespace IotDashboard.Application.Dtos.Configs
{
    public class FoundryConfigs
    {
        public const string SectionName = "Foundry";

        public string Endpoint { get; set; } =
            "https://hamzarehman033-8273-resource.services.ai.azure.com/openai/v1";

        public string apiKey { get; set; } = string.Empty;

        /// <summary>The deployment name shown in the Foundry model's sample code.</summary>
        public string ModelId { get; set; } = "gpt-4.1-nano";

        /// <summary>Optional second model if primary is unavailable.</summary>
        public string FallbackModelId { get; set; } = string.Empty;

        public int MaxTokens { get; set; } = 600;
        public int MaxMessageLength { get; set; } = 1000;
        public int TimeoutSeconds { get; set; } = 180;
    }
}
