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
        private readonly ILogger<DeviceDataService> _logger;

        public DeviceDataService(
            IMqttClientService mqttClientService,
            IDeviceDataCache deviceDataCache,
            IHubContext<DeviceDataHub> hubContext,
            IMqttPayloadDecoder mqttPayloadDecoder,
            ITelemetryPersistenceService telemetryPersistenceService,
            ILogger<DeviceDataService> logger)
        {
            _mqttClientService = mqttClientService;
            _deviceDataCache = deviceDataCache;
            _hubContext = hubContext;
            _mqttPayloadDecoder = mqttPayloadDecoder;
            _telemetryPersistenceService = telemetryPersistenceService;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            // Register callback with MQTT service to handle incoming messages
            _mqttClientService.RegisterMessageReceivedCallback(async (eventArgs) =>
            {
                try
                {
                    if (VisionDetectionParser.IsVisionTopic(eventArgs.Topic))
                    {
                        await HandleVisionDetectionAsync(eventArgs);
                        return;
                    } else {
                        await HandleRmsTelemetryAsync(eventArgs);
                    }

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
            if (!VisionDetectionParser.TryParseAndValidate(eventArgs.Payload, out var message, out var error)
                || message?.Image == null)
            {
                var preview = eventArgs.Payload.Length <= 80
                    ? eventArgs.Payload
                    : eventArgs.Payload[..80];
                _logger.LogWarning(
                    "Invalid vision detection for device {DeviceId} on topic {Topic}: {Error}. PayloadPreview={PayloadPreview}",
                    eventArgs.DeviceId,
                    eventArgs.Topic,
                    error ?? "Unknown error",
                    preview);
                return;
            }

            var detectionSummary = message.Detections
                .Select(d => $"{d.ClassName}:{d.Confidence:F2}")
                .ToList();

            _logger.LogInformation(
                "Vision detection received. DeviceId={DeviceId}, Topic={Topic}, EventId={EventId}, SourceDevice={SourceDevice}, Timestamp={Timestamp}, EventType={EventType}, Detections={DetectionCount} [{Detections}], Image={Width}x{Height} ({ByteSize} bytes, {Format})",
                eventArgs.DeviceId,
                eventArgs.Topic,
                message.EventId,
                message.DeviceId,
                message.Timestamp,
                message.EventType,
                message.Detections.Count,
                string.Join(", ", detectionSummary),
                message.Image.Width,
                message.Image.Height,
                message.Image.ByteSize,
                message.Image.Format);

            var groupName = $"device-{eventArgs.DeviceId}";
            await _hubContext.Clients.Group(groupName).SendAsync(
                "VisionDetectionReceived",
                new
                {
                    DeviceId = eventArgs.DeviceId,
                    Topic = eventArgs.Topic,
                    ReceivedAt = eventArgs.ReceivedAt,
                    EventId = message.EventId,
                    SourceDeviceId = message.DeviceId,
                    Timestamp = message.Timestamp,
                    EventType = message.EventType,
                    Detections = message.Detections.Select(d => new
                    {
                        d.ClassId,
                        d.ClassName,
                        d.Confidence,
                        Bbox = d.Bbox == null
                            ? null
                            : new { d.Bbox.X1, d.Bbox.Y1, d.Bbox.X2, d.Bbox.Y2 }
                    }),
                    Image = new
                    {
                        message.Image.Encoding,
                        message.Image.Format,
                        message.Image.Width,
                        message.Image.Height,
                        message.Image.ByteSize,
                        message.Image.Sha256,
                        message.Image.Data
                    }
                });

            _logger.LogInformation(
                "Broadcasted vision detection for device {DeviceId} on topic {Topic} to SignalR clients",
                eventArgs.DeviceId,
                eventArgs.Topic);
        }
    }
}
