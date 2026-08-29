using System.Net.Http.Headers;
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
