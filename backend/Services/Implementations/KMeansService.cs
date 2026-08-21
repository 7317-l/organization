using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>K-Means错题知识点聚类服务（占位实现）</summary>
public class KMeansService : IKMeansService
{
    private readonly AppDbContext _context;

    // 预设知识点标签池
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

        // 提取错题（占位：实际应从考试记录中分析）
        var errorCount = await _context.MemberTestRecords
            .Where(r => r.MemberId == request.PartyMemberId)
            .CountAsync();

        // 模拟聚类：随机选取知识点标签
        var random = new Random(request.PartyMemberId);
        var clusters = new List<KMeansClusterDto>();
        var allTags = new List<string>();

        for (int i = 0; i < request.ClusterCount; i++)
        {
            var tagCount = random.Next(2, 5);
            var tags = KnowledgeTagPool
                .OrderBy(_ => random.Next())
                .Take(tagCount)
                .ToList();
            allTags.AddRange(tags);

            clusters.Add(new KMeansClusterDto
            {
                ClusterName = $"薄弱板块{i + 1}",
                KnowledgeTags = tags,
                ErrorCount = random.Next(3, 15),
                Severity = Math.Round(random.NextDouble() * 0.5 + 0.5, 2)
            });
        }

        var topTags = allTags
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        return new KMeansClusteringResponse
        {
            PartyMemberId = request.PartyMemberId,
            MemberName = member.Name,
            Clusters = clusters.OrderByDescending(c => c.Severity).ToList(),
            TopWeaknessTags = topTags,
            Suggestion = $"基于K-Means聚类分析，您在「{string.Join("、", topTags.Take(3))}」等知识点上较为薄弱。" +
                         "建议针对性复习相关内容，并完成配套练习题巩固。"
        };
    }
}
