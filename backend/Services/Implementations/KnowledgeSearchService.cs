using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// 本地党建知识库检索服务实现（BM25算法版）。
/// 文档目录：项目根 knowledge/documents（.txt / .md）。
/// 检索算法：BM25（TF + IDF + 文档长度归一化），K1=1.5, B=0.75。
/// 中文采用 bigram 分词，英文按词分词。
/// </summary>
public class KnowledgeSearchService : IKnowledgeSearchService
{
    private const double K1 = 1.5;
    private const double B = 0.75;

    private readonly ILogger<KnowledgeSearchService> _logger;
    private readonly IReadOnlyList<KnowledgeDocument> _documents;
    private readonly Dictionary<string, List<string>> _docTokens;
    private readonly Dictionary<string, int> _docLengths;
    private readonly double _avgDocLength;

    public KnowledgeSearchService(ILogger<KnowledgeSearchService> logger)
    {
        _logger = logger;
        _documents = LoadDocuments();
        _docTokens = new Dictionary<string, List<string>>();
        _docLengths = new Dictionary<string, int>();

        foreach (var doc in _documents)
        {
            var tokens = Tokenize(doc.Content);
            _docTokens[doc.Id] = tokens;
            _docLengths[doc.Id] = tokens.Count;
        }

        _avgDocLength = _docLengths.Values.Count > 0
            ? _docLengths.Values.Average()
            : 1.0;
    }

    public int DocumentCount => _documents.Count;

    public IReadOnlyList<KnowledgeDocument> Search(string query, int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(query) || _documents.Count == 0)
            return Array.Empty<KnowledgeDocument>();

        var queryTerms = Tokenize(query).Distinct().ToList();
        if (queryTerms.Count == 0)
            return Array.Empty<KnowledgeDocument>();

        // 计算文档频率 DF
        var df = new Dictionary<string, int>();
        foreach (var term in queryTerms)
        {
            df[term] = _docTokens.Count(kv => kv.Value.Contains(term));
        }

        // 计算每个文档的 BM25 分数
        var results = new List<(KnowledgeDocument Doc, double Score)>();
        foreach (var doc in _documents)
        {
            var tokens = _docTokens[doc.Id];
            int dl = _docLengths[doc.Id];
            double score = 0;

            foreach (var term in queryTerms)
            {
                int tf = tokens.Count(t => t == term);
                if (tf == 0) continue;

                int n = df.ContainsKey(term) ? df[term] : 0;
                // IDF
                double idf = Math.Log((_documents.Count - n + 0.5) / (n + 0.5) + 1);
                // BM25 term score
                double termScore = idf * (tf * (K1 + 1)) / (tf + K1 * (1 - B + B * dl / _avgDocLength));
                score += termScore;
            }

            if (score > 0)
            {
                results.Add((doc, score));
            }
        }

        // 按 BM25 分数降序重排
        return results
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Select(x =>
            {
                x.Doc.Score = Math.Round(x.Score, 4);
                x.Doc.Snippet = ExtractSnippet(x.Doc.Content, queryTerms);
                return x.Doc;
            })
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

    /// <summary>
    /// 分词：中文按 bigram（相邻两字），英文按词。
    /// </summary>
    private static List<string> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text)) return new List<string>();
        var tokens = new List<string>();
        var lower = text.ToLowerInvariant();

        // 英文/数字按标点和空格切分
        var words = Regex.Split(lower, @"[\s，。！？、；：,.!?;:（）()【】\[\]《》""'']+")
            .Where(w => w.Length > 0);
        foreach (var word in words)
        {
            // 纯英文/数字词
            if (Regex.IsMatch(word, @"^[a-z0-9]+$"))
            {
                if (word.Length > 1) tokens.Add(word);
            }
            else
            {
                // 包含中文，添加 bigram
                for (int i = 0; i < word.Length - 1; i++)
                {
                    if (IsChinese(word[i]) && IsChinese(word[i + 1]))
                    {
                        tokens.Add(word.Substring(i, 2));
                    }
                    else if (char.IsLetterOrDigit(word[i]) && char.IsLetterOrDigit(word[i + 1]))
                    {
                        tokens.Add(word.Substring(i, 2).ToLowerInvariant());
                    }
                }
                // 单字也加入（长度为1的中文）
                if (word.Length == 1 && IsChinese(word[0]))
                {
                    tokens.Add(word);
                }
            }
        }

        return tokens;
    }

    private static bool IsChinese(char c)
    {
        return c >= '\u4e00' && c <= '\u9fff';
    }

    /// <summary>
    /// 提取匹配片段：找到第一个匹配词的位置，前后各取50字。
    /// </summary>
    private static string ExtractSnippet(string content, List<string> queryTerms, int length = 100)
    {
        if (string.IsNullOrEmpty(content)) return "";

        int bestPos = -1;
        foreach (var term in queryTerms)
        {
            int pos = content.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            if (pos >= 0 && (bestPos < 0 || pos < bestPos))
            {
                bestPos = pos;
            }
        }

        if (bestPos < 0)
        {
            return content.Length > length
                ? content.Substring(0, length) + "..."
                : content;
        }

        int start = Math.Max(0, bestPos - 30);
        int end = Math.Min(content.Length, start + length);
        var prefix = start > 0 ? "..." : "";
        var suffix = end < content.Length ? "..." : "";
        return prefix + content.Substring(start, end - start) + suffix;
    }

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
        var candidates = new List<string>
        {
            Path.Combine(Environment.CurrentDirectory, "knowledge", "documents"),
            Path.Combine(Environment.CurrentDirectory, "..", "knowledge", "documents"),
        };

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
}
