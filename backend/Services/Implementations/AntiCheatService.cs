using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>防挂机验证服务</summary>
public class AntiCheatService : IAntiCheatService
{
    private readonly AppDbContext _context;

    // 内存缓存挑战题（生产环境建议用Redis）
    private static readonly ConcurrentDictionary<string, AntiCheatChallenge> _challenges = new();

    public AntiCheatService(AppDbContext context)
    {
        _context = context;
    }

    public AntiCheatChallengeDto GenerateChallenge()
    {
        var challengeId = Guid.NewGuid().ToString("N");
        var questions = new List<(string Q, List<string> Ops, string Ans)>
        {
            ("请确认您正在学习", new List<string> { "我在学习", "我在挂机", "不确定" }, "我在学习"),
            ("当前学习状态如何？", new List<string> { "专注学习", "暂停休息", "已离开" }, "专注学习"),
            ("请点击正确按钮继续学习", new List<string> { "继续学习", "退出学习" }, "继续学习")
        };
        var picked = questions[new Random().Next(questions.Count)];

        var challenge = new AntiCheatChallenge
        {
            ChallengeId = challengeId,
            Answer = picked.Ans,
            ExpireAt = DateTime.UtcNow.AddMinutes(2)
        };
        _challenges[challengeId] = challenge;

        return new AntiCheatChallengeDto
        {
            ChallengeId = challengeId,
            Question = picked.Q,
            Options = picked.Ops,
            ExpireAt = challenge.ExpireAt
        };
    }

    public AntiCheatVerifyResponse Verify(AntiCheatVerifyRequest request)
    {
        if (!_challenges.TryRemove(request.ChallengeId, out var challenge))
            return new AntiCheatVerifyResponse { IsValid = false, Message = "验证已过期，请重新获取" };

        if (DateTime.UtcNow > challenge.ExpireAt)
            return new AntiCheatVerifyResponse { IsValid = false, Message = "验证超时，请重新获取" };

        if (request.Answer != challenge.Answer)
            return new AntiCheatVerifyResponse { IsValid = false, Message = "验证失败，请确认您在学习" };

        return new AntiCheatVerifyResponse { IsValid = true, Message = "验证通过，继续学习吧" };
    }

    public async Task<List<AntiCheatStatsDto>> GetStatsAsync(int? orgId)
    {
        var q = _context.PartyMembers
            .Include(m => m.Organization)
            .Where(m => m.IsEnabled)
            .AsQueryable();

        if (orgId.HasValue)
            q = q.Where(m => m.OrganizationId == orgId.Value);

        var members = await q.ToListAsync();
        var result = new List<AntiCheatStatsDto>();

        foreach (var member in members)
        {
            var totalSeconds = await _context.MemberLearningProgress
                .Where(p => p.MemberId == member.Id)
                .SumAsync(p => (int?)p.DurationSeconds) ?? 0;

            var totalMinutes = totalSeconds / 60.0;
            // 模拟：假设挂机时长占比随机（实际应基于验证失败记录计算）
            var idleRate = new Random(member.Id).NextDouble() * 0.3;
            var idleMinutes = Math.Round(totalMinutes * idleRate, 2);
            var validMinutes = Math.Round(totalMinutes - idleMinutes, 2);

            result.Add(new AntiCheatStatsDto
            {
                MemberId = member.Id,
                MemberName = member.Name,
                OrganizationId = member.OrganizationId,
                OrganizationName = member.Organization != null ? member.Organization.Name : string.Empty,
                ValidLearningMinutes = validMinutes,
                IdleMinutes = idleMinutes,
                IdleRate = Math.Round(idleRate * 100, 2),
                PassCount = new Random(member.Id).Next(5, 20),
                FailCount = new Random(member.Id).Next(0, 3)
            });
        }

        return result.OrderByDescending(s => s.IdleRate).ToList();
    }

    private class AntiCheatChallenge
    {
        public string ChallengeId { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
    }
}
