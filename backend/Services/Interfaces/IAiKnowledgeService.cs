using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Services.Interfaces;

public interface IAiKnowledgeService
{
    Task<AiKnowledgeQueryResponse> QueryAsync(AiKnowledgeQueryRequest request);

    /// <summary>流式问答预处理：完成检索/重排并构造提示词，供 SSE 端点先下发元数据再流式输出回答</summary>
    Task<AiKnowledgeStreamContext> PrepareStreamAsync(AiKnowledgeQueryRequest request);

    /// <summary>错题分析预处理：构造单题深度解析的提示词，供 SSE 端点流式输出分析</summary>
    Task<AiAnalyzeStreamContext> PrepareAnalyzeStreamAsync(AiQuestionAnalyzeRequest request);
}
