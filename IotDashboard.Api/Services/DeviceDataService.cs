using IotDashboard.Infrastructure.ExternalServices.Mqtt;
using IotDashboard.Application.Util;
using IotDashboard.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IotDashboard.Api.Services
{
    /// <summary>
    /// Service to bridge MQTT messages and SignalR real-time notifications
    /// </summary>
    public interface IDeviceDataService
    {
        /// <summary>
        /// Initialize the service with cache and SignalR hub context
        /// </summary>
        Task InitializeAsync();
    }

    public class DeviceDataService : IDeviceDataService
    {
        private readonly IMqttClientService _mqttClientService;
        private readonly IDeviceDataCache _deviceDataCache;
        private readonly IHubContext<DeviceDataHub> _hubContext;
        private readonly IMqttPayloadDecoder _mqttPayloadDecoder;
        private readonly ITelemetryPersistenceService _telemetryPersistenceService;
        private readonly IAiVisionPersistenceService _aiVisionPersistenceService;
        private readonly ILogger<DeviceDataService> _logger;

        public DeviceDataService(
            IMqttClientService mqttClientService,
            IDeviceDataCache deviceDataCache,
            IHubContext<DeviceDataHub> hubContext,
            IMqttPayloadDecoder mqttPayloadDecoder,
            ITelemetryPersistenceService telemetryPersistenceService,
            IAiVisionPersistenceService aiVisionPersistenceService,
            ILogger<DeviceDataService> logger)
        {
            _mqttClientService = mqttClientService;
            _deviceDataCache = deviceDataCache;
            _hubContext = hubContext;
            _mqttPayloadDecoder = mqttPayloadDecoder;
            _telemetryPersistenceService = telemetryPersistenceService;
            _aiVisionPersistenceService = aiVisionPersistenceService;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            // Register callback with MQTT service to handle incoming messages
            _mqttClientService.RegisterMessageReceivedCallback(async (eventArgs) =>
            {
                try
                {
                    if (AiVisionBinaryDecoder.IsAiVisionTopic(eventArgs.Topic))
                    {
                        await HandleVisionDetectionAsync(eventArgs);
                        return;
                    }

                    await HandleRmsTelemetryAsync(eventArgs);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing device data for device {eventArgs.DeviceId}");
                }
            });

            _logger.LogInformation("DeviceDataService initialized with MQTT and SignalR integration");
            await Task.CompletedTask;
        }

        private async Task HandleRmsTelemetryAsync(MqttMessageReceivedEventArgs eventArgs)
        {
            var decodedPayload = _mqttPayloadDecoder.Decode(eventArgs.Topic, eventArgs.Payload);

            if (decodedPayload.TelemetryPacket != null)
            {
                decodedPayload.TelemetryPacket.DeviceNumber = eventArgs.DeviceId;
            }

            await _telemetryPersistenceService.PersistAsync(
                eventArgs.Topic,
                decodedPayload,
                eventArgs.ReceivedAt);

            var decodedForClients = (object?)decodedPayload.TelemetryPacket ?? decodedPayload.Fields;

            _deviceDataCache.SetDeviceData(
                eventArgs.DeviceId,
                eventArgs.Topic,
                eventArgs.Payload,
                decodedForClients,
                decodedPayload.IsHexPayload,
                decodedPayload.NormalizedHexPayload,
                decodedPayload.Error,
                eventArgs.ReceivedAt);

            var groupName = $"device-{eventArgs.DeviceId}";
            await _hubContext.Clients.Group(groupName).SendAsync(
                "DeviceDataReceived",
                new
                {
                    DeviceId = eventArgs.DeviceId,
                    Topic = eventArgs.Topic,
                    Payload = eventArgs.Payload,
                    DecodedPayload = decodedForClients,
                    IsHexPayload = decodedPayload.IsHexPayload,
                    NormalizedHexPayload = decodedPayload.NormalizedHexPayload,
                    DecodingError = decodedPayload.Error,
                    ReceivedAt = eventArgs.ReceivedAt
                });

            if (decodedPayload.TelemetryPacket != null)
            {
                _logger.LogInformation(
                    $"Decoded telecom telemetry for device {eventArgs.DeviceId} on topic {eventArgs.Topic}: {JsonSerializer.Serialize(decodedPayload.TelemetryPacket)}");
            }

            _logger.LogInformation(
                $"Broadcasted device {eventArgs.DeviceId} data to SignalR clients on topic {eventArgs.Topic}");
        }

        private async Task HandleVisionDetectionAsync(MqttMessageReceivedEventArgs eventArgs)
        {
            if (!AiVisionBinaryDecoder.TryDecode(eventArgs.PayloadBytes, out var packet, out var error)
                || packet == null)
            {
                _logger.LogWarning(
                    "Invalid AI Vision binary packet for device {DeviceId} on topic {Topic}: {Error}",
                    eventArgs.DeviceId,
                    eventArgs.Topic,
                    error ?? "Unknown error");
                return;
            }

            try
            {
                await _aiVisionPersistenceService.PersistAsync(
                    eventArgs.DeviceId,
                    eventArgs.Topic,
                    packet,
                    eventArgs.ReceivedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to persist AI Vision packet for device {DeviceId} on topic {Topic}",
                    eventArgs.DeviceId,
                    eventArgs.Topic);
            }

            await BroadcastBinaryVisionAsync(eventArgs, packet);
        }

        private async Task BroadcastBinaryVisionAsync(MqttMessageReceivedEventArgs eventArgs, AiVisionPacket packet)
        {
            _logger.LogInformation(
                "AI Vision binary packet. DeviceId={DeviceId}, Topic={Topic}, MessageType={MessageType}, EventType={EventType}, Severity={Severity}, ConfidenceRaw={ConfidenceRaw}, CameraId={CameraId}, ActivityZone={ActivityZone}, EhsCodes=[{EhsCodes}], Image={Width}x{Height} ({ByteSize} bytes)",
                eventArgs.DeviceId,
                eventArgs.Topic,
                packet.MessageType,
                packet.EventType,
                packet.Severity,
                packet.ConfidenceRaw,
                packet.CameraId,
                packet.ActivityZone,
                string.Join(",", packet.EhsCodes),
                packet.ImageWidth,
                packet.ImageHeight,
                packet.ImageSizeBytes);

            object? image = null;
            if (packet.ImageBytes.Length > 0)
            {
                image = new
                {
                    Encoding = packet.ImageEncoding,
                    Format = packet.ImageFormat,
                    Width = packet.ImageWidth,
                    Height = packet.ImageHeight,
                    ByteSize = packet.ImageSizeBytes,
                    Crc32 = packet.ImageCrc32,
                    Data = Convert.ToBase64String(packet.ImageBytes)
                };
            }

            var groupName = $"device-{eventArgs.DeviceId}";
            await _hubContext.Clients.Group(groupName).SendAsync(
                "VisionDetectionReceived",
                new
                {
                    DeviceId = eventArgs.DeviceId,
                    Topic = eventArgs.Topic,
                    ReceivedAt = eventArgs.ReceivedAt,
                    PacketSignature = packet.PacketSignature,
                    ProtocolVersion = packet.ProtocolVersion,
                    MessageType = packet.MessageType,
                    HeaderLength = packet.HeaderLength,
                    Flags = packet.Flags,
                    PacketSequence = packet.PacketSequence,
                    TimestampUtc = packet.TimestampUtc,
                    SiteIdHash = packet.SiteIdHash,
                    EdgeDeviceIdHash = packet.EdgeDeviceIdHash,
                    MessageIdHash = packet.MessageIdHash,
                    EventIdHash = packet.EventIdHash,
                    CameraId = packet.CameraId,
                    EventType = packet.EventType,
                    Severity = packet.Severity,
                    ConfidenceRaw = packet.ConfidenceRaw,
                    ActivityZone = packet.ActivityZone,
                    ObjectCount = packet.ObjectCount,
                    EhsCodeCount = packet.EhsCodeCount,
                    EhsCodes = packet.EhsCodes,
                    SnapshotReasonCode = packet.SnapshotReasonCode,
                    ActiveCameraCount = packet.ActiveCameraCount,
                    ConfiguredCameraCount = packet.ConfiguredCameraCount,
                    DetectionEnabled = packet.DetectionEnabled,
                    SystemStatus = packet.SystemStatus,
                    HeartbeatIntervalSec = packet.HeartbeatIntervalSec,
                    EdgeUptimeSec = packet.EdgeUptimeSec,
                    CpuUsagePercent = packet.CpuUsagePercent,
                    RamUsagePercent = packet.RamUsagePercent,
                    DiskFreePercent = packet.DiskFreePercent,
                    CameraStatusBitmap = packet.CameraStatusBitmap,
                    ModelId = packet.ModelId,
                    ImageFormat = packet.ImageFormat,
                    ImageEncoding = packet.ImageEncoding,
                    ImageWidth = packet.ImageWidth,
                    ImageHeight = packet.ImageHeight,
                    ImageSizeBytes = packet.ImageSizeBytes,
                    ImageCrc32 = packet.ImageCrc32,
                    HeaderCrc16 = packet.HeaderCrc16,
                    IsHeaderCrcValid = packet.IsHeaderCrcValid,
                    IsImageCrcValid = packet.IsImageCrcValid,
                    Image = image
                });

            _logger.LogInformation(
                "Broadcasted AI Vision message_type={MessageType} for device {DeviceId} on topic {Topic} to SignalR clients",
                packet.MessageType,
                eventArgs.DeviceId,
                eventArgs.Topic);
        }
    }
}
