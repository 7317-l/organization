namespace PartySchoolApi.Services.Interfaces;

/// <summary>
/// 统一AI模型调用服务接口
/// 所有AI功能（问答、推荐、报告、内容生成、NL2SQL、聚类等）均通过此接口调用模型
/// 目前为基于规则的模拟实现，后续可替换为真实AI模型API（如OpenAI、通义千问等）
/// </summary>
public interface IAiModelService
{
    /// <summary>
    /// 通用AI对话接口
    /// </summary>
    /// <param name="prompt">提示词</param>
    /// <param name="systemPrompt">系统提示词（可选）</param>
    /// <param name="temperature">温度参数（0-1，默认0.7）</param>
    /// <returns>AI回复内容</returns>
    Task<string> ChatAsync(string prompt, string? systemPrompt = null, double temperature = 0.7);

    /// <summary>
    /// AI党建知识问答
    /// </summary>
    Task<string> QueryKnowledgeAsync(string question);

    /// <summary>
    /// AI内容生成
    /// </summary>
    Task<string> GenerateContentAsync(string topic, string contentType, int length = 500);

    /// <summary>
    /// NL2SQL：自然语言转SQL
    /// </summary>
    Task<string> Nl2SqlAsync(string naturalLanguageQuery);

    /// <summary>
    /// 文本摘要
    /// </summary>
    Task<string> SummarizeAsync(string text, int maxLength = 200);

    /// <summary>
    /// 文本润色
    /// </summary>
    Task<string> PolishTextAsync(string text);
}
