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
            DateTime? fromUtc,
            DateTime? toUtc,
            int limit,
            CancellationToken cancellationToken = default)
        {
            var safeLimit = Math.Clamp(limit <= 0 ? 100 : limit, 1, 1000);

            var query = _dbContext.AiVisionPackets
                .AsNoTracking()
                .Where(x => x.DeviceNumber == deviceNumber);

            if (messageType.HasValue)
            {
                query = query.Where(x => x.MessageType == messageType.Value);
            }

            if (fromUtc.HasValue)
            {
                query = query.Where(x => x.ReceivedAtUtc >= fromUtc.Value);
            }

            if (toUtc.HasValue)
            {
                query = query.Where(x => x.ReceivedAtUtc <= toUtc.Value);
            }

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
                    ProtocolVersion = x.ProtocolVersion,
                    MessageType = x.MessageType,
                    HeaderLength = x.HeaderLength,
                    Flags = x.Flags,
                    PacketSequence = x.PacketSequence,
                    TimestampUtc = x.TimestampUtc,
                    SiteIdHash = x.SiteIdHash,
                    EdgeDeviceIdHash = x.EdgeDeviceIdHash,
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
                    ActiveCameraCount = x.ActiveCameraCount,
                    ConfiguredCameraCount = x.ConfiguredCameraCount,
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
                    ImageWidth = x.ImageWidth,
                    ImageHeight = x.ImageHeight,
                    ImageSizeBytes = x.ImageSizeBytes,
                    ImageCrc32 = x.ImageCrc32,
                    HeaderCrc16 = x.HeaderCrc16,
                    IsHeaderCrcValid = x.IsHeaderCrcValid,
                    IsImageCrcValid = x.IsImageCrcValid,
                    HasImage = x.ImageBytes != null && x.ImageBytes.Length > 0,
                    ImageBase64 = x.ImageBytes != null && x.ImageBytes.Length > 0
                    ? Convert.ToBase64String(x.ImageBytes)
                    : null
                })
                .ToListAsync(cancellationToken);

            return new Response<List<AiVisionPacketDetailVM>>
            {
                Status = "Success",
                Data = data
            };
        }

    }
}
