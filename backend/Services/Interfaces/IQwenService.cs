using PartySchoolApi.Models.Common;

namespace PartySchoolApi.Services.Interfaces;

/// <summary>
/// 千问(通义千问)大模型客户端服务接口
/// </summary>
public interface IQwenService
{
    /// <summary>是否已配置API Key</summary>
    bool IsConfigured { get; }

    /// <summary>通用AI对话（消息列表）</summary>
    Task<string> ChatAsync(
        IEnumerable<QwenChatMessage> messages,
        double temperature = 0.7,
        bool jsonMode = false,
        int maxTokens = 4096,
        CancellationToken cancellationToken = default);

    /// <summary>通用AI对话（系统提示+用户提示）</summary>
    Task<string> ChatAsync(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.7,
        bool jsonMode = false,
        int maxTokens = 4096,
        CancellationToken cancellationToken = default);

    /// <summary>JSON模式对话，自动解析返回的JSON</summary>
    Task<T?> ChatJsonAsync<T>(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.3,
        int maxTokens = 4096,
        CancellationToken cancellationToken = default) where T : class;
}
