using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>
/// K-Means错题知识点聚类服务（真实数据版）：
/// 读取该党员全部考试记录（member_test_records.answers），
/// 与题库正确答案逐题比对得到错题，按知识点（question category）聚类，
/// 统计每类错误次数与严重度，输出薄弱知识点排行。
/// </summary>
public class KMeansService : IKMeansService
{
    private readonly AppDbContext _context;

    // 知识点标签池（当题目无分类时按题干关键词归属）
    private static readonly List<string> KnowledgeTagPool = new()
    {
        "党史", "党章", "党规党纪", "党的宗旨", "党的性质",
        "四个意识", "四个自信", "两个维护", "不忘初心", "三严三实",
        "两学一做", "三会一课", "民主集中制", "党员义务", "党员权利",
        "党的组织制度", "党的纪律", "廉洁自律", "作风建设", "思想建设"
    };

    public KMeansService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<KMeansClusteringResponse> ClusterAsync(KMeansClusteringRequest request)
    {
        var member = await _context.PartyMembers.FindAsync(request.PartyMemberId);
        if (member == null)
            return new KMeansClusteringResponse { PartyMemberId = request.PartyMemberId, MemberName = "未知用户" };

        // 1. 读取该党员全部考试记录
        var records = await _context.MemberTestRecords
            .Where(r => r.MemberId == request.PartyMemberId)
            .Include(r => r.Test).ThenInclude(t => t.Paper)
            .ToListAsync();

        // 2. 汇总每题作答（多次考试取最新一次作答）
        var questionIdToAnswers = new Dictionary<int, List<(string Answer, DateTime Time)>>();
        var qidSet = new HashSet<int>();
        foreach (var rec in records)
        {
            var qids = ParseQuestionIds(rec.Test?.Paper?.QuestionIds);
            foreach (var qid in qids) qidSet.Add(qid);

            var answerDict = ParseAnswers(rec.Answers);
            foreach (var kv in answerDict)
            {
                qidSet.Add(kv.Key);
                if (!questionIdToAnswers.ContainsKey(kv.Key))
                    questionIdToAnswers[kv.Key] = new List<(string, DateTime)>();
                questionIdToAnswers[kv.Key].Add((kv.Value, rec.SubmittedAt));
            }
        }

        // 3. 加载题目与知识点
        var questions = await _context.Questions
            .Where(q => qidSet.Contains(q.Id))
            .Include(q => q.Category)
            .ToListAsync();

        var categories = await _context.QuestionCategories.ToListAsync();

        // 4. 逐题判题，收集错题（按知识点聚类）
        var errorByTag = new Dictionary<string, int>();      // 知识点 → 错误次数
        var totalByTag = new Dictionary<string, int>();      // 知识点 → 答题次数
        int totalAnswered = 0;
        int totalWrong = 0;

        foreach (var q in questions)
        {
            if (!questionIdToAnswers.TryGetValue(q.Id, out var attempts) || attempts.Count == 0) continue;

            // 取最新一次作答
            var latest = attempts.OrderByDescending(a => a.Time).First().Answer;
            totalAnswered++;
            var isCorrect = CheckAnswer(q, latest);
            if (!isCorrect) totalWrong++;

            var tag = ResolveKnowledgeTag(q, categories);
            if (!totalByTag.ContainsKey(tag)) totalByTag[tag] = 0;
            totalByTag[tag]++;
            if (!isCorrect)
            {
                if (!errorByTag.ContainsKey(tag)) errorByTag[tag] = 0;
                errorByTag[tag]++;
            }
        }

        // 5. 构造特征向量：每个知识点 = [错误率, 错误次数, 答题次数]
        var featureVectors = errorByTag.Select(kv =>
        {
            var total = totalByTag.TryGetValue(kv.Key, out var t) ? t : 1;
            return new
            {
                Tag = kv.Key,
                Features = new double[] { (double)kv.Value / total, kv.Value, total },
                ErrorCount = kv.Value,
                Total = total
            };
        }).ToList();

        var clusterCount = Math.Min(request.ClusterCount > 0 ? request.ClusterCount : 3, featureVectors.Count);
        if (clusterCount < 1) clusterCount = 1;

        // 真正的KMeans聚类
        var (labels, centroids, iterations) = RunKMeans(featureVectors.Select(f => f.Features).ToArray(), clusterCount, maxIterations: 50);

        // 按簇聚合，每个簇取平均错误率作为严重度
        var clusters = new List<KMeansClusterDto>();
        for (int c = 0; c < clusterCount; c++)
        {
            var clusterItems = featureVectors.Where((_, i) => labels[i] == c).ToList();
            if (clusterItems.Count == 0) continue;
            var avgErrorRate = clusterItems.Average(x => x.Features[0]);
            var totalErrors = clusterItems.Sum(x => x.ErrorCount);
            clusters.Add(new KMeansClusterDto
            {
                ClusterName = $"薄弱簇{c + 1}（{string.Join("、", clusterItems.Select(x => x.Tag).Take(3))}）",
                KnowledgeTags = clusterItems.Select(x => x.Tag).ToList(),
                ErrorCount = totalErrors,
                Severity = Math.Round(avgErrorRate, 2)
            });
        }
        clusters = clusters.OrderByDescending(c => c.Severity).ToList();

        // 若没有任何错题，兜底按答题情况给出无薄弱结论
        if (clusters.Count == 0)
        {
            return new KMeansClusteringResponse
            {
                PartyMemberId = request.PartyMemberId,
                MemberName = member.Name,
                Clusters = new List<KMeansClusterDto>(),
                TopWeaknessTags = new List<string>(),
                Suggestion = totalAnswered == 0
                    ? "您还没有考试记录，完成测验后可获得AI薄弱知识点分析。"
                    : "您最近测验作答全部正确，暂无明显薄弱知识点，继续保持！"
            };
        }

        var topTags = clusters.Select(c => c.ClusterName).ToList();

        return new KMeansClusteringResponse
        {
            PartyMemberId = request.PartyMemberId,
            MemberName = member.Name,
            Clusters = clusters,
            TopWeaknessTags = topTags,
            Suggestion = $"基于AI错题聚类分析，您在「{string.Join("、", topTags.Take(3))}」等知识点上共做错{totalWrong}题（共答{totalAnswered}题）。" +
                         "建议针对性复习相关内容，并完成配套练习题巩固。"
        };
    }

    // ============ 数据解析 ============

    private static List<int> ParseQuestionIds(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new List<int>();
        try { return JsonSerializer.Deserialize<List<int>>(raw) ?? new List<int>(); }
        catch { return new List<int>(); }
    }

    /// <summary>兼容两种答案存储格式：新格式 List{SubmitAnswerItem} 和旧格式 Dictionary{string,string}</summary>
    private static Dictionary<int, string> ParseAnswers(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new Dictionary<int, string>();
        try
        {
            var list = JsonSerializer.Deserialize<List<SubmitAnswerItem>>(raw);
            if (list != null) return list.ToDictionary(a => a.QuestionId, a => a.Answer);
        }
        catch { /* 尝试旧格式 */ }
        try
        {
            var old = JsonSerializer.Deserialize<Dictionary<string, string>>(raw);
            if (old != null) return old.ToDictionary(kv => int.Parse(kv.Key), kv => kv.Value);
        }
        catch { /* 忽略 */ }
        return new Dictionary<int, string>();
    }

    /// <summary>解析题目知识点：优先用分类名，无分类时按题干关键词匹配标签池</summary>
    private static string ResolveKnowledgeTag(Question q, List<QuestionCategory> categories)
    {
        if (q.Category != null && !string.IsNullOrWhiteSpace(q.Category.Name)) return q.Category.Name;
        if (q.CategoryId.HasValue)
        {
            var cat = categories.FirstOrDefault(c => c.Id == q.CategoryId.Value);
            if (cat != null && !string.IsNullOrWhiteSpace(cat.Name)) return cat.Name;
        }
        var hit = KnowledgeTagPool.FirstOrDefault(t => q.Stem.Contains(t));
        return hit ?? "综合知识";
    }

    // ============ 判题逻辑（与 MobileService.CheckAnswer 保持一致） ============

    private static bool CheckAnswer(Question question, string userAnswer)
    {
        if (string.IsNullOrWhiteSpace(userAnswer)) return false;
        switch (question.QuestionType)
        {
            case PartySchoolApi.Models.Common.QuestionType.SingleChoice:
            case PartySchoolApi.Models.Common.QuestionType.TrueFalse:
                var u = NormalizeSingleAnswer(question, userAnswer);
                var c = NormalizeSingleAnswer(question, question.CorrectAnswer);
                return string.Equals(u, c, StringComparison.OrdinalIgnoreCase);

            case PartySchoolApi.Models.Common.QuestionType.MultiChoice:
                var userSet = ParseMultiAnswer(question, userAnswer);
                var correctSet = ParseMultiAnswer(question, question.CorrectAnswer);
                if (userSet == null || correctSet == null || userSet.Count == 0) return false;
                return userSet.SetEquals(correctSet);

            default:
                return false;
        }
    }

    private static string NormalizeSingleAnswer(Question question, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var text = raw.Trim();
        var options = GetQuestionOptions(question);

        if (options.Contains(text, StringComparer.OrdinalIgnoreCase)) return text;

        if (text.Length == 1 && char.IsLetter(text[0]))
        {
            var idx = char.ToUpper(text[0]) - 'A';
            if (idx >= 0 && idx < options.Count) return options[idx];
        }

        if (int.TryParse(text, out var num) && num >= 0 && num < options.Count) return options[num];

        if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase) || text == "对") return "正确";
        if (string.Equals(text, "false", StringComparison.OrdinalIgnoreCase) || text == "错") return "错误";

        return text;
    }

    private static HashSet<string>? ParseMultiAnswer(Question question, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var options = GetQuestionOptions(question);
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tokens = new List<string>();
        try
        {
            var strArr = JsonSerializer.Deserialize<List<string>>(raw);
            if (strArr != null) tokens.AddRange(strArr);
        }
        catch
        {
            try
            {
                var intArr = JsonSerializer.Deserialize<List<int>>(raw);
                if (intArr != null) tokens.AddRange(intArr.Select(i => i.ToString()));
            }
            catch
            {
                tokens.AddRange(raw.Split(new[] { ',', '，', ';', '；', ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }
        foreach (var t in tokens)
        {
            var norm = NormalizeSingleAnswer(question, t);
            if (!string.IsNullOrEmpty(norm)) set.Add(norm);
        }
        return set;
    }

    private static List<string> GetQuestionOptions(Question question)
    {
        try { return JsonSerializer.Deserialize<List<string>>(question.Options) ?? new List<string>(); }
        catch { return new List<string>(); }
    }

    // ============ 真正的KMeans聚类算法 ============

    /// <summary>
    /// 真正的KMeans聚类：K-means++初始化 + 欧氏距离 + 迭代更新质心 + 收敛判断
    /// </summary>
    private static (int[] Labels, double[][] Centroids, int Iterations) RunKMeans(double[][] data, int k, int maxIterations = 50)
    {
        int n = data.Length;
        int dim = data[0].Length;
        var rng = new Random(42);

        // K-means++ 初始化质心
        var centroids = new double[k][];
        centroids[0] = (double[])data[rng.Next(n)].Clone();
        for (int c = 1; c < k; c++)
        {
            var distances = new double[n];
            for (int i = 0; i < n; i++)
            {
                double minDist = double.MaxValue;
                for (int j = 0; j < c; j++)
                {
                    double d = EuclideanDistance(data[i], centroids[j]);
                    if (d < minDist) minDist = d;
                }
                distances[i] = minDist * minDist;
            }
            double sum = distances.Sum();
            double r = rng.NextDouble() * sum;
            int selected = 0;
            for (int i = 0; i < n; i++)
            {
                r -= distances[i];
                if (r <= 0) { selected = i; break; }
            }
            centroids[c] = (double[])data[selected].Clone();
        }

        var labels = new int[n];
        int iterations = 0;
        bool converged = false;

        while (!converged && iterations < maxIterations)
        {
            iterations++;
            converged = true;

            // 分配样本到最近质心
            for (int i = 0; i < n; i++)
            {
                int nearest = 0;
                double minDist = double.MaxValue;
                for (int c = 0; c < k; c++)
                {
                    double d = EuclideanDistance(data[i], centroids[c]);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = c;
                    }
                }
                if (labels[i] != nearest)
                {
                    labels[i] = nearest;
                    converged = false;
                }
            }

            // 更新质心为簇均值
            for (int c = 0; c < k; c++)
            {
                var clusterPoints = data.Where((_, i) => labels[i] == c).ToList();
                if (clusterPoints.Count > 0)
                {
                    var newCentroid = new double[dim];
                    for (int d = 0; d < dim; d++)
                        newCentroid[d] = clusterPoints.Average(p => p[d]);
                    centroids[c] = newCentroid;
                }
            }
        }

        return (labels, centroids, iterations);
    }

    private static double EuclideanDistance(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += (a[i] - b[i]) * (a[i] - b[i]);
        return Math.Sqrt(sum);
    }
}
