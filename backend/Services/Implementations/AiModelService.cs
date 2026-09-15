using PartySchoolApi.Services.Interfaces;
using System.Text;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// 统一AI模型调用服务实现
/// 优先使用通义千问(Qwen)大模型，其次使用配置的OpenAI兼容API，最后回退到基于规则的模拟
/// </summary>
public class AiModelService : IAiModelService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IQwenService _qwen;

    public AiModelService(IConfiguration configuration, IHttpClientFactory httpClientFactory, IQwenService qwen)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _qwen = qwen;
    }

    /// <summary>
    /// 通用AI对话接口
    /// 优先使用千问，其次使用配置的OpenAI兼容API，最后使用基于规则的模拟
    /// </summary>
    public async Task<string> ChatAsync(string prompt, string? systemPrompt = null, double temperature = 0.7)
    {
        // 优先使用千问大模型
        if (_qwen.IsConfigured)
        {
            try
            {
                return await _qwen.ChatAsync(systemPrompt ?? "你是一位专业的AI助手。", prompt, temperature);
            }
            catch
            {
                // 千问调用失败，继续尝试其他方式
            }
        }

        var apiKey = _configuration["Ai:ApiKey"];
        var apiEndpoint = _configuration["Ai:Endpoint"];

        // 如果配置了其他真实AI模型API，则调用
        if (!string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiEndpoint))
        {
            return await CallRealModelAsync(prompt, systemPrompt, apiEndpoint, apiKey, temperature);
        }

        // 否则使用基于规则的模拟
        return await Task.FromResult(SimulateAiResponse(prompt, systemPrompt));
    }

    /// <summary>
    /// AI党建知识问答
    /// </summary>
    public async Task<string> QueryKnowledgeAsync(string question)
    {
        var systemPrompt = "你是一位专业的党建知识助手，请准确回答关于中国共产党党史、党章、党的理论、党的政策等方面的问题。";
        return await ChatAsync(question, systemPrompt, 0.3);
    }

    /// <summary>
    /// AI内容生成
    /// </summary>
    public async Task<string> GenerateContentAsync(string topic, string contentType, int length = 500)
    {
        var prompt = $"请以{contentType}的形式，围绕\"{topic}\"生成一篇约{length}字的党建学习内容，要求内容准确、结构清晰、语言规范。";
        var systemPrompt = "你是一位专业的党建内容编辑，擅长撰写党史、党章、党的理论等方面的学习材料。";
        return await ChatAsync(prompt, systemPrompt, 0.8);
    }

    /// <summary>
    /// NL2SQL：自然语言转SQL
    /// </summary>
    public async Task<string> Nl2SqlAsync(string naturalLanguageQuery)
    {
        var prompt = $"请将以下自然语言查询转换为SQL语句：{naturalLanguageQuery}\n\n数据库表包括：partymembers(党员), organizations(组织), learningcontents(学习内容), learningtasks(学习任务), memberlearningprogress(学习进度), membertestrecords(考试记录), questions(题目), exampapers(试卷), examtests(测验)。";
        var systemPrompt = "你是一位专业的数据库工程师，擅长将自然语言转换为SQL查询语句。只返回SQL语句，不要返回其他内容。";
        return await ChatAsync(prompt, systemPrompt, 0.1);
    }

    /// <summary>
    /// 文本摘要
    /// </summary>
    public async Task<string> SummarizeAsync(string text, int maxLength = 200)
    {
        var prompt = $"请将以下文本摘要为不超过{maxLength}字的内容：\n\n{text}";
        return await ChatAsync(prompt, null, 0.3);
    }

    /// <summary>
    /// 文本润色
    /// </summary>
    public async Task<string> PolishTextAsync(string text)
    {
        var prompt = $"请润色以下文本，使其更加通顺、规范、专业：\n\n{text}";
        var systemPrompt = "你是一位专业的文字编辑，擅长润色和优化中文文本。";
        return await ChatAsync(prompt, systemPrompt, 0.5);
    }

    /// <summary>
    /// 调用真实AI模型API（预留实现）
    /// </summary>
    private async Task<string> CallRealModelAsync(string prompt, string? systemPrompt, string endpoint, string apiKey, double temperature)
    {
        try
        {
            var requestBody = new
            {
                model = _configuration["Ai:Model"] ?? "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt ?? "你是一位专业的AI助手。" },
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                max_tokens = 2000
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            // 解析OpenAI格式的响应
            using var doc = System.Text.Json.JsonDocument.Parse(responseContent);
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                return choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            }
            return responseContent;
        }
        catch (Exception ex)
        {
            // 真实模型调用失败时，回退到模拟
            return $"[AI模型调用失败，已回退到模拟模式] {SimulateAiResponse(prompt, systemPrompt)}";
        }
    }

    /// <summary>
    /// 基于规则的AI响应模拟
    /// </summary>
    private string SimulateAiResponse(string prompt, string? systemPrompt)
    {
        // 党建知识问答模拟
        if (prompt.Contains("四个意识"))
            return "\"四个意识\"是指政治意识、大局意识、核心意识、看齐意识。这是中国共产党加强党的建设的重要要求，也是党员干部必须具备的政治素养。";
        if (prompt.Contains("两个维护"))
            return "\"两个维护\"是指坚决维护习近平总书记党中央的核心、全党的核心地位，坚决维护党中央权威和集中统一领导。这是党的最高政治原则和根本政治规矩。";
        if (prompt.Contains("四个自信"))
            return "\"四个自信\"是指中国特色社会主义道路自信、理论自信、制度自信、文化自信。这是中国共产党和中国人民在长期奋斗中形成的科学信念。";
        if (prompt.Contains("不忘初心"))
            return "不忘初心，方得始终。中国共产党人的初心和使命，就是为中国人民谋幸福，为中华民族谋复兴。这个初心和使命是激励中国共产党人不断前进的根本动力。";
        if (prompt.Contains("党章"))
            return "《中国共产党章程》是党的总章程，是全党必须共同遵守的根本行为规范。现行党章由中国共产党第二十次全国代表大会部分修改，2022年10月22日通过。";
        if (prompt.Contains("五四运动"))
            return "五四运动是1919年5月4日发生在北京的爱国运动，是中国新民主主义革命的开端，促进了马克思主义在中国的传播，为中国共产党成立做了思想上干部上的准备。";
        if (prompt.Contains("二十大"))
            return "中国共产党第二十次全国代表大会于2022年10月16日至22日在北京举行。大会主题是：高举中国特色社会主义伟大旗帜，全面贯彻新时代中国特色社会主义思想，弘扬伟大建党精神，自信自强、守正创新，踔厉奋发、勇毅前行，为全面建设社会主义现代化国家、全面推进中华民族伟大复兴而团结奋斗。";

        // 通用回复
        return $"关于\"{prompt}\"的问题，我来为您解答：\n\n这是一个重要的党建理论问题。根据相关文献和政策文件，建议您查阅相关学习资料或参加支部组织的专题学习。如需更详细的解读，请配置真实AI模型API以获得更准确的回答。";
    }
}
