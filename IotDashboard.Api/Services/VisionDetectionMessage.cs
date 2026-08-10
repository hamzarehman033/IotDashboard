using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IotDashboard.Api.Services
{
    public class VisionDetectionMessage
    {
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = string.Empty;

        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("detections")]
        public List<VisionDetectionItem> Detections { get; set; } = new();

        [JsonPropertyName("image")]
        public VisionImageInfo? Image { get; set; }
    }

    public class VisionDetectionItem
    {
        [JsonPropertyName("class_id")]
        public int ClassId { get; set; }

        [JsonPropertyName("class_name")]
        public string ClassName { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("bbox")]
        public VisionBoundingBox? Bbox { get; set; }
    }

    public class VisionBoundingBox
    {
        [JsonPropertyName("x1")]
        public double X1 { get; set; }

        [JsonPropertyName("y1")]
        public double Y1 { get; set; }

        [JsonPropertyName("x2")]
        public double X2 { get; set; }

        [JsonPropertyName("y2")]
        public double Y2 { get; set; }
    }

    public class VisionImageInfo
    {
        [JsonPropertyName("encoding")]
        public string Encoding { get; set; } = string.Empty;

        [JsonPropertyName("format")]
        public string Format { get; set; } = string.Empty;

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("byte_size")]
        public int ByteSize { get; set; }

        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;
    }

    public static class VisionDetectionParser
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static bool IsVisionTopic(string topic)
        {
            return !string.IsNullOrWhiteSpace(topic)
                && topic.StartsWith("vision", StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryParseAndValidate(string payload, out VisionDetectionMessage? message, out string? error)
        {
            message = null;
            error = null;

            try
            {
                message = JsonSerializer.Deserialize<VisionDetectionMessage>(payload, JsonOptions);
            }
            catch (JsonException ex)
            {
                error = $"Invalid vision JSON: {ex.Message}";
                return false;
            }

            if (message == null)
            {
                error = "Vision payload deserialized to null";
                return false;
            }

            if (message.Image == null)
            {
                error = "Vision payload missing image";
                return false;
            }

            if (!string.Equals(message.Image.Encoding, "base64", StringComparison.OrdinalIgnoreCase))
            {
                error = $"Unsupported image encoding: {message.Image.Encoding}";
                return false;
            }

            byte[] jpegBytes;
            try
            {
                jpegBytes = Convert.FromBase64String(message.Image.Data);
            }
            catch (FormatException)
            {
                error = "Image data is not valid base64";
                return false;
            }

            if (jpegBytes.Length != message.Image.ByteSize)
            {
                error = $"Image byte size mismatch. Expected {message.Image.ByteSize}, got {jpegBytes.Length}";
                return false;
            }

            var hash = Convert.ToHexString(SHA256.HashData(jpegBytes)).ToLowerInvariant();
            if (!string.Equals(hash, message.Image.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                error = "Image SHA-256 verification failed";
                return false;
            }

            return true;
        }
    }
}
