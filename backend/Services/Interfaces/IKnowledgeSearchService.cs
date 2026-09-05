using System.Text.RegularExpressions;

namespace PartySchoolApi.Services.Interfaces;

/// <summary>知识库文档片段</summary>
public class KnowledgeDocument
{
    public string Id { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    /// <summary>BM25相关性分数</summary>
    public double Score { get; set; }
    /// <summary>匹配片段（用于前端显示引用来源）</summary>
    public string? Snippet { get; set; }
}

/// <summary>
/// 本地党建知识库检索服务：加载 knowledge/documents 下的 .txt/.md 文档，
/// 按 BM25 算法召回与问题最相关的片段（与千问 RAG 问答配合使用）。
/// </summary>
public interface IKnowledgeSearchService
{
    /// <summary>已加载的文档片段总数</summary>
    int DocumentCount { get; }

    /// <summary>按相关性召回知识片段</summary>
    IReadOnlyList<KnowledgeDocument> Search(string query, int limit = 5);

    /// <summary>把召回片段拼装为给大模型的参考上下文</summary>
    string BuildContext(IReadOnlyList<KnowledgeDocument> results);
}
