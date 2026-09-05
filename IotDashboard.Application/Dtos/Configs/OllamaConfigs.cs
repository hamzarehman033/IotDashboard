namespace IotDashboard.Application.Dtos.Configs
{
    public class OllamaConfigs
    {
        public const string SectionName = "Ollama";

        public string BaseUrl { get; set; } =
            "https://olama-container.kindground-dc24e970.uaenorth.azurecontainerapps.io";

        /// <summary>Optional. Ollama usually does not require a key.</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>Must match a model pulled on the Ollama host (e.g. llama3.2).</summary>
        public string ModelId { get; set; } = "qwen2.5-coder:1.5b";

        /// <summary>Optional second model if primary is unavailable.</summary>
        public string FallbackModelId { get; set; } = string.Empty;

        public int MaxTokens { get; set; } = 600;
        public int MaxMessageLength { get; set; } = 1000;
        public int TimeoutSeconds { get; set; } = 180;
    }
}
