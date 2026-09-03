using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class PairHelpService : IPairHelpService
{
    private readonly AppDbContext _db;

    public PairHelpService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PairHelpRecommendResponse> RecommendAsync(int memberId, PairHelpRecommendRequest request)
    {
        var me = await _db.PartyMembers.FindAsync(memberId);
        if (me == null) throw new BusinessException("用户不存在");

        var myTags = request.MyWeaknessTags ?? new List<string>();
        if (myTags.Count == 0)
        {
            myTags = await GetMemberWeaknessTagsAsync(memberId);
        }

        var candidates = await _db.PartyMembers
            .Where(m => m.Id != memberId && m.OrganizationId == me.OrganizationId && m.IsEnabled)
            .Take(50)
            .ToListAsync();

        var recommendations = new List<PairHelpRecommendationDto>();
        foreach (var c in candidates)
        {
            var cTags = await GetMemberWeaknessTagsAsync(c.Id);
            var strongTags = cTags.Count > 0 ? new List<string> { "综合能力" } : new List<string>();
            var overlap = myTags.Intersect(cTags).Count();
            var score = 100 - overlap * 10 + (c.PointTotal / 10.0);
            if (score > 0)
            {
                recommendations.Add(new PairHelpRecommendationDto
                {
                    MemberId = c.Id,
                    MemberName = c.Name,
                    OrganizationName = (await _db.Organizations.FindAsync(c.OrganizationId))?.Name ?? "",
                    WeaknessTags = cTags,
                    Score = Math.Round(score, 1),
                    MatchReason = $"基于薄弱点互补分析，{c.Name}在相关领域表现较强，可提供帮扶"
                });
            }
        }

        return new PairHelpRecommendResponse
        {
            Recommendations = recommendations.OrderByDescending(r => r.Score).Take(request.Count).ToList()
        };
    }

    public async Task RequestPairAsync(int receiverId, PairHelpRequestDto request)
    {
        if (receiverId == request.HelperId)
            throw new BusinessException("不能与自己结对");

        var existing = await _db.PairHelpRequests
            .AnyAsync(r => r.HelperId == request.HelperId && r.HelpReceiverId == receiverId && r.Status == 0);
        if (existing)
            throw new BusinessException("已存在待处理的结对申请");

        _db.PairHelpRequests.Add(new PairHelpRequest
        {
            HelperId = request.HelperId,
            HelpReceiverId = receiverId,
            Status = 0,
            MatchReason = "用户发起结对申请",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();
    }

    public async Task AcceptRequestAsync(int requestId, int helperId)
    {
        var req = await _db.PairHelpRequests.FindAsync(requestId);
        if (req == null) throw new BusinessException("申请不存在");
        if (req.HelperId != helperId) throw new BusinessException("无权操作此申请");
        if (req.Status != 0) throw new BusinessException("申请状态不允许接受");

        req.Status = 1;
        req.UpdatedAt = DateTime.Now;

        _db.PairHelpRecords.Add(new PairHelpRecord
        {
            HelperId = req.HelperId,
            HelpReceiverId = req.HelpReceiverId,
            StartTime = DateTime.Now
        });
        await _db.SaveChangesAsync();
    }

    public async Task RejectRequestAsync(int requestId, int helperId)
    {
        var req = await _db.PairHelpRequests.FindAsync(requestId);
        if (req == null) throw new BusinessException("申请不存在");
        if (req.HelperId != helperId) throw new BusinessException("无权操作此申请");
        if (req.Status != 0) throw new BusinessException("申请状态不允许拒绝");

        req.Status = 2;
        req.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task<PairHelpMyResponse> GetMyPairsAsync(int memberId)
    {
        var activeRecords = await _db.PairHelpRecords
            .Where(r => (r.HelperId == memberId || r.HelpReceiverId == memberId) && r.EndTime == null)
            .ToListAsync();

        var historyRecords = await _db.PairHelpRecords
            .Where(r => (r.HelperId == memberId || r.HelpReceiverId == memberId) && r.EndTime != null)
            .ToListAsync();

        var active = await MapPairHelpDtosAsync(activeRecords, memberId);
        var history = await MapPairHelpDtosAsync(historyRecords, memberId);

        return new PairHelpMyResponse { Active = active, History = history };
    }

    public async Task CompletePairAsync(int recordId, int memberId, PairHelpCompleteRequest request)
    {
        var record = await _db.PairHelpRecords.FindAsync(recordId);
        if (record == null) throw new BusinessException("结对记录不存在");
        if (record.HelperId != memberId && record.HelpReceiverId != memberId)
            throw new BusinessException("无权操作此结对");
        if (record.EndTime != null) throw new BusinessException("结对已结束");

        record.EndTime = DateTime.Now;
        record.OutcomeSummary = request.OutcomeSummary;

        var req = await _db.PairHelpRequests
            .FirstOrDefaultAsync(r => r.HelperId == record.HelperId && r.HelpReceiverId == record.HelpReceiverId && r.Status == 1);
        if (req != null)
        {
            req.Status = 3;
            req.UpdatedAt = DateTime.Now;
        }

        await _db.SaveChangesAsync();
    }

    public async Task LogHelpAsync(int recordId, int memberId, PairHelpLogRequest request)
    {
        var record = await _db.PairHelpRecords.FindAsync(recordId);
        if (record == null) throw new BusinessException("结对记录不存在");
        if (record.HelperId != memberId && record.HelpReceiverId != memberId)
            throw new BusinessException("无权操作此结对");

        var logs = string.IsNullOrEmpty(record.HelpContentJson)
            ? new List<object>()
            : JsonSerializer.Deserialize<List<object>>(record.HelpContentJson) ?? new();
        logs.Add(new { time = DateTime.Now, content = request.Content, by = memberId });
        record.HelpContentJson = JsonSerializer.Serialize(logs);
        await _db.SaveChangesAsync();
    }

    private async Task<List<PairHelpMyDto>> MapPairHelpDtosAsync(List<PairHelpRecord> records, int memberId)
    {
        var result = new List<PairHelpMyDto>();
        foreach (var r in records)
        {
            var isHelper = r.HelperId == memberId;
            var partnerId = isHelper ? r.HelpReceiverId : r.HelperId;
            var partner = await _db.PartyMembers.FindAsync(partnerId);
            var org = partner != null ? await _db.Organizations.FindAsync(partner.OrganizationId) : null;
            result.Add(new PairHelpMyDto
            {
                RecordId = r.Id,
                PartnerId = partnerId,
                PartnerName = partner?.Name ?? "",
                PartnerOrgName = org?.Name ?? "",
                Role = isHelper ? "helper" : "receiver",
                StartTime = r.StartTime,
                WeaknessTags = await GetMemberWeaknessTagsAsync(partnerId)
            });
        }
        return result;
    }

    private async Task<List<string>> GetMemberWeaknessTagsAsync(int memberId)
    {
        var records = await _db.MemberTestRecords
            .Where(r => r.MemberId == memberId)
            .OrderByDescending(r => r.SubmittedAt)
            .Take(5)
            .ToListAsync();

        var tags = new HashSet<string>();
        foreach (var rec in records)
        {
            try
            {
                var answers = JsonSerializer.Deserialize<List<JsonElement>>(rec.Answers) ?? new();
                foreach (var a in answers)
                {
                    if (a.TryGetProperty("questionId", out var qidEl))
                    {
                        var qid = qidEl.GetInt32();
                        var q = await _db.Questions.FindAsync(qid);
                        if (q != null && q.CategoryId.HasValue)
                        {
                            var cat = await _db.QuestionCategories.FindAsync(q.CategoryId.Value);
                            if (cat != null) tags.Add(cat.Name);
                        }
                    }
                }
            }
            catch { }
        }
        return tags.Take(5).ToList();
    }
}
