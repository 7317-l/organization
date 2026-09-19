using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// 千问(通义千问)大模型客户端服务
/// 通过 DashScope OpenAI 兼容模式调用，配置项见 appsettings.json 的 Qwen 节点。
/// </summary>
public class QwenService : IQwenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<QwenService> _logger;

    private readonly string _baseUrl;
    private readonly string _model;
    private readonly string _apiKey;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public QwenService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<QwenService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;

        _baseUrl = (configuration["Qwen:BaseUrl"] ?? "https://dashscope.aliyuncs.com/compatible-mode/v1")
            .TrimEnd('/');
        _model = configuration["Qwen:Model"] ?? "qwen-plus";
        _apiKey = ResolveApiKey(configuration);
    }

    /// <summary>
    /// API Key 解析顺序：appsettings → 环境变量 DASHSCOPE_API_KEY → 仓库根 .env 文件。
    /// 与千问 AI 模块共用同一份 .env，用户只需维护一处密钥。
    /// </summary>
    private static string ResolveApiKey(IConfiguration configuration)
    {
        var fromConfig = configuration["Qwen:ApiKey"];
        if (!string.IsNullOrWhiteSpace(fromConfig) && fromConfig != "sk-XXXX")
            return fromConfig.Trim();

        var fromEnv = Environment.GetEnvironmentVariable("DASHSCOPE_API_KEY");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv.Trim();

        var envFile = FindEnvFile();
        if (envFile != null)
        {
            try
            {
                foreach (var line in File.ReadAllLines(envFile))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("DASHSCOPE_API_KEY=", StringComparison.OrdinalIgnoreCase))
                    {
                        var value = trimmed.Substring("DASHSCOPE_API_KEY=".Length).Trim();
                        if (value.StartsWith("\"") && value.EndsWith("\""))
                            value = value.Substring(1, value.Length - 2);
                        if (!string.IsNullOrWhiteSpace(value))
                            return value.Trim();
                    }
                }
            }
            catch
            {
                // 忽略 .env 读取失败
            }
        }

        return string.Empty;
    }

    private static string? FindEnvFile()
    {
        // 从当前工作目录向上最多 5 层查找 .env
        var start = new DirectoryInfo(Environment.CurrentDirectory);
        var dir = start;
        for (var i = 0; i < 6 && dir != null; i++)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }

        // 再从程序基目录向上查找
        dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 6 && dir != null; i++)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }

        return null;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey) && _apiKey != "sk-XXXX";

    public async Task<string> ChatAsync(
        IEnumerable<QwenChatMessage> messages,
        double temperature = 0.7,
        bool jsonMode = false,
        int maxTokens = 4096,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("尚未配置千问 API Key，请在 appsettings.json 或环境变量 DASHSCOPE_API_KEY 中填写。");
        }

        var client = _httpClientFactory.CreateClient("Qwen");
        client.Timeout = TimeSpan.FromSeconds(90);

        var payload = new Dictionary<string, object>
        {
            ["model"] = _model,
            ["messages"] = messages.Select(m => new { role = m.Role, content = m.Content }),
            ["temperature"] = temperature,
            ["max_tokens"] = maxTokens,
            ["stream"] = false
        };

        if (jsonMode)
        {
            // DashScope 兼容模式支持 response_format = { type = "json_object" }
            payload["response_format"] = new { type = "json_object" };
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOpts),
            Encoding.UTF8,
            "application/json");

        using var response = await client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("千问 API 调用失败：{Status} {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException($"千问 API 调用失败（{(int)response.StatusCode}）：{Truncate(body, 300)}");
        }

        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("choices", out var choices)
            || choices.ValueKind != JsonValueKind.Array
            || choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("千问 API 返回为空。");
        }

        var content = choices[0].TryGetProperty("message", out var msg)
                      && msg.TryGetProperty("content", out var c)
            ? c.GetString()
            : null;

        return content?.Trim() ?? string.Empty;
    }

    public Task<string> ChatAsync(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.7,
        bool jsonMode = false,
        int maxTokens = 4096,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<QwenChatMessage>();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(QwenChatMessage.System(systemPrompt));
        messages.Add(QwenChatMessage.User(userPrompt));
        return ChatAsync(messages, temperature, jsonMode, maxTokens, cancellationToken);
    }

    public async Task<T?> ChatJsonAsync<T>(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.3,
        int maxTokens = 4096,
        CancellationToken cancellationToken = default) where T : class
    {
        var raw = await ChatAsync(systemPrompt, userPrompt, temperature, jsonMode: true, maxTokens, cancellationToken);
        return TryParseJson<T>(raw);
    }

    /// <summary>
    /// 流式对话：请求 stream=true，逐块读取 DashScope SSE 返回，产出 content 增量。
    /// </summary>
    public async IAsyncEnumerable<string> ChatStreamAsync(
        IEnumerable<QwenChatMessage> messages,
        double temperature = 0.7,
        int maxTokens = 4096,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("尚未配置千问 API Key，请在 appsettings.json 或环境变量 DASHSCOPE_API_KEY 中填写。");
        }

        var client = _httpClientFactory.CreateClient("Qwen");
        // 流式请求整体不设固定超时（避免截断长回答），但用分阶段超时兜底断流：
        // ① 等待响应头（首帧）最多 60 秒；② 后续每行数据最多 45 秒无新内容视为断流
        client.Timeout = Timeout.InfiniteTimeSpan;

        var payload = new Dictionary<string, object>
        {
            ["model"] = _model,
            ["messages"] = messages.Select(m => new { role = m.Role, content = m.Content }),
            ["temperature"] = temperature,
            ["max_tokens"] = maxTokens,
            ["stream"] = true
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOpts),
            Encoding.UTF8,
            "application/json");

        using var headCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        headCts.CancelAfter(TimeSpan.FromSeconds(60));
        using var response = await SafeSendWithTimeoutAsync(client, request, headCts.Token);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("千问流式调用失败：{Status} {Body}", (int)response.StatusCode, errorBody);
            throw new InvalidOperationException($"千问 API 调用失败（{(int)response.StatusCode}）：{Truncate(errorBody, 300)}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 45 秒未收到任何新数据视为断流（网络中断/千问侧挂起），及时让前端感知
            using var readCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readCts.CancelAfter(TimeSpan.FromSeconds(45));
            string line;
            try
            {
                line = await reader.ReadLineAsync(readCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("千问流式响应超时（45秒无数据），请检查网络后重试");
            }
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;

            var data = line.Substring(5).Trim();
            if (data == "[DONE]") break;

            if (TryGetDeltaPiece(data, out var piece) && !string.IsNullOrWhiteSpace(piece))
                yield return piece;
        }
    }

    /// <summary>带首帧超时的 SendAsync，超时时抛出可读错误而非无限挂起</summary>
    private static async Task<HttpResponseMessage> SafeSendWithTimeoutAsync(
        HttpClient client, HttpRequestMessage request, CancellationToken token)
    {
        try
        {
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            throw new TimeoutException("千问服务响应超时（60秒未返回），请检查网络后重试");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
    }

    /// <summary>从 DashScope SSE data 帧中提取 content 增量；无法解析时返回 false</summary>
    private static bool TryGetDeltaPiece(string data, out string? piece)
    {
        piece = null;
        try
        {
            using var doc = JsonDocument.Parse(data);
            if (!doc.RootElement.TryGetProperty("choices", out var choices)
                || choices.ValueKind != JsonValueKind.Array
                || choices.GetArrayLength() == 0)
                return false;

            if (!choices[0].TryGetProperty("delta", out var delta)) return false;
            if (!delta.TryGetProperty("content", out var contentPiece)
                || contentPiece.ValueKind != JsonValueKind.String)
                return false;

            piece = contentPiece.GetString();
            return !string.IsNullOrWhiteSpace(piece);
        }
        catch (JsonException)
        {
            // 忽略无法解析的中间帧（如 usage 统计帧）
            return false;
        }
    }

    private static T? TryParseJson<T>(string raw) where T : class
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        // 提取首个 JSON 对象/数组（兼容模型偶尔输出多余文字）
        var start = raw.IndexOf('{');
        var arrStart = raw.IndexOf('[');
        if (start < 0 && arrStart < 0) return null;
        var open = start >= 0 && (arrStart < 0 || start < arrStart) ? '{' : '[';
        var begin = open == '{' ? start : arrStart;
        var end = open == '{' ? raw.LastIndexOf('}') : raw.LastIndexOf(']');
        if (begin < 0 || end <= begin) return null;
        var json = raw.Substring(begin, end - begin + 1);

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }
        catch
        {
            return null;
        }
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s.Substring(0, max) + "...";
}
