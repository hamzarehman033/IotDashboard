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
        private readonly OllamaConfigs _ollama;
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
            IOptions<OllamaConfigs> ollama,
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
            _ollama = ollama.Value;
            _ollama.ApiKey = (_ollama.ApiKey ?? string.Empty).Trim();
            _ollama.BaseUrl = (_ollama.BaseUrl ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(_ollama.ModelId))
                _ollama.ModelId = "qwen2.5-coder:1.5b";
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

            if (message.Length > _ollama.MaxMessageLength)
                return Fail($"Message must be at most {_ollama.MaxMessageLength} characters.");

            if (string.IsNullOrWhiteSpace(_ollama.BaseUrl))
                return Fail("Ollama BaseUrl is missing. Set Ollama__BaseUrl in Azure App Settings.");

            request ??= new ChatRequestVM();
            var messages = BuildMessages(request, message);
            var useTools = LooksLikeLiveDataQuestion(message);


            try
            {
                var first = await CallLlmAsync(messages, includeTools: useTools, ct);
                if (!first.Ok)
                    return Fail(first.Error!);

                // Small models sometimes emit a tool call as plain text JSON instead of tool_calls.
                if ((first.ToolCalls is null || first.ToolCalls.Count == 0) &&
                    TryParseTextToolCall(first.Content, out var textCall))
                {
                    first = LlmCallResult.Success(null, new List<LlmToolCall> { textCall });
                }

                if (first.ToolCalls is { Count: > 0 })
                {
                    foreach (var tc in first.ToolCalls)
                    {
                        if (string.IsNullOrWhiteSpace(tc.Id))
                            tc.Id = $"call_{Guid.NewGuid():N}";
                    }

                    var call = first.ToolCalls[0];
                    var toolName = call.Function?.Name;
                    if (!ChatTools.IsKnown(toolName))
                    {
                        // Invented tool (e.g. GetApplicationDescription) — answer from FAQ without tools.
                        messages.Add(new LlmMessage
                        {
                            Role = "assistant",
                            Content = "I should answer from the FAQ in plain text without calling tools."
                        });
                        messages.Add(new LlmMessage
                        {
                            Role = "user",
                            Content = message
                        });
                        var faqOnly = await CallLlmAsync(messages, includeTools: false, ct);
                        if (!faqOnly.Ok)
                            return Fail(faqOnly.Error!);
                        if (string.IsNullOrWhiteSpace(faqOnly.Content) || LooksLikeToolJson(faqOnly.Content))
                            return Ok(GetFaqFallbackAnswer(message));
                        return Ok(ToPlainText(faqOnly.Content));
                    }

                    var toolResult = await ExecuteToolAsync(
                        toolName,
                        call.Function?.Arguments,
                        request,
                        ct);

                    messages.Add(new LlmMessage
                    {
                        Role = "assistant",
                        Content = first.Content,
                        ToolCalls = first.ToolCalls
                    });
                    messages.Add(new LlmMessage
                    {
                        Role = "tool",
                        ToolCallId = call.Id,
                        Name = toolName,
                        Content = toolResult
                    });

                    var second = await CallLlmAsync(messages, includeTools: false, ct);
                    if (!second.Ok)
                        return Fail(second.Error!);

                    if (string.IsNullOrWhiteSpace(second.Content) || LooksLikeToolJson(second.Content))
                        return Fail("No answer was returned. Please try again.");

                    return Ok(ToPlainText(second.Content));
                }

                if (string.IsNullOrWhiteSpace(first.Content) || LooksLikeToolJson(first.Content))
                {
                    if (useTools)
                    {
                        var faqOnly = await CallLlmAsync(messages, includeTools: false, ct);
                        if (faqOnly.Ok && !string.IsNullOrWhiteSpace(faqOnly.Content) && !LooksLikeToolJson(faqOnly.Content))
                            return Ok(ToPlainText(faqOnly.Content));
                    }

                    return Ok(GetFaqFallbackAnswer(message));
                }

                return Ok(ToPlainText(first.Content));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat request failed");
                return Fail(MapException(ex, ct));
            }
        }

        private List<LlmMessage> BuildMessages(ChatRequestVM request, string message)
        {
            var contextHint = string.Empty;
            if (request.DeviceId.HasValue || !string.IsNullOrWhiteSpace(request.DeviceName))
            {
                contextHint =
                    $"\nSelected device context: id={request.DeviceId?.ToString() ?? "none"}, name={request.DeviceName ?? "none"}. " +
                    "Use this when calling GetDeviceStatus if the user asks about a device status.\n";
            }

            return new List<LlmMessage>
            {
                new()
                {
                    Role = "system",
                    Content =
                        "You are the IoT Dashboard help assistant.\n" +
                        "For how-to and definition questions (including what this application is), answer from the FAQ in plain text.\n" +
                        "For live data only, call exactly one tool from the provided tool list.\n" +
                        "Never invent tool names. Never write tool calls as JSON text in your reply.\n" +
                        "Never invent numbers or status. If a device-specific question has no device selected, ask the user to select a device.\n" +
                        "For device status answers: say the device name, whether it is online or offline, and key fields if present. " +
                        "Do not mention a 10-minute window, online window, Active flag, isActive, or that telemetry data is unavailable.\n" +
                        "Reply in plain text only. Do not use markdown: no **, __, *, #, or backticks.\n" +
                        contextHint + "\n" +
                        _faq
                }
            }
            .Concat(BuildHistory(request))
            .Append(new LlmMessage { Role = "user", Content = message })
            .ToList();
        }

        private static IEnumerable<LlmMessage> BuildHistory(ChatRequestVM request)
        {
            foreach (var h in (request.History ?? new()).TakeLast(6))
            {
                var role = h.Role?.Trim().ToLowerInvariant();
                if ((role == "user" || role == "assistant") && !string.IsNullOrWhiteSpace(h.Content))
                    yield return new LlmMessage { Role = role, Content = h.Content.Trim() };
            }
        }

        private async Task<LlmCallResult> CallLlmAsync(List<LlmMessage> messages, bool includeTools, CancellationToken ct)
        {
            var models = new List<string> { _ollama.ModelId };
            if (!string.IsNullOrWhiteSpace(_ollama.FallbackModelId) &&
                !string.Equals(_ollama.FallbackModelId, _ollama.ModelId, StringComparison.OrdinalIgnoreCase))
                models.Add(_ollama.FallbackModelId);

            LlmCallResult? lastFail = null;
            foreach (var model in models)
            {
                for (var attempt = 0; attempt < 3; attempt++)
                {
                    if (attempt > 0)
                    {
                        var delayMs = 400 * (1 << (attempt - 1)) + Random.Shared.Next(0, 250);
                        _logger.LogInformation(
                            "Retrying Ollama model {Model} (attempt {Attempt})",
                            model, attempt + 1);
                        await Task.Delay(delayMs, ct);
                    }

                    var result = await CallLlmOnceAsync(messages, includeTools, model, ct);
                    if (result.Ok)
                        return result;

                    lastFail = result;
                    if (!IsTransientCapacityFailure(result.Error))
                        return result;
                }
            }

            return lastFail ?? LlmCallResult.Fail("Ollama is temporarily unavailable. Please try again shortly.");
        }

        private async Task<LlmCallResult> CallLlmOnceAsync(
            List<LlmMessage> messages,
            bool includeTools,
            string modelId,
            CancellationToken ct)
        {
            var body = new LlmChatRequest
            {
                Model = modelId,
                Messages = messages,
                MaxTokens = _ollama.MaxTokens,
                Temperature = 0.2,
                Tools = includeTools ? ChatTools.All : null,
                ToolChoice = includeTools ? "auto" : null
            };

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
                {
                    Content = JsonContent.Create(body, options: JsonOptions)
                };
                if (!string.IsNullOrWhiteSpace(_ollama.ApiKey))
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _ollama.ApiKey);

                using var res = await _http.SendAsync(req, ct);
                var json = await res.Content.ReadAsStringAsync(ct);

                if (!res.IsSuccessStatusCode)
                    return LlmCallResult.Fail(MapHttpError(res.StatusCode, json));

                using var doc = JsonDocument.Parse(json);
                var choice = doc.RootElement.GetProperty("choices")[0].GetProperty("message");
                var content = choice.TryGetProperty("content", out var c) && c.ValueKind != JsonValueKind.Null
                    ? c.GetString()
                    : null;

                List<LlmToolCall>? toolCalls = null;
                if (choice.TryGetProperty("tool_calls", out var toolsEl) && toolsEl.ValueKind == JsonValueKind.Array)
                    toolCalls = JsonSerializer.Deserialize<List<LlmToolCall>>(toolsEl.GetRawText(), JsonOptions);

                return LlmCallResult.Success(content, toolCalls);
            }
            catch (Exception ex) when (IsNetworkFailure(ex) || IsTimeout(ex, ct))
            {
                _logger.LogError(ex, "Unable to reach Ollama");
                return LlmCallResult.Fail(MapException(ex, ct));
            }
        }

        private static bool IsTransientCapacityFailure(string? error)
        {
            if (string.IsNullOrWhiteSpace(error))
                return false;

            return error.Contains("high demand", StringComparison.OrdinalIgnoreCase)
                || error.Contains("rate limit", StringComparison.OrdinalIgnoreCase)
                || error.Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase)
                || error.Contains("overloaded", StringComparison.OrdinalIgnoreCase)
                || error.Contains("UNAVAILABLE", StringComparison.OrdinalIgnoreCase)
                || error.Contains("try again", StringComparison.OrdinalIgnoreCase);
        }

        private string MapHttpError(System.Net.HttpStatusCode statusCode, string json)
        {
            _logger.LogWarning("Ollama failed: {Status} {Body}", (int)statusCode, json);
            var detail = TryReadLlmError(json);

            return statusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized =>
                    "Invalid Ollama API key. Check Ollama__ApiKey if your container requires auth.",
                System.Net.HttpStatusCode.Forbidden =>
                    string.IsNullOrWhiteSpace(detail)
                        ? "Ollama access denied (HTTP 403)."
                        : $"Ollama access denied: {detail}",
                System.Net.HttpStatusCode.TooManyRequests =>
                    string.IsNullOrWhiteSpace(detail)
                        ? "Ollama rate limit reached. Please try again shortly."
                        : detail!,
                System.Net.HttpStatusCode.NotFound =>
                    string.IsNullOrWhiteSpace(detail)
                        ? $"Ollama model '{_ollama.ModelId}' was not found. Pull it on the container (e.g. ollama pull {_ollama.ModelId}) and set Ollama__ModelId."
                        : detail!,
                System.Net.HttpStatusCode.BadRequest when LooksLikeKeyOrAuthError(detail) =>
                    "Ollama rejected authentication. Check Ollama__ApiKey.",
                System.Net.HttpStatusCode.BadRequest =>
                    string.IsNullOrWhiteSpace(detail)
                        ? "Ollama rejected the request (HTTP 400)."
                        : detail!,
                System.Net.HttpStatusCode.BadGateway or
                System.Net.HttpStatusCode.ServiceUnavailable or
                System.Net.HttpStatusCode.GatewayTimeout =>
                    string.IsNullOrWhiteSpace(detail)
                        ? "Ollama is temporarily unavailable. Please try again shortly."
                        : detail!,
                _ => string.IsNullOrWhiteSpace(detail)
                    ? $"Ollama request failed (HTTP {(int)statusCode})."
                    : detail!
            };
        }

        private string MapException(Exception ex, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return "The chat request was cancelled.";

            if (IsTimeout(ex, ct))
                return $"Timed out while reaching Ollama. Check {_ollama.BaseUrl} and increase Ollama__TimeoutSeconds if the model is slow.";

            if (IsNetworkFailure(ex))
                return $"Cannot reach Ollama (network error). Check App Service outbound access to {_ollama.BaseUrl}.";

            return _env.IsDevelopment()
                ? $"Chat failed: {ex.Message}"
                : "Unable to get an answer right now. Please try again later.";
        }

        private static bool IsTimeout(Exception ex, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return false;

            return ex is TaskCanceledException or TimeoutException
                || ex.InnerException is TaskCanceledException or TimeoutException;
        }

        private static bool IsNetworkFailure(Exception ex)
        {
            for (var current = ex; current != null; current = current.InnerException)
            {
                if (current is HttpRequestException or System.Net.Sockets.SocketException or System.Net.Http.HttpIOException)
                    return true;

                var name = current.GetType().FullName ?? string.Empty;
                if (name.Contains("Socket", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Network", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool LooksLikeKeyOrAuthError(string? detail)
        {
            if (string.IsNullOrWhiteSpace(detail))
                return false;

            return detail.Contains("api key", StringComparison.OrdinalIgnoreCase)
                || detail.Contains("invalid key", StringComparison.OrdinalIgnoreCase)
                || detail.Contains("authentication", StringComparison.OrdinalIgnoreCase)
                || detail.Contains("unauthorized", StringComparison.OrdinalIgnoreCase);
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

        private static string? TryReadLlmError(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("error", out var error))
                    return null;

                if (error.ValueKind == JsonValueKind.String)
                    return error.GetString();

                if (error.TryGetProperty("message", out var msg))
                    return msg.GetString();
            }
            catch { }
            return null;
        }

        private static string ToPlainText(string text) =>
            text.Replace("**", string.Empty).Replace("__", string.Empty).Trim();

        private static bool LooksLikeLiveDataQuestion(string message)
        {
            var m = message.ToLowerInvariant();
            return m.Contains("how many")
                || m.Contains("online")
                || m.Contains("offline")
                || m.Contains("status of")
                || m.Contains("device status")
                || m.Contains("listed")
                || m.Contains("activit")
                || m.Contains("scheduled today")
                || m.Contains("devices are");
        }

        private static bool LooksLikeToolJson(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var t = text.Trim();
            return (t.StartsWith('{') && t.Contains("\"name\"", StringComparison.Ordinal) &&
                    (t.Contains("\"arguments\"", StringComparison.Ordinal) || t.Contains("\"parameters\"", StringComparison.Ordinal)))
                || t.Contains("\"tool_calls\"", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseTextToolCall(string? content, out LlmToolCall toolCall)
        {
            toolCall = new LlmToolCall();
            if (!LooksLikeToolJson(content))
                return false;

            try
            {
                using var doc = JsonDocument.Parse(content!);
                var root = doc.RootElement;
                if (!root.TryGetProperty("name", out var nameEl))
                    return false;

                var name = nameEl.GetString();
                if (string.IsNullOrWhiteSpace(name))
                    return false;

                var args = "{}";
                if (root.TryGetProperty("arguments", out var argsEl))
                {
                    args = argsEl.ValueKind == JsonValueKind.String
                        ? (argsEl.GetString() ?? "{}")
                        : argsEl.GetRawText();
                }

                toolCall = new LlmToolCall
                {
                    Id = $"call_{Guid.NewGuid():N}",
                    Type = "function",
                    Function = new LlmFunctionCall { Name = name, Arguments = args }
                };
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetFaqFallbackAnswer(string message)
        {
            var m = message.ToLowerInvariant();
            if (m.Contains("what is this application") || m.Contains("what is the application") ||
                m.Contains("what does this app") || (m.Contains("application") && m.Contains("what")))
            {
                return
                    "This application is an IoT dashboard for remote monitoring of telecom sites (RMS). " +
                    "Users can manage customers, tenants, locations, and devices; view live and historical telemetry " +
                    "(power, battery, solar, grid, generator, environment, alarms); download status reports; " +
                    "manage scheduled field activities; and view optional AI camera vision events for EHS and Security.";
            }

            return
                "I can help with how-to questions from the product FAQ, and live questions about device counts, " +
                "online status, a selected device status, or today's activities. Please rephrase your question.";
        }

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

        private sealed class LlmCallResult
        {
            public bool Ok { get; init; }
            public string? Content { get; init; }
            public List<LlmToolCall>? ToolCalls { get; init; }
            public string? Error { get; init; }

            public static LlmCallResult Success(string? content, List<LlmToolCall>? toolCalls) => new()
            {
                Ok = true,
                Content = content,
                ToolCalls = toolCalls
            };

            public static LlmCallResult Fail(string error) => new()
            {
                Ok = false,
                Error = error
            };
        }

        private sealed class LlmChatRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public List<LlmMessage> Messages { get; set; } = new();

            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; }

            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }

            [JsonPropertyName("tools")]
            public List<LlmTool>? Tools { get; set; }

            [JsonPropertyName("tool_choice")]
            public string? ToolChoice { get; set; }
        }

        private sealed class LlmMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string? Content { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("tool_call_id")]
            public string? ToolCallId { get; set; }

            [JsonPropertyName("tool_calls")]
            public List<LlmToolCall>? ToolCalls { get; set; }
        }

        private sealed class LlmToolCall
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("type")]
            public string Type { get; set; } = "function";

            [JsonPropertyName("function")]
            public LlmFunctionCall? Function { get; set; }
        }

        private sealed class LlmFunctionCall
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("arguments")]
            public string Arguments { get; set; } = "{}";
        }

        private sealed class LlmTool
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "function";

            [JsonPropertyName("function")]
            public LlmFunctionDef Function { get; set; } = new();
        }

        private sealed class LlmFunctionDef
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
            private static readonly HashSet<string> KnownNames = new(StringComparer.OrdinalIgnoreCase)
            {
                "GetDeviceCount",
                "GetOnlineDeviceCount",
                "GetDeviceStatus",
                "GetTodaysActivities"
            };

            public static bool IsKnown(string? name) =>
                !string.IsNullOrWhiteSpace(name) && KnownNames.Contains(name);

            public static readonly List<LlmTool> All = new()
            {
                new()
                {
                    Function = new LlmFunctionDef
                    {
                        Name = "GetDeviceCount",
                        Description =
                            "How many devices are listed for the current customer (total, active, inactive). " +
                            "Use for 'How many devices are listed?'"
                    }
                },
                new()
                {
                    Function = new LlmFunctionDef
                    {
                        Name = "GetOnlineDeviceCount",
                        Description =
                            "How many devices are currently online vs offline for the current customer. " +
                            "Use for 'How many devices are online?' Answer with the counts only; do not explain timing windows."
                    }
                },
                new()
                {
                    Function = new LlmFunctionDef
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
                    Function = new LlmFunctionDef
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
