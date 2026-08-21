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

    public PartyDevelopmentService(AppDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<PagedResponse> GetPagedAsync(PartyDevelopmentQueryParams query)
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
            .ToListAsync();

        var dtos = items.Select(p => new PartyDevelopmentListItemDto
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

        return PagedResponse.Ok(dtos, query.Page, query.Size, total);
    }

    public async Task<PartyDevelopmentDetailDto> GetByIdAsync(int id)
    {
        var p = await _context.PartyDevelopmentProcesses
            .Include(x => x.PartyMember)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) throw new BusinessException("记录不存在", 404);

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
        return await GetByIdAsync(process.Id);
    }

    public async Task SubmitAsync(int id, SubmitPartyDevelopmentRequest request)
    {
        var p = await _context.PartyDevelopmentProcesses.FindAsync(id);
        if (p == null) throw new BusinessException("记录不存在", 404);

        p.MaterialsJson = request.Materials != null ? JsonMappingHelper.ToJson(request.Materials) : p.MaterialsJson;
        p.ReportContent = request.ReportContent ?? p.ReportContent;
        p.Status = ProcessStatus.UnderReview;
        p.SubmittedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ReviewAsync(int id, ReviewPartyDevelopmentRequest request)
    {
        var p = await _context.PartyDevelopmentProcesses.FindAsync(id);
        if (p == null) throw new BusinessException("记录不存在", 404);

        p.Status = request.IsApproved ? ProcessStatus.Approved : ProcessStatus.Rejected;
        p.ReviewComment = request.ReviewComment;
        p.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task AdvanceStageAsync(int id)
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
    }

    public Task<AiMaterialCheckResultDto> AiCheckMaterialsAsync(int id)
    {
        // 占位：AI材料完整性校验
        return Task.FromResult(new AiMaterialCheckResultDto
        {
            IsComplete = true,
            MissingMaterials = new List<string>(),
            Suggestion = "材料完整，建议补充近期思想汇报以增强说服力。"
        });
    }

    public async Task<List<PartyDevelopmentListItemDto>> GetRemindersAsync()
    {
        // 预备党员满一年提醒转正
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

        // 标记已发送
        foreach (var p in reminders) p.IsReminderSent = true;
        await _context.SaveChangesAsync();

        return result;
    }

    public async Task DeleteAsync(int id)
    {
        var p = await _context.PartyDevelopmentProcesses.FindAsync(id);
        if (p == null) throw new BusinessException("记录不存在", 404);
        _context.PartyDevelopmentProcesses.Remove(p);
        await _context.SaveChangesAsync();
    }
}
