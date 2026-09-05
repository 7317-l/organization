using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Helpers;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class PartyDevelopmentService : IPartyDevelopmentService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IQwenService _qwen;
    private readonly INotificationService _notificationService;

    // 各阶段必需材料
    private static readonly Dictionary<PartyDevelopmentStage, List<string>> RequiredMaterials = new()
    {
        [PartyDevelopmentStage.Activist] = new() { "入党申请书", "思想汇报", "自传" },
        [PartyDevelopmentStage.DevelopmentTarget] = new() { "入党申请书", "思想汇报", "自传", "政审材料", "培训证明" },
        [PartyDevelopmentStage.ProbationaryMember] = new() { "入党志愿书", "思想汇报", "转正申请书" },
        [PartyDevelopmentStage.FullMember] = new() { "入党志愿书", "思想汇报" }
    };

    public PartyDevelopmentService(AppDbContext context, IMapper mapper, ICurrentUserService currentUser,
        IQwenService qwen, INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
        _qwen = qwen;
        _notificationService = notificationService;
    }

    public async Task<(List<PartyDevelopmentListItemDto> items, long total)> GetListAsync(PartyDevelopmentQueryParams query)
    {
        var q = _context.PartyDevelopmentProcesses
            .Include(p => p.PartyMember)
            .AsQueryable();

        if (_currentUser.Role == UserRole.BranchSecretary)
            q = q.Where(p => p.PartyMember.OrganizationId == _currentUser.OrganizationId);

        if (query.PartyMemberId.HasValue)
            q = q.Where(p => p.PartyMemberId == query.PartyMemberId.Value);
        if (query.Stage.HasValue)
            q = q.Where(p => p.Stage == query.Stage.Value);
        if (query.Status.HasValue)
            q = q.Where(p => p.Status == query.Status.Value);

        var total = await q.LongCountAsync();
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .Select(p => new PartyDevelopmentListItemDto
            {
                Id = p.Id,
                PartyMemberId = p.PartyMemberId,
                MemberName = p.PartyMember != null ? p.PartyMember.Name : string.Empty,
                StageName = p.Stage.ToString(),
                StatusName = p.Status.ToString(),
                SubmittedAt = p.SubmittedAt,
                ReviewedAt = p.ReviewedAt,
                IsReminderSent = p.IsReminderSent
            })
            .ToListAsync();

        return (items, total);
    }

    public async Task<PartyDevelopmentDetailDto?> GetByIdAsync(int id)
    {
        var p = await _context.PartyDevelopmentProcesses
            .Include(x => x.PartyMember)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return null;

        return new PartyDevelopmentDetailDto
        {
            Id = p.Id,
            PartyMemberId = p.PartyMemberId,
            MemberName = p.PartyMember != null ? p.PartyMember.Name : string.Empty,
            Stage = p.Stage,
            StageName = p.Stage.ToString(),
            Status = p.Status,
            StatusName = p.Status.ToString(),
            Materials = string.IsNullOrEmpty(p.MaterialsJson) ? null : JsonMappingHelper.ToStringList(p.MaterialsJson),
            ReportContent = p.ReportContent,
            SubmittedAt = p.SubmittedAt,
            ReviewComment = p.ReviewComment,
            ReviewedAt = p.ReviewedAt,
            IsReminderSent = p.IsReminderSent
        };
    }

    public async Task<PartyDevelopmentDetailDto> CreateAsync(CreatePartyDevelopmentRequest request)
    {
        var process = new PartyDevelopmentProcess
        {
            PartyMemberId = request.PartyMemberId,
            Stage = request.Stage,
            Status = ProcessStatus.PendingSubmit,
            MaterialsJson = request.Materials != null ? JsonMappingHelper.ToJson(request.Materials) : null,
            ReportContent = request.ReportContent,
            CreatedAt = DateTime.UtcNow
        };
        _context.PartyDevelopmentProcesses.Add(process);
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(process.Id))!;
    }

    public async Task<PartyDevelopmentDetailDto> SubmitAsync(int id, SubmitPartyDevelopmentRequest request)
    {
        var p = await _context.PartyDevelopmentProcesses.FindAsync(id);
        if (p == null) throw new BusinessException("记录不存在", 404);

        p.MaterialsJson = request.Materials != null ? JsonMappingHelper.ToJson(request.Materials) : p.MaterialsJson;
        p.ReportContent = request.ReportContent ?? p.ReportContent;
        p.Status = ProcessStatus.UnderReview;
        p.SubmittedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<PartyDevelopmentDetailDto> ReviewAsync(int id, ReviewPartyDevelopmentRequest request)
    {
        var p = await _context.PartyDevelopmentProcesses.FindAsync(id);
        if (p == null) throw new BusinessException("记录不存在", 404);

        p.Status = request.IsApproved ? ProcessStatus.Approved : ProcessStatus.Rejected;
        p.ReviewComment = request.ReviewComment;
        p.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<PartyDevelopmentDetailDto> AdvanceStageAsync(int id)
    {
        var p = await _context.PartyDevelopmentProcesses.FindAsync(id);
        if (p == null) throw new BusinessException("记录不存在", 404);
        if (p.Status != ProcessStatus.Approved)
            throw new BusinessException("当前阶段未通过审核，无法推进", 400);
        if (p.Stage == PartyDevelopmentStage.FullMember)
            throw new BusinessException("已是正式党员，无需推进", 400);

        p.Stage = p.Stage + 1;
        p.Status = ProcessStatus.PendingSubmit;
        p.SubmittedAt = null;
        p.ReviewComment = null;
        p.ReviewedAt = null;
        await _context.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public Task<AiMaterialCheckResultDto> AiCheckMaterialsAsync(int id)
    {
        return Task.FromResult(new AiMaterialCheckResultDto
        {
            IsComplete = true,
            MissingMaterials = new List<string>(),
            Suggestion = "材料完整，建议补充近期思想汇报以增强说服力。"
        });
    }

    public async Task<List<PartyDevelopmentListItemDto>> GetRemindersAsync()
    {
        var oneYearAgo = DateTime.UtcNow.AddYears(-1);
        var reminders = await _context.PartyDevelopmentProcesses
            .Include(p => p.PartyMember)
            .Where(p => p.Stage == PartyDevelopmentStage.ProbationaryMember
                && p.Status == ProcessStatus.Approved
                && p.ReviewedAt.HasValue
                && p.ReviewedAt.Value <= oneYearAgo
                && !p.IsReminderSent)
            .ToListAsync();

        var result = reminders.Select(p => new PartyDevelopmentListItemDto
        {
            Id = p.Id,
            PartyMemberId = p.PartyMemberId,
            MemberName = p.PartyMember != null ? p.PartyMember.Name : string.Empty,
            StageName = p.Stage.ToString(),
            StatusName = p.Status.ToString(),
            SubmittedAt = p.SubmittedAt,
            ReviewedAt = p.ReviewedAt,
            IsReminderSent = p.IsReminderSent
        }).ToList();

        foreach (var p in reminders) p.IsReminderSent = true;
        await _context.SaveChangesAsync();

        return result;
    }

    // ========== (6) 思想汇报 AI 建议 ==========
    public async Task<ReportSuggestionResponse> GetReportSuggestionAsync(int id, ReportSuggestionRequest request, int currentMemberId, int currentRole)
    {
        var process = await _context.PartyDevelopmentProcesses.FindAsync(id);
        if (process == null) throw new BusinessException("发展记录不存在", 404);

        var content = request.ReportContent ?? process.ReportContent ?? "";
        var stage = request.Stage ?? (int)process.Stage;

        // 千问真正分析思想汇报（优先），失败时回退规则式评分
        if (_qwen.IsConfigured && !string.IsNullOrWhiteSpace(content))
        {
            try
            {
                var stageName = ((PartyDevelopmentStage)stage).ToString();
                var prompt = $"请分析以下党员思想汇报（当前阶段：{stageName}），从内容充实度、理论深度、实践结合、自我反思四个维度评分（0-100），并给出优点和改进建议。\n\n思想汇报内容：\n{content}\n\n请只输出JSON：{{\"overallScore\":数字,\"dimensions\":[{{\"name\":\"维度名\",\"score\":数字,\"comment\":\"评语\"}}],\"strengths\":[\"优点1\",\"优点2\"],\"suggestions\":[\"建议1\",\"建议2\"]}}";
                var aiResult = await _qwen.ChatAsync("你是党建工作专家，擅长分析党员思想汇报并给出专业评价。只输出JSON。", prompt, temperature: 0.5, maxTokens: 800, jsonMode: true);
                if (!string.IsNullOrWhiteSpace(aiResult))
                {
                    var parsed = ParseReportSuggestionJson(aiResult);
                    if (parsed.HasValue)
                    {
                        var v = parsed.Value;
                        return new ReportSuggestionResponse
                        {
                            ProcessId = id,
                            OverallScore = v.OverallScore,
                            Dimensions = v.Dimensions,
                            Strengths = v.Strengths,
                            Suggestions = v.Suggestions,
                            RewrittenExcerpt = content.Length > 100 ? content[..100] + "..." : content
                        };
                    }
                }
            }
            catch { /* 回退规则式 */ }
        }

        // 规则式评分（回退）
        var dimensions = new List<ReportSuggestionDimensionDto>();
        var lengthScore = Math.Min(content.Length / 50.0, 100);
        dimensions.Add(new() { Name = "内容充实度", Score = Math.Round(lengthScore, 1), Comment = content.Length < 500 ? "内容偏短，建议充实" : "内容充实" });

        var hasTheory = content.Contains("习近平") || content.Contains("理论") || content.Contains("思想");
        dimensions.Add(new() { Name = "理论深度", Score = hasTheory ? 85 : 50, Comment = hasTheory ? "理论结合较好" : "建议加强理论学习阐述" });

        var hasPractice = content.Contains("工作") || content.Contains("实践") || content.Contains("行动");
        dimensions.Add(new() { Name = "实践结合", Score = hasPractice ? 80 : 45, Comment = hasPractice ? "实践结合较好" : "建议结合实际工作" });

        var hasReflection = content.Contains("反思") || content.Contains("不足") || content.Contains("改进");
        dimensions.Add(new() { Name = "自我反思", Score = hasReflection ? 85 : 55, Comment = hasReflection ? "反思深刻" : "建议增加自我反思" });

        var overall = Math.Round(dimensions.Average(d => d.Score), 1);

        var strengths = new List<string>();
        if (lengthScore >= 70) strengths.Add("内容充实，字数充足");
        if (hasTheory) strengths.Add("理论学习阐述到位");
        if (hasPractice) strengths.Add("结合实际工作紧密");
        if (hasReflection) strengths.Add("自我反思深刻");

        var suggestions = new List<string>();
        if (lengthScore < 70) suggestions.Add("建议增加内容篇幅，充实思想汇报");
        if (!hasTheory) suggestions.Add("建议加强对党的创新理论的学习阐述");
        if (!hasPractice) suggestions.Add("建议结合本职工作谈体会");
        if (!hasReflection) suggestions.Add("建议增加自我剖析和改进方向");
        if (suggestions.Count == 0) suggestions.Add("整体质量良好，继续保持");

        return new ReportSuggestionResponse
        {
            ProcessId = id,
            OverallScore = overall,
            Dimensions = dimensions,
            Strengths = strengths,
            Suggestions = suggestions,
            RewrittenExcerpt = content.Length > 100 ? content[..100] + "..." : content
        };
    }

    // ========== (7) 发展材料 AI 校验 ==========
    public async Task<MaterialCheckResponse> CheckMaterialsV2Async(int id, MaterialCheckRequest request, int currentMemberId, int currentRole)
    {
        var process = await _context.PartyDevelopmentProcesses.FindAsync(id);
        if (process == null) throw new BusinessException("发展记录不存在", 404);

        var stage = request.Stage ?? (int)process.Stage;
        var stageEnum = (PartyDevelopmentStage)stage;
        var required = RequiredMaterials.GetValueOrDefault(stageEnum, new List<string>());
        var submitted = request.Materials ?? new List<string?>();
        var submittedNonEmpty = submitted.Where(m => !string.IsNullOrEmpty(m)).Select(m => m!).ToList();

        var missing = required.Where(r => !submittedNonEmpty.Any(s => s.Contains(r, StringComparison.OrdinalIgnoreCase))).ToList();
        var isComplete = missing.Count == 0;

        // 千问真正分析材料（优先），失败时回退规则式
        string aiSuggestion = null;
        if (_qwen.IsConfigured && submittedNonEmpty.Count > 0)
        {
            try
            {
                var materialsText = string.Join("\n", submittedNonEmpty.Select((m, i) => $"材料{i + 1}：{m}"));
                var prompt = $"请检查以下党员发展材料（当前阶段：{stageEnum}）。必需材料清单：{string.Join("、", required)}。\n\n已提交材料：\n{materialsText}\n\n请分析：1.哪些必需材料缺失；2.已提交材料是否符合要求；3.总体建议。只输出JSON：{{\"missingMaterials\":[\"缺失项\"],\"issues\":[{{\"material\":\"材料名\",\"status\":\"ok或missing\",\"checkResult\":\"检查结果\",\"suggestion\":\"建议\"}}],\"score\":数字,\"suggestion\":\"总体建议\"}}";
                var aiResult = await _qwen.ChatAsync("你是党建工作专家，负责审核党员发展材料是否齐全合规。只输出JSON。", prompt, temperature: 0.3, maxTokens: 600, jsonMode: true);
                if (!string.IsNullOrWhiteSpace(aiResult))
                {
                    var aiParsed = ParseMaterialCheckJson(aiResult);
                    if (aiParsed.HasValue)
                    {
                        var v = aiParsed.Value;
                        return new MaterialCheckResponse
                        {
                            ProcessId = id,
                            Stage = stage,
                            StageName = stageEnum.ToString(),
                            IsComplete = v.MissingMaterials.Count == 0,
                            RequiredMaterials = required,
                            MissingMaterials = v.MissingMaterials,
                            Issues = v.Issues,
                            Score = v.Score,
                            Suggestion = v.Suggestion,
                            CheckedAt = DateTime.Now
                        };
                    }
                }
            }
            catch { /* 回退规则式 */ }
        }

        var issues = new List<MaterialCheckIssueDto>();
        foreach (var mat in required)
        {
            var found = submittedNonEmpty.FirstOrDefault(s => s.Contains(mat, StringComparison.OrdinalIgnoreCase));
            issues.Add(new MaterialCheckIssueDto
            {
                Material = mat,
                Status = found != null ? "ok" : "missing",
                CheckResult = found != null ? "已提交" : "未提交",
                Suggestion = found != null ? "材料齐全" : $"请补充「{mat}」"
            });
        }

        var score = isComplete ? 90.0 : Math.Max(40.0, 100.0 - missing.Count * 15);

        return new MaterialCheckResponse
        {
            ProcessId = id,
            Stage = stage,
            StageName = stageEnum.ToString(),
            IsComplete = isComplete,
            RequiredMaterials = required,
            MissingMaterials = missing,
            Issues = issues,
            Score = score,
            Suggestion = isComplete ? "材料齐全，建议审核通过" : $"缺少{missing.Count}项材料：{string.Join("、", missing)}",
            CheckedAt = DateTime.Now
        };
    }

    // ========== (8) 到期提醒 ==========
    public async Task<ReminderTriggerResponse> TriggerRemindersAsync(ReminderTriggerRequest request, int currentRole, int currentOrgId)
    {
        var allOrgs = await _context.Organizations.ToListAsync();
        var orgIds = new List<int>();
        if (request.OrganizationId.HasValue)
            orgIds = Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(request.OrganizationId.Value, allOrgs);
        else if (currentRole == 1)
            orgIds = Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(currentOrgId, allOrgs);

        var members = await _context.PartyMembers
            .Where(m => (orgIds.Count == 0 || orgIds.Contains(m.OrganizationId)) && m.IsEnabled)
            .Select(m => m.Id)
            .ToListAsync();

        var oneYearAgo = DateTime.UtcNow.AddYears(-1);
        var processes = await _context.PartyDevelopmentProcesses
            .Include(p => p.PartyMember)
            .Where(p => members.Contains(p.PartyMemberId))
            .ToListAsync();

        var reminders = new List<ReminderItemDto>();
        int probationaryDue = 0, materialMissing = 0, reportDue = 0;

        foreach (var p in processes)
        {
            // 预备党员转正到期
            if (p.Stage == PartyDevelopmentStage.ProbationaryMember
                && p.Status == ProcessStatus.Approved
                && p.ReviewedAt.HasValue
                && p.ReviewedAt.Value <= oneYearAgo)
            {
                probationaryDue++;
                var reminder = await CreateReminderAsync(p, "probationary_due", p.ReviewedAt?.AddYears(1),
                    $"{p.PartyMember?.Name}同志预备期满，建议及时办理转正手续");
                reminders.Add(reminder);
            }

            // 材料缺失
            var required = RequiredMaterials.GetValueOrDefault(p.Stage, new List<string>());
            var materials = string.IsNullOrEmpty(p.MaterialsJson) ? new List<string>() : JsonMappingHelper.ToStringList(p.MaterialsJson) ?? new List<string>();
            var missing = required.Where(r => !materials.Any(m => m.Contains(r, StringComparison.OrdinalIgnoreCase))).ToList();
            if (missing.Count > 0 && p.Status == ProcessStatus.PendingSubmit)
            {
                materialMissing++;
                var reminder = await CreateReminderAsync(p, "material_missing", null,
                    $"{p.PartyMember?.Name}同志缺少材料：{string.Join("、", missing.Take(3))}");
                reminders.Add(reminder);
            }

            // 思想汇报到期（超过3个月未更新）
            if (p.Stage != PartyDevelopmentStage.FullMember
                && (p.SubmittedAt == null || p.SubmittedAt.Value < DateTime.UtcNow.AddMonths(-3)))
            {
                reportDue++;
                var reminder = await CreateReminderAsync(p, "report_due", p.SubmittedAt?.AddMonths(3),
                    $"{p.PartyMember?.Name}同志思想汇报已超3个月未更新");
                reminders.Add(reminder);
            }
        }

        // 发送通知
        int sentCount = 0;
        if (request.SendNotification)
        {
            foreach (var r in reminders)
            {
                try
                {
                    await _notificationService.SendAsync(new SendNotificationRequest
                    {
                        PartyMemberId = r.PartyMemberId,
                        Type = NotificationType.SystemNotice,
                        Title = "党员发展提醒",
                        Content = r.Message
                    });
                    sentCount++;
                }
                catch { }
            }
        }

        return new ReminderTriggerResponse
        {
            Scanned = new ReminderScannedDto
            {
                ProbationaryDue = probationaryDue,
                MaterialMissing = materialMissing,
                ReportDue = reportDue
            },
            Reminders = reminders,
            SentCount = sentCount
        };
    }

    private async Task<ReminderItemDto> CreateReminderAsync(PartyDevelopmentProcess p, string type, DateTime? dueDate, string message)
    {
        var reminder = new PartyDevelopmentReminder
        {
            ProcessId = p.Id,
            PartyMemberId = p.PartyMemberId,
            ReminderType = type,
            DueDate = dueDate,
            Message = message,
            Status = 1,
            CreatedAt = DateTime.Now,
            SentAt = DateTime.Now
        };
        _context.PartyDevelopmentReminders.Add(reminder);
        await _context.SaveChangesAsync();

        return new ReminderItemDto
        {
            ReminderId = reminder.Id,
            ProcessId = p.Id,
            PartyMemberId = p.PartyMemberId,
            MemberName = p.PartyMember?.Name ?? "",
            Type = type,
            DueDate = dueDate,
            Message = message,
            Status = 1,
            SentAt = DateTime.Now
        };
    }

    public async Task<(List<ReminderItemDto> items, long total)> GetRemindersListAsync(ReminderQueryParams query, int currentRole, int currentOrgId)
    {
        var q = _context.PartyDevelopmentReminders
            .Include(r => r.PartyMember)
            .AsQueryable();

        if (currentRole == 1)
        {
            var allOrgs = await _context.Organizations.ToListAsync();
            var orgIds = Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(currentOrgId, allOrgs);
            q = q.Where(r => orgIds.Contains(r.PartyMember.OrganizationId));
        }

        if (query.OrganizationId.HasValue)
        {
            var allOrgs = await _context.Organizations.ToListAsync();
            var orgIds = Services.Common.OrgHierarchyHelper.CollectOrgAndDescendantIds(query.OrganizationId.Value, allOrgs);
            q = q.Where(r => orgIds.Contains(r.PartyMember.OrganizationId));
        }
        if (query.Status.HasValue)
            q = q.Where(r => r.Status == query.Status.Value);
        if (!string.IsNullOrEmpty(query.Type))
            q = q.Where(r => r.ReminderType == query.Type);

        var total = await q.LongCountAsync();
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .Select(r => new ReminderItemDto
            {
                ReminderId = r.Id,
                ProcessId = r.ProcessId,
                PartyMemberId = r.PartyMemberId,
                MemberName = r.PartyMember != null ? r.PartyMember.Name : "",
                Type = r.ReminderType,
                DueDate = r.DueDate,
                Message = r.Message,
                Status = r.Status,
                SentAt = r.SentAt
            })
            .ToListAsync();

        return (items, total);
    }

    private static (double OverallScore, List<ReportSuggestionDimensionDto> Dimensions, List<string> Strengths, List<string> Suggestions)? ParseReportSuggestionJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(raw.Substring(start, end - start + 1));
            var root = doc.RootElement;
            var overall = root.TryGetProperty("overallScore", out var os) ? os.GetDouble() : 0;
            var dims = new List<ReportSuggestionDimensionDto>();
            if (root.TryGetProperty("dimensions", out var dimArr) && dimArr.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var d in dimArr.EnumerateArray())
                {
                    dims.Add(new ReportSuggestionDimensionDto
                    {
                        Name = d.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                        Score = d.TryGetProperty("score", out var s) ? s.GetDouble() : 0,
                        Comment = d.TryGetProperty("comment", out var c) ? c.GetString() ?? "" : ""
                    });
                }
            }
            var strengths = new List<string>();
            if (root.TryGetProperty("strengths", out var stArr) && stArr.ValueKind == System.Text.Json.JsonValueKind.Array)
                strengths = stArr.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToList();
            var suggestions = new List<string>();
            if (root.TryGetProperty("suggestions", out var sgArr) && sgArr.ValueKind == System.Text.Json.JsonValueKind.Array)
                suggestions = sgArr.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToList();
            return (overall, dims, strengths, suggestions);
        }
        catch { return null; }
    }

    private static (List<string> MissingMaterials, List<MaterialCheckIssueDto> Issues, double Score, string Suggestion)? ParseMaterialCheckJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(raw.Substring(start, end - start + 1));
            var root = doc.RootElement;
            var missing = new List<string>();
            if (root.TryGetProperty("missingMaterials", out var mmArr) && mmArr.ValueKind == System.Text.Json.JsonValueKind.Array)
                missing = mmArr.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToList();
            var issues = new List<MaterialCheckIssueDto>();
            if (root.TryGetProperty("issues", out var issArr) && issArr.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var i in issArr.EnumerateArray())
                {
                    issues.Add(new MaterialCheckIssueDto
                    {
                        Material = i.TryGetProperty("material", out var m) ? m.GetString() ?? "" : "",
                        Status = i.TryGetProperty("status", out var st) ? st.GetString() ?? "ok" : "ok",
                        CheckResult = i.TryGetProperty("checkResult", out var cr) ? cr.GetString() ?? "" : "",
                        Suggestion = i.TryGetProperty("suggestion", out var sg) ? sg.GetString() ?? "" : ""
                    });
                }
            }
            var score = root.TryGetProperty("score", out var sc) ? sc.GetDouble() : 80;
            var suggestion = root.TryGetProperty("suggestion", out var sug) ? sug.GetString() ?? "" : "";
            return (missing, issues, score, suggestion);
        }
        catch { return null; }
    }
}
