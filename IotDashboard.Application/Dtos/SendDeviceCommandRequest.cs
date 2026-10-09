using System.ComponentModel.DataAnnotations;

namespace IotDashboard.Application.Dtos
{
    /// <summary>Requests one command using the unified binary device packet.</summary>
    public sealed record SendDeviceCommandRequest
    {
        [Required, Range(1, ushort.MaxValue)]
        public ushort? TargetId { get; init; }

        [Required, Range(0, 4)]
        public byte? Action { get; init; }

        [Required, Range(0, byte.MaxValue)]
        public byte? Channel { get; init; }

        [Required, Range(0, ushort.MaxValue)]
        public ushort? DurationSeconds { get; init; }
    }
}
