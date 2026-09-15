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

public class MeetingActivityService : IMeetingActivityService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public MeetingActivityService(AppDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<PagedResponse> GetPagedAsync(MeetingActivityQueryParams query)
    {
        var q = _context.MeetingActivities
            .Include(a => a.Organization)
            .Include(a => a.ActivityHearts)
            .AsQueryable();

        if (_currentUser.Role == UserRole.BranchSecretary)
            q = q.Where(a => a.OrganizationId == _currentUser.OrganizationId);

        if (query.OrganizationId.HasValue)
            q = q.Where(a => a.OrganizationId == query.OrganizationId.Value);
        if (query.Type.HasValue)
            q = q.Where(a => a.Type == query.Type.Value);
        if (query.StartDate.HasValue)
            q = q.Where(a => a.ActivityTime >= query.StartDate.Value);
        if (query.EndDate.HasValue)
            q = q.Where(a => a.ActivityTime <= query.EndDate.Value);

        var total = await q.LongCountAsync();
        var items = await q
            .OrderByDescending(a => a.ActivityTime)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        var dtos = items.Select(a => new MeetingActivityListItemDto
        {
            Id = a.Id,
            OrganizationId = a.OrganizationId,
            OrganizationName = a.Organization != null ? a.Organization.Name : string.Empty,
            Type = a.Type,
            TypeName = a.Type.ToString(),
            Title = a.Title,
            ActivityTime = a.ActivityTime,
            IsAiSummaryGenerated = a.IsAiSummaryGenerated,
            HeartCount = a.ActivityHearts.Count
        }).ToList();

        return PagedResponse.Ok(dtos, query.Page, query.Size, total);
    }

    public async Task<MeetingActivityDetailDto> GetByIdAsync(int id)
    {
        var a = await _context.MeetingActivities
            .Include(x => x.Organization)
            .Include(x => x.ActivityHearts).ThenInclude(h => h.PartyMember)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) throw new BusinessException("活动不存在", 404);

        return new MeetingActivityDetailDto
        {
            Id = a.Id,
            OrganizationId = a.OrganizationId,
            OrganizationName = a.Organization != null ? a.Organization.Name : string.Empty,
            Type = a.Type,
            TypeName = a.Type.ToString(),
            Title = a.Title,
            Description = a.Description,
            ActivityTime = a.ActivityTime,
            IsAiSummaryGenerated = a.IsAiSummaryGenerated,
            AiSummaryContent = a.AiSummaryContent,
            Hearts = a.ActivityHearts.Select(h => new ActivityHeartDto
            {
                Id = h.Id,
                MeetingActivityId = h.MeetingActivityId,
                PartyMemberId = h.PartyMemberId,
                MemberName = h.PartyMember != null ? h.PartyMember.Name : string.Empty,
                Content = h.Content,
                SubmittedAt = h.SubmittedAt,
                AiPolishSuggestion = h.AiPolishSuggestion
            }).ToList()
        };
    }

    public async Task<MeetingActivityDetailDto> CreateAsync(CreateMeetingActivityRequest request)
    {
        var activity = new MeetingActivity
        {
            OrganizationId = request.OrganizationId,
            Type = request.Type,
            Title = request.Title,
            Description = request.Description,
            ActivityTime = request.ActivityTime,
            CreatedAt = DateTime.UtcNow
        };
        _context.MeetingActivities.Add(activity);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(activity.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var a = await _context.MeetingActivities.FindAsync(id);
        if (a == null) throw new BusinessException("活动不存在", 404);
        _context.MeetingActivities.Remove(a);
        await _context.SaveChangesAsync();
    }

    public async Task<ActivityHeartDto> SubmitHeartAsync(int memberId, SubmitActivityHeartRequest request)
    {
        var activity = await _context.MeetingActivities.FindAsync(request.MeetingActivityId);
        if (activity == null) throw new BusinessException("活动不存在", 404);

        var heart = new ActivityHeart
        {
            MeetingActivityId = request.MeetingActivityId,
            PartyMemberId = memberId,
            Content = request.Content,
            SubmittedAt = DateTime.UtcNow,
            AiPolishSuggestion = "建议结合具体事例，增强心得的感染力和真实性。"
        };
        _context.ActivityHearts.Add(heart);
        await _context.SaveChangesAsync();

        var member = await _context.PartyMembers.FindAsync(memberId);
        return new ActivityHeartDto
        {
            Id = heart.Id,
            MeetingActivityId = heart.MeetingActivityId,
            PartyMemberId = heart.PartyMemberId,
            MemberName = member != null ? member.Name : string.Empty,
            Content = heart.Content,
            SubmittedAt = heart.SubmittedAt,
            AiPolishSuggestion = heart.AiPolishSuggestion
        };
    }

    public async Task<List<ActivityHeartDto>> GetHeartsByActivityAsync(int activityId)
    {
        var hearts = await _context.ActivityHearts
            .Include(h => h.PartyMember)
            .Where(h => h.MeetingActivityId == activityId)
            .OrderByDescending(h => h.SubmittedAt)
            .ToListAsync();

        return hearts.Select(h => new ActivityHeartDto
        {
            Id = h.Id,
            MeetingActivityId = h.MeetingActivityId,
            PartyMemberId = h.PartyMemberId,
            MemberName = h.PartyMember != null ? h.PartyMember.Name : string.Empty,
            Content = h.Content,
            SubmittedAt = h.SubmittedAt,
            AiPolishSuggestion = h.AiPolishSuggestion
        }).ToList();
    }

    public async Task<AiMeetingSummaryDto> GenerateAiSummaryAsync(int activityId)
    {
        var activity = await _context.MeetingActivities
            .Include(a => a.ActivityHearts)
            .FirstOrDefaultAsync(a => a.Id == activityId);
        if (activity == null) throw new BusinessException("活动不存在", 404);

        // 占位：AI总结
        var summary = $"本次「{activity.Title}」活动共有{activity.ActivityHearts.Count}名党员提交心得。" +
                      "大家围绕主题深入学习讨论，普遍表示收获颇丰。活动达到了预期效果。";

        activity.IsAiSummaryGenerated = true;
        activity.AiSummaryContent = summary;
        await _context.SaveChangesAsync();

        return new AiMeetingSummaryDto
        {
            ActivityId = activityId,
            Summary = summary,
            KeyPoints = new List<string> { "深入学习党的理论", "结合实际谈体会", "明确下一步努力方向" }
        };
    }
}
