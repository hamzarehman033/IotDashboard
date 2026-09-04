using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Dtos.Configs;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Util;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using IotDashboard.Infrastructure.AuditServices;
using IotDashboard.Infrastructure.ExternalServices.Mqtt;
using IotDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IotDashboard.Api.Services
{
    public class ChatService : IChatService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _http;
        private readonly GroqConfigs _groq;
        private readonly IDeviceRepository _devices;
        private readonly IActivityRepository _activities;
        private readonly ITelemetryHandler _telemetry;
        private readonly IDeviceDataCache _deviceDataCache;
        private readonly AppDBContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IHostEnvironment _env;
        private readonly ILogger<ChatService> _logger;
        private readonly string _faq;

        /// <summary>Device is offline if last packet is older than this (matches DeviceDataHub).</summary>
        private static readonly TimeSpan OfflineAfterNoPacket = TimeSpan.FromMinutes(10);

        public ChatService(
            HttpClient http,
            IOptions<GroqConfigs> groq,
            IDeviceRepository devices,
            IActivityRepository activities,
            ITelemetryHandler telemetry,
            IDeviceDataCache deviceDataCache,
            AppDBContext db,
            ICurrentUserService currentUser,
            IWebHostEnvironment webEnv,
            IHostEnvironment env,
            ILogger<ChatService> logger)
        {
            _http = http;
            _groq = groq.Value;
            _devices = devices;
            _activities = activities;
            _telemetry = telemetry;
            _deviceDataCache = deviceDataCache;
            _db = db;
            _currentUser = currentUser;
            _env = env;
            _logger = logger;

            var faqPath = Path.Combine(webEnv.ContentRootPath, "Content", "faq.md");
            _faq = File.Exists(faqPath)
                ? File.ReadAllText(faqPath)
                : "Answer from available tools and the FAQ. Do not invent numbers.";
        }

        public async Task<Response<ChatReplyVM>> AskAsync(ChatRequestVM request, CancellationToken ct = default)
        {
            var message = request?.Message?.Trim();
            if (string.IsNullOrWhiteSpace(message))
                return Fail("Message is required.");

            if (message.Length > _groq.MaxMessageLength)
                return Fail($"Message must be at most {_groq.MaxMessageLength} characters.");

            if (string.IsNullOrWhiteSpace(_groq.ApiKey))
                return Fail("Chat is not configured. Missing Groq API key.");

            request ??= new ChatRequestVM();
            var messages = BuildMessages(request, message);

            try
            {
                var first = await CallGroqAsync(messages, includeTools: true, ct);
                if (!first.Ok)
                    return Fail(first.Error!);

                if (first.ToolCalls is { Count: > 0 })
                {
                    var call = first.ToolCalls[0];
                    var toolResult = await ExecuteToolAsync(
                        call.Function?.Name,
                        call.Function?.Arguments,
                        request,
                        ct);

                    messages.Add(new GroqMessage
                    {
                        Role = "assistant",
                        Content = first.Content,
                        ToolCalls = first.ToolCalls
                    });
                    messages.Add(new GroqMessage
                    {
                        Role = "tool",
                        ToolCallId = call.Id,
                        Content = toolResult
                    });

                    var second = await CallGroqAsync(messages, includeTools: false, ct);
                    if (!second.Ok)
                        return Fail(second.Error!);

                    if (string.IsNullOrWhiteSpace(second.Content))
                        return Fail("No answer was returned. Please try again.");

                    return Ok(ToPlainText(second.Content));
                }

                if (string.IsNullOrWhiteSpace(first.Content))
                    return Fail("No answer was returned. Please try again.");

                return Ok(ToPlainText(first.Content));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat request failed");
                return Fail(_env.IsDevelopment()
                    ? $"Chat failed: {ex.Message}"
                    : "Unable to get an answer right now. Please try again later.");
            }
        }

        private List<GroqMessage> BuildMessages(ChatRequestVM request, string message)
        {
            var contextHint = string.Empty;
            if (request.DeviceId.HasValue || !string.IsNullOrWhiteSpace(request.DeviceName))
            {
                contextHint =
                    $"\nSelected device context: id={request.DeviceId?.ToString() ?? "none"}, name={request.DeviceName ?? "none"}. " +
                    "Use this when calling GetDeviceStatus if the user asks about a device status.\n";
            }

            return new List<GroqMessage>
            {
                new()
                {
                    Role = "system",
                    Content =
                        "You are the IoT Dashboard help assistant.\n" +
                        "For how-to and definition questions, answer from the FAQ.\n" +
                        "For live data, call exactly one matching tool.\n" +
                        "Never invent numbers or status. If a device-specific question has no device selected, ask the user to select a device.\n" +
                        "For device status answers: say the device name, whether it is online or offline, and key fields if present. " +
                        "Do not mention a 10-minute window, online window, Active flag, isActive, or that telemetry data is unavailable.\n" +
                        "Reply in plain text only. Do not use markdown: no **, __, *, #, or backticks.\n" +
                        contextHint + "\n" +
                        _faq
                }
            }
            .Concat(BuildHistory(request))
            .Append(new GroqMessage { Role = "user", Content = message })
            .ToList();
        }

        private static IEnumerable<GroqMessage> BuildHistory(ChatRequestVM request)
        {
            foreach (var h in (request.History ?? new()).TakeLast(6))
            {
                var role = h.Role?.Trim().ToLowerInvariant();
                if ((role == "user" || role == "assistant") && !string.IsNullOrWhiteSpace(h.Content))
                    yield return new GroqMessage { Role = role, Content = h.Content.Trim() };
            }
        }

        private async Task<GroqCallResult> CallGroqAsync(List<GroqMessage> messages, bool includeTools, CancellationToken ct)
        {
            var body = new GroqRequest
            {
                Model = _groq.ModelId,
                Messages = messages,
                MaxTokens = _groq.MaxTokens,
                Temperature = 0.2,
                Tools = includeTools ? ChatTools.All : null,
                ToolChoice = includeTools ? "auto" : null
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = JsonContent.Create(body, options: JsonOptions)
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groq.ApiKey);

            using var res = await _http.SendAsync(req, ct);
            var json = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Groq failed: {Status} {Body}", (int)res.StatusCode, json);
                var detail = TryReadGroqError(json);
                if (res.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    return GroqCallResult.Fail("AI rate limit reached. Please try again shortly.");
                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return GroqCallResult.Fail("Invalid Groq API key.");
                return GroqCallResult.Fail(_env.IsDevelopment() && !string.IsNullOrWhiteSpace(detail)
                    ? detail!
                    : "The AI service returned an error. Please try again later.");
            }

            using var doc = JsonDocument.Parse(json);
            var choice = doc.RootElement.GetProperty("choices")[0].GetProperty("message");
            var content = choice.TryGetProperty("content", out var c) && c.ValueKind != JsonValueKind.Null
                ? c.GetString()
                : null;

            List<GroqToolCall>? toolCalls = null;
            if (choice.TryGetProperty("tool_calls", out var toolsEl) && toolsEl.ValueKind == JsonValueKind.Array)
                toolCalls = JsonSerializer.Deserialize<List<GroqToolCall>>(toolsEl.GetRawText(), JsonOptions);

            return GroqCallResult.Success(content, toolCalls);
        }

        private async Task<string> ExecuteToolAsync(
            string? name,
            string? argumentsJson,
            ChatRequestVM request,
            CancellationToken ct)
        {
            return name?.ToLowerInvariant() switch
            {
                "getdevicecount" => await GetDeviceCountAsync(ct),
                "getonlinedevicecount" => await GetOnlineDeviceCountAsync(ct),
                "getdevicestatus" => await GetDeviceStatusAsync(argumentsJson, request, ct),
                "gettodaysactivities" => await GetTodaysActivitiesAsync(ct),
                _ => JsonSerializer.Serialize(new { error = $"Unknown tool '{name}'." })
            };
        }

        private async Task<string> GetDeviceCountAsync(CancellationToken ct)
        {
            if (!TryCustomer(out var customerId, out var error))
                return error;

            var query = _devices.GetAllAsync();
            var total = await query.CountAsync(ct);
            var active = await query.CountAsync(x => x.IsActive, ct);

            return JsonSerializer.Serialize(new
            {
                customerId,
                totalDevicesListed = total,
                activeDevices = active,
                inactiveDevices = total - active
            });
        }

        private async Task<string> GetOnlineDeviceCountAsync(CancellationToken ct)
        {
            if (!TryCustomer(out var customerId, out var error))
                return error;

            var customerDevices = await _devices.GetAllAsync()
                .Select(x => new { x.Id, x.Name, x.Code })
                .ToListAsync(ct);

            var total = customerDevices.Count;
            var now = DateTime.UtcNow;
            var cutoff = now - OfflineAfterNoPacket;

            var codeKeys = customerDevices
                .SelectMany(d => new[] { d.Id.ToString(), d.Code })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var latestByKey = await _db.DeviceTelemetryLatest
                .AsNoTracking()
                .Where(x => codeKeys.Contains(x.DeviceId))
                .Select(x => new { x.DeviceId, x.ReceivedAtUtc })
                .ToListAsync(ct);

            var latestByDeviceKey = latestByKey
                .GroupBy(x => x.DeviceId)
                .ToDictionary(g => g.Key, g => g.Max(x => x.ReceivedAtUtc));

            var liveCache = _deviceDataCache.GetAllDeviceData();

            var online = new List<object>();
            var offline = 0;

            foreach (var device in customerDevices)
            {
                var lastPacketAt = GetLastPacketUtc(device.Id, device.Code, liveCache, latestByDeviceKey);
                var isOnline = lastPacketAt.HasValue && lastPacketAt.Value >= cutoff;

                if (isOnline)
                {
                    online.Add(new
                    {
                        device.Id,
                        device.Name,
                        device.Code,
                        lastPacketAtUtc = lastPacketAt
                    });
                }
                else
                {
                    offline++;
                }
            }

            return JsonSerializer.Serialize(new
            {
                customerId,
                totalDevices = total,
                onlineDevices = online.Count,
                offlineDevices = offline,
                sampleOnlineDevices = online.Take(20)
            });
        }

        private static DateTime? GetLastPacketUtc(
            long deviceId,
            string code,
            Dictionary<int, DeviceDataPoint> liveCache,
            Dictionary<string, DateTime> latestByDeviceKey)
        {
            DateTime? last = null;

            if (liveCache.TryGetValue((int)deviceId, out var cached))
                last = cached.ReceivedAt;

            foreach (var key in new[] { deviceId.ToString(), code })
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                if (latestByDeviceKey.TryGetValue(key, out var dbAt) && (!last.HasValue || dbAt > last.Value))
                    last = dbAt;
            }

            return last;
        }

        private async Task<string> GetDeviceStatusAsync(string? argumentsJson, ChatRequestVM request, CancellationToken ct)
        {
            if (!TryCustomer(out _, out var error))
                return error;

            long? deviceId = request.DeviceId;
            string? deviceName = request.DeviceName;

            if (!string.IsNullOrWhiteSpace(argumentsJson))
            {
                try
                {
                    using var args = JsonDocument.Parse(argumentsJson);
                    if (args.RootElement.TryGetProperty("deviceId", out var idEl) &&
                        idEl.ValueKind == JsonValueKind.Number &&
                        idEl.TryGetInt64(out var id))
                        deviceId ??= id;

                    if (args.RootElement.TryGetProperty("deviceName", out var nameEl) &&
                        nameEl.ValueKind == JsonValueKind.String)
                        deviceName ??= nameEl.GetString();
                }
                catch { }
            }

            Device? device = null;
            if (deviceId.HasValue)
                device = await _devices.GetAllAsync().FirstOrDefaultAsync(x => x.Id == deviceId.Value, ct);

            if (device == null && !string.IsNullOrWhiteSpace(deviceName))
            {
                var name = deviceName.Trim();
                device = await _devices.GetAllAsync()
                    .FirstOrDefaultAsync(x =>
                        x.Name == name ||
                        x.Code == name ||
                        x.Name.ToLower() == name.ToLower() ||
                        x.Code.ToLower() == name.ToLower(), ct);
            }

            if (device == null)
            {
                return JsonSerializer.Serialize(new
                {
                    error = "No device selected. Ask the user to select a device, or provide deviceId / deviceName."
                });
            }

            var telemetryKey = !string.IsNullOrWhiteSpace(device.Code) ? device.Code : device.Id.ToString();
            var latest = await _telemetry.GetLatestByDeviceAsync(telemetryKey, ct);
            if (latest.Status != "Success" && telemetryKey != device.Id.ToString())
                latest = await _telemetry.GetLatestByDeviceAsync(device.Id.ToString(), ct);

            var liveCache = _deviceDataCache.GetAllDeviceData();
            DateTime? lastPacketAt = null;
            if (liveCache.TryGetValue((int)device.Id, out var cached))
                lastPacketAt = cached.ReceivedAt;
            if (latest.Status == "Success" && latest.Data != null &&
                (!lastPacketAt.HasValue || latest.Data.ReceivedAtUtc > lastPacketAt.Value))
                lastPacketAt = latest.Data.ReceivedAtUtc;

            var cutoff = DateTime.UtcNow - OfflineAfterNoPacket;
            var isOnline = lastPacketAt.HasValue && lastPacketAt.Value >= cutoff;

            object? telemetry = null;
            if (latest.Status == "Success" && latest.Data != null)
            {
                telemetry = new
                {
                    latest.Data.ReceivedAtUtc,
                    latest.Data.IsCrcValid,
                    latest.Data.DecodeError,
                    summary = latest.Data.SummaryPayloadJson
                };
            }

            return JsonSerializer.Serialize(new
            {
                deviceId = device.Id,
                name = device.Name,
                code = device.Code,
                configuredStatus = device.Status,
                isOnline,
                lastPacketAtUtc = lastPacketAt,
                aiEhsInstalled = device.AiEhsInstalled,
                aiSecurityInstalled = device.AiSecurityInstalled,
                latestTelemetry = telemetry
            });
        }

        private async Task<string> GetTodaysActivitiesAsync(CancellationToken ct)
        {
            if (!TryCustomer(out var customerId, out var error))
                return error;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var items = await _activities.GetAllAsync()
                .Where(x => x.IsActive && x.Date == today)
                .OrderBy(x => x.StartTime)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.DeviceId,
                    deviceName = x.Device.Name,
                    date = x.Date.ToString("yyyy-MM-dd"),
                    startTime = x.StartTime.ToString("HH:mm"),
                    endTime = x.EndTime.ToString("HH:mm"),
                    x.Team,
                    x.Persons
                })
                .Take(20)
                .ToListAsync(ct);

            return JsonSerializer.Serialize(new
            {
                customerId,
                dateUtc = today.ToString("yyyy-MM-dd"),
                count = items.Count,
                activities = items
            });
        }

        private bool TryCustomer(out long customerId, out string errorJson)
        {
            customerId = _currentUser.GetCustomerId();
            if (customerId <= 0)
            {
                errorJson = JsonSerializer.Serialize(new
                {
                    error = "No valid customer context. Send X-Customer-Id header."
                });
                return false;
            }

            errorJson = string.Empty;
            return true;
        }

        private static string? TryReadGroqError(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("error", out var error) &&
                    error.TryGetProperty("message", out var msg))
                    return msg.GetString();
            }
            catch { }
            return null;
        }

        private static string ToPlainText(string text) =>
            text.Replace("**", string.Empty).Replace("__", string.Empty).Trim();

        private static Response<ChatReplyVM> Ok(string answer) => new()
        {
            Status = "Success",
            Data = new ChatReplyVM { Answer = answer }
        };

        private static Response<ChatReplyVM> Fail(string message) => new()
        {
            Status = "Error",
            Message = new List<string> { message }
        };

        private sealed class GroqCallResult
        {
            public bool Ok { get; init; }
            public string? Content { get; init; }
            public List<GroqToolCall>? ToolCalls { get; init; }
            public string? Error { get; init; }

            public static GroqCallResult Success(string? content, List<GroqToolCall>? toolCalls) => new()
            {
                Ok = true,
                Content = content,
                ToolCalls = toolCalls
            };

            public static GroqCallResult Fail(string error) => new()
            {
                Ok = false,
                Error = error
            };
        }

        private sealed class GroqRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public List<GroqMessage> Messages { get; set; } = new();

            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; }

            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }

            [JsonPropertyName("tools")]
            public List<GroqTool>? Tools { get; set; }

            [JsonPropertyName("tool_choice")]
            public string? ToolChoice { get; set; }
        }

        private sealed class GroqMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string? Content { get; set; }

            [JsonPropertyName("tool_call_id")]
            public string? ToolCallId { get; set; }

            [JsonPropertyName("tool_calls")]
            public List<GroqToolCall>? ToolCalls { get; set; }
        }

        private sealed class GroqToolCall
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("type")]
            public string Type { get; set; } = "function";

            [JsonPropertyName("function")]
            public GroqFunctionCall? Function { get; set; }
        }

        private sealed class GroqFunctionCall
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("arguments")]
            public string Arguments { get; set; } = "{}";
        }

        private sealed class GroqTool
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "function";

            [JsonPropertyName("function")]
            public GroqFunctionDef Function { get; set; } = new();
        }

        private sealed class GroqFunctionDef
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("parameters")]
            public object Parameters { get; set; } = new { type = "object", properties = new { } };
        }

        private static class ChatTools
        {
            public static readonly List<GroqTool> All = new()
            {
                new()
                {
                    Function = new GroqFunctionDef
                    {
                        Name = "GetDeviceCount",
                        Description =
                            "How many devices are listed for the current customer (total, active, inactive). " +
                            "Use for 'How many devices are listed?'"
                    }
                },
                new()
                {
                    Function = new GroqFunctionDef
                    {
                        Name = "GetOnlineDeviceCount",
                        Description =
                            "How many devices are currently online vs offline for the current customer. " +
                            "Use for 'How many devices are online?' Answer with the counts only; do not explain timing windows."
                    }
                },
                new()
                {
                    Function = new GroqFunctionDef
                    {
                        Name = "GetDeviceStatus",
                        Description =
                            "Whether one device is online or offline, plus basic device fields. Prefer the selected device in the request. " +
                            "Use for 'What is the status of a device?' Answer simply with online/offline; do not explain timing windows.",
                        Parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                deviceId = new { type = "integer", description = "Device database id if known" },
                                deviceName = new { type = "string", description = "Device name or code if known" }
                            }
                        }
                    }
                },
                new()
                {
                    Function = new GroqFunctionDef
                    {
                        Name = "GetTodaysActivities",
                        Description =
                            "Activities scheduled for today (UTC date) for the current customer. " +
                            "Use for 'What activities are scheduled today?'"
                    }
                }
            };
        }
    }
}
