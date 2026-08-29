namespace PartySchoolApi.Services.Interfaces;

/// <summary>千问(通义千问)大模型对话消息</summary>
public class QwenChatMessage
{
    /// <summary>角色：system / user / assistant</summary>
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;

    public QwenChatMessage() { }

    public QwenChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }

    public static QwenChatMessage System(string content) => new("system", content);
    public static QwenChatMessage User(string content) => new("user", content);
    public static QwenChatMessage Assistant(string content) => new("assistant", content);
}

/// <summary>千问(通义千问)大模型客户端服务</summary>
public interface IQwenService
{
    /// <summary>是否已配置有效 API Key</summary>
    bool IsConfigured { get; }

    /// <summary>发起多轮对话，返回模型文本回答</summary>
    Task<string> ChatAsync(
        IEnumerable<QwenChatMessage> messages,
        double temperature = 0.7,
        bool jsonMode = false,
        int maxTokens = 4096,
        CancellationToken cancellationToken = default);

    /// <summary>单轮对话（system + user）</summary>
    Task<string> ChatAsync(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.7,
        bool jsonMode = false,
        int maxTokens = 4096,
        CancellationToken cancellationToken = default);

    /// <summary>请求模型返回严格 JSON，并解析为目标类型；解析失败返回 default</summary>
    Task<T?> ChatJsonAsync<T>(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.3,
        int maxTokens = 4096,
        CancellationToken cancellationToken = default) where T : class;
}
