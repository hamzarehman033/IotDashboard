using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IotDashboard.Application.Handlers.Implimentation
{
    public class AiVisionHandler : IAiVisionHandler
    {
        private readonly AppDBContext _dbContext;

        public AiVisionHandler(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Response<List<AiVisionPacketDetailVM>>> GetHistoryByDeviceAsync(
            int deviceNumber,
            byte? messageType,
            string? timeSpan,
            int limit,
            CancellationToken cancellationToken = default)
        {
            var safeLimit = Math.Clamp(limit <= 0 ? 100 : limit, 1, 1000);
            var nowUtc = DateTime.UtcNow;
            var fromUtc = ResolveFromUtc(timeSpan, nowUtc);

            var query = _dbContext.AiVisionPackets
                .AsNoTracking()
                .Where(x => x.DeviceNumber == deviceNumber);

            if (messageType.HasValue)
            {
                query = query.Where(x => x.MessageType == messageType.Value);
            }

            query = query.Where(x => x.ReceivedAtUtc >= fromUtc && x.ReceivedAtUtc <= nowUtc);

            var data = await query
                .OrderByDescending(x => x.ReceivedAtUtc)
                .Take(safeLimit)
                .Select(x => new AiVisionPacketDetailVM
                {
                    Id = x.Id,
                    DeviceNumber = x.DeviceNumber,
                    Topic = x.Topic,
                    ReceivedAtUtc = x.ReceivedAtUtc,
                    PacketSignature = x.PacketSignature,
                    // ProtocolVersion = x.ProtocolVersion,
                    MessageType = x.MessageType,
                    // HeaderLength = x.HeaderLength,
                    // Flags = x.Flags,
                    // PacketSequence = x.PacketSequence,
                    TimestampUtc = x.TimestampUtc,
                    SiteIdHash = x.SiteIdHash,
                    // EdgeDeviceIdHash = x.EdgeDeviceIdHash,
                    MessageIdHash = x.MessageIdHash,
                    EventIdHash = x.EventIdHash,
                    CameraId = x.CameraId,
                    EventType = x.EventType,
                    Severity = x.Severity,
                    ConfidenceRaw = x.ConfidenceRaw,
                    ActivityZone = x.ActivityZone,
                    ObjectCount = x.ObjectCount,
                    EhsCodeCount = x.EhsCodeCount,
                    EhsCodes = x.EhsCodes,
                    SnapshotReasonCode = x.SnapshotReasonCode,
                    // ActiveCameraCount = x.ActiveCameraCount,
                    // ConfiguredCameraCount = x.ConfiguredCameraCount,
                    DetectionEnabled = x.DetectionEnabled,
                    SystemStatus = x.SystemStatus,
                    HeartbeatIntervalSec = x.HeartbeatIntervalSec,
                    EdgeUptimeSec = x.EdgeUptimeSec,
                    CpuUsagePercent = x.CpuUsagePercent,
                    RamUsagePercent = x.RamUsagePercent,
                    DiskFreePercent = x.DiskFreePercent,
                    CameraStatusBitmap = x.CameraStatusBitmap,
                    ModelId = x.ModelId,
                    ImageFormat = x.ImageFormat,
                    ImageEncoding = x.ImageEncoding,
                    // ImageWidth = x.ImageWidth,
                    // ImageHeight = x.ImageHeight,
                    // ImageSizeBytes = x.ImageSizeBytes,
                    ImageCrc32 = x.ImageCrc32,
                    // HeaderCrc16 = x.HeaderCrc16,
                    // IsHeaderCrcValid = x.IsHeaderCrcValid,
                    IsImageCrcValid = x.IsImageCrcValid,
                    HasImage = x.ImageBytes != null && x.ImageBytes.Length > 0
                })
                .ToListAsync(cancellationToken);

            return new Response<List<AiVisionPacketDetailVM>>
            {
                Status = "Success",
                Data = data
            };
        }

        private static DateTime ResolveFromUtc(string? timeSpan, DateTime nowUtc)
        {
            var span = timeSpan?.Trim().ToLowerInvariant();

            return span switch
            {
                "1w" => nowUtc.AddDays(-7),
                "1m" => nowUtc.AddDays(-30),
                _ => nowUtc.AddDays(-1)
            };
        }

        public async Task<Response<string?>> GetVisionPacketDetails(
            long id,
            CancellationToken cancellationToken = default)
        {
            var imageBytes = await _dbContext.AiVisionPackets
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => x.ImageBytes)
                .FirstOrDefaultAsync(cancellationToken);

            if (imageBytes == null)
            {
                return new Response<string?>
                {
                    Status = "Error",
                    Message = new List<string> { "Packet not found or image is not available." },
                    Data = null
                };
            }

            if (imageBytes.Length == 0)
            {
                return new Response<string?>
                {
                    Status = "Success",
                    Data = null
                };
            }

            return new Response<string?>
            {
                Status = "Success",
                Data = Convert.ToBase64String(imageBytes)
            };
        }

    }
}
