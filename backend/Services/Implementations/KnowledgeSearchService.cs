using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// 本地党建知识库检索服务实现。
/// 文档目录：项目根 knowledge/documents（.txt / .md），与千问 AI 模块共用同一份知识库。
/// 检索算法与 Express 版 knowledge.js 保持一致：整词匹配×2 + 连续4字子串匹配。
/// </summary>
public class KnowledgeSearchService : IKnowledgeSearchService
{
    private readonly ILogger<KnowledgeSearchService> _logger;
    private readonly IReadOnlyList<KnowledgeDocument> _documents;

    public KnowledgeSearchService(ILogger<KnowledgeSearchService> logger)
    {
        _logger = logger;
        _documents = LoadDocuments();
    }

    public int DocumentCount => _documents.Count;

    public IReadOnlyList<KnowledgeDocument> Search(string query, int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(query) || _documents.Count == 0)
            return Array.Empty<KnowledgeDocument>();

        return _documents
            .Select(d => new { Doc = d, Score = CalculateScore(query, d.Content) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Select(x => x.Doc)
            .ToList();
    }

    public string BuildContext(IReadOnlyList<KnowledgeDocument> results)
    {
        if (results == null || results.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        for (var i = 0; i < results.Count; i++)
        {
            var item = results[i];
            if (i > 0) sb.AppendLine().AppendLine();
            sb.AppendLine($"资料{i + 1}");
            sb.AppendLine($"来源：{item.File}");
            sb.AppendLine("内容：");
            sb.AppendLine(item.Content);
        }
        return sb.ToString();
    }

    // ---------- 私有 ----------

    private IReadOnlyList<KnowledgeDocument> LoadDocuments()
    {
        var dir = ResolveKnowledgeDirectory();
        if (dir == null)
        {
            _logger.LogWarning("未找到知识库目录 knowledge/documents，AI 知识问答将退回仅依赖大模型。");
            return Array.Empty<KnowledgeDocument>();
        }

        var docs = new List<KnowledgeDocument>();
        try
        {
            foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext != ".txt" && ext != ".md") continue;

                var content = File.ReadAllText(file, Encoding.UTF8);
                var chunks = SplitText(content);
                for (var i = 0; i < chunks.Count; i++)
                {
                    docs.Add(new KnowledgeDocument
                    {
                        Id = $"{Path.GetFileName(file)}-{i}",
                        File = Path.GetFileName(file),
                        Content = chunks[i]
                    });
                }
            }
            _logger.LogInformation("知识库加载完成：目录 {Dir}，共 {Count} 个片段。", dir, docs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载知识库失败：{Dir}", dir);
        }

        return docs;
    }

    private static string? ResolveKnowledgeDirectory()
    {
        // 候选路径（自上而下优先）：
        // 1) 当前工作目录下 knowledge/documents（后端在项目根运行时）
        // 2) 后端目录上一级 knowledge/documents（dotnet run 从 backend/ 启动）
        // 3) 程序基目录逐级向上找
        var candidates = new List<string>
        {
            Path.Combine(Environment.CurrentDirectory, "knowledge", "documents"),
            Path.Combine(Environment.CurrentDirectory, "..", "knowledge", "documents"),
        };

        // 从程序基目录向上查找（最多上溯 5 层）
        var baseDir = new DirectoryInfo(AppContext.BaseDirectory);
        var dir = baseDir;
        for (var i = 0; i < 6 && dir != null; i++)
        {
            candidates.Add(Path.Combine(dir.FullName, "knowledge", "documents"));
            dir = dir.Parent;
        }

        foreach (var c in candidates)
        {
            try
            {
                var full = Path.GetFullPath(c);
                if (Directory.Exists(full)) return full;
            }
            catch { /* 忽略非法路径 */ }
        }

        return null;
    }

    private static string NormalizeText(string text)
        => Regex.Replace(Regex.Replace(text.Replace("\r\n", "\n").Replace("\r", "\n"), @"[ \t]+", " "), @"^\s+|\s+$", "");

    private static List<string> SplitText(string text)
    {
        var normalized = NormalizeText(text);
        if (string.IsNullOrEmpty(normalized)) return new List<string>();

        var chunks = new List<string>();
        var paragraphs = normalized
            .Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0);

        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length <= 1000)
            {
                chunks.Add(paragraph);
                continue;
            }

            for (var start = 0; start < paragraph.Length; start += 800)
            {
                var len = Math.Min(1000, paragraph.Length - start);
                var chunk = paragraph.Substring(start, len).Trim();
                if (chunk.Length > 0) chunks.Add(chunk);
            }
        }

        return chunks;
    }

    private static int CalculateScore(string query, string content)
    {
        var q = query.ToLowerInvariant();
        var target = content.ToLowerInvariant();

        // 1. 整词匹配（按标点切分后的词，长度>1）
        var words = Regex.Split(q, @"[\s，。！？、；：,.!?;:]+")
            .Where(w => w.Length > 1);
        var score = words.Count(w => target.Contains(w)) * 2;

        // 2. 连续4字子串匹配
        const int n = 4;
        for (var i = 0; i <= q.Length - n; i++)
        {
            var sub = q.Substring(i, n);
            if (target.Contains(sub)) score += 1;
        }

        return score;
    }
}
