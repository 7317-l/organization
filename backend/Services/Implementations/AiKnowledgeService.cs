using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>党建知识库问答服务（占位实现，预留RAG接入）</summary>
public class AiKnowledgeService : IAiKnowledgeService
{
    // 预设知识库（占位）
    private static readonly Dictionary<string, string> KnowledgeBase = new()
    {
        ["入党誓词"] = "我志愿加入中国共产党，拥护党的纲领，遵守党的章程，履行党员义务，执行党的决定，严守党的纪律，保守党的秘密，对党忠诚，积极工作，为共产主义奋斗终身，随时准备为党和人民牺牲一切，永不叛党。",
        ["党的宗旨"] = "中国共产党的根本宗旨是全心全意为人民服务。",
        ["党的性质"] = "中国共产党是中国工人阶级的先锋队，同时是中国人民和中华民族的先锋队，是中国特色社会主义事业的领导核心。",
        ["四个意识"] = "政治意识、大局意识、核心意识、看齐意识。",
        ["四个自信"] = "道路自信、理论自信、制度自信、文化自信。",
        ["两个维护"] = "坚决维护习近平总书记党中央的核心、全党的核心地位，坚决维护党中央权威和集中统一领导。",
        ["三会一课"] = "支部党员大会、支部委员会、党小组会和党课。",
        ["两学一做"] = "学党章党规、学系列讲话，做合格党员。",
        ["不忘初心"] = "不忘初心，方得始终。中国共产党人的初心和使命，就是为中国人民谋幸福，为中华民族谋复兴。"
    };

    public Task<AiKnowledgeQueryResponse> QueryAsync(AiKnowledgeQueryRequest request)
    {
        var question = request.Question.Trim();
        string answer = "抱歉，暂未找到相关知识。建议您查阅党章或相关学习资料。";
        double confidence = 0.3;
        var references = new List<string>();

        // 关键词匹配（占位RAG）
        foreach (var kv in KnowledgeBase)
        {
            if (question.Contains(kv.Key) || kv.Key.Contains(question))
            {
                answer = kv.Value;
                confidence = 0.95;
                references.Add($"《党建知识库》- {kv.Key}");
                break;
            }
        }

        var sessionId = string.IsNullOrEmpty(request.SessionId)
            ? Guid.NewGuid().ToString("N")
            : request.SessionId;

        return Task.FromResult(new AiKnowledgeQueryResponse
        {
            Answer = answer,
            SourceReferences = references,
            Confidence = confidence,
            SessionId = sessionId
        });
    }
}
