namespace PartySchoolApi.Models.DTOs;

public class AiKnowledgeQueryRequest
{
    public string Question { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public int TopK { get; set; } = 5;
    public bool Rerank { get; set; } = true;
    public string? FilterFile { get; set; }
    /// <summary>端侧角色：member=党员端（学习者视角）；admin=管理端（党务管理者视角）；默认 member</summary>
    public string Role { get; set; } = "member";
}

public class AiKnowledgeQueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<string> SourceReferences { get; set; } = new();
    public double Confidence { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public List<RagResultItem> Results { get; set; } = new();
}

/// <summary>流式问答首帧元数据（先于回答增量下发）</summary>
public class AiKnowledgeStreamMeta
{
    public string SessionId { get; set; } = string.Empty;
    public List<string> SourceReferences { get; set; } = new();
    public double Confidence { get; set; }
    public List<RagResultItem> Results { get; set; } = new();
    /// <summary>问题类型：knowledge=知识问答（显示引用/置信度）；chat=寒暄闲聊（不显示）</summary>
    public string QuestionType { get; set; } = "knowledge";
}

/// <summary>流式问答内部上下文（检索结果 + 提示词 + 兜底回答）</summary>
public class AiKnowledgeStreamContext
{
    public AiKnowledgeStreamMeta Meta { get; set; } = new();
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    /// <summary>非空时表示走兜底回答，不再调用模型</summary>
    public string? FallbackAnswer { get; set; }
}

/// <summary>错题分析请求（AI 感应当前题目后，对单题做深度解析）</summary>
public class AiQuestionAnalyzeRequest
{
    /// <summary>题干</summary>
    public string Question { get; set; } = string.Empty;
    /// <summary>选项列表（选项文本）</summary>
    public List<string> Options { get; set; } = new();
    /// <summary>党员的作答（字母，如 "A" / "AB" / "正确"）</summary>
    public string? UserAnswer { get; set; }
    /// <summary>正确答案（字母，如 "B" / "AB" / "正确"）</summary>
    public string? CorrectAnswer { get; set; }
    /// <summary>所属知识点/章节（可选）</summary>
    public string? KnowledgePoint { get; set; }
    public string? SessionId { get; set; }
}

/// <summary>错题分析内部上下文（提示词 + 元数据）</summary>
public class AiAnalyzeStreamContext
{
    public AiKnowledgeStreamMeta Meta { get; set; } = new();
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public string? FallbackAnswer { get; set; }
}
