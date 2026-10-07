using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace IotDashboard.Application.Dtos
{
    /// <summary>Requests a device command with a command-specific JSON object payload.</summary>
    public sealed record SendDeviceCommandRequest
    {
        [Required, MaxLength(100)]
        public string? Command { get; init; }

        [Required]
        public JsonElement? Payload { get; init; }
    }
}