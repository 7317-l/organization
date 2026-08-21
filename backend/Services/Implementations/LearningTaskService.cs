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

public class LearningTaskService : ILearningTaskService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public LearningTaskService(AppDbContext context, IMapper mapper, ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<PagedResponse> GetPagedAsync(TaskQueryParams query)
    {
        var queryable = _context.LearningTasks
            .Include(t => t.TargetOrg)
            .Include(t => t.TaskContents)
            .AsQueryable();

        // 数据权限
        if (_currentUser.Role == UserRole.BranchSecretary)
            queryable = queryable.Where(t => t.TargetOrgId == _currentUser.OrganizationId);

        if (!string.IsNullOrWhiteSpace(query.TaskName))
            queryable = queryable.Where(t => t.TaskName.Contains(query.TaskName));

        if (query.TargetOrgId.HasValue)
            queryable = queryable.Where(t => t.TargetOrgId == query.TargetOrgId.Value);

        var total = await queryable.LongCountAsync();
        var items = await queryable
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToListAsync();

        return PagedResponse.Ok(_mapper.Map<List<TaskListItemDto>>(items), query.Page, query.Size, total);
    }

    public async Task<TaskDetailDto> GetByIdAsync(int id)
    {
        var task = await _context.LearningTasks
            .Include(t => t.TargetOrg)
            .Include(t => t.TaskContents).ThenInclude(tc => tc.Content)
                .ThenInclude(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            throw new BusinessException("任务不存在", 404);

        var dto = _mapper.Map<TaskDetailDto>(task);
        dto.Contents = task.TaskContents.Select(tc => _mapper.Map<ContentListItemDto>(tc.Content)).ToList();
        return dto;
    }

    public async Task<TaskDetailDto> CreateAsync(CreateTaskRequest request)
    {
        var org = await _context.Organizations.FindAsync(request.TargetOrgId);
        if (org == null)
            throw new BusinessException("目标支部不存在", 404);

        var task = new LearningTask
        {
            TaskName = request.TaskName,
            TargetOrgId = request.TargetOrgId,
            Deadline = request.Deadline,
            CreatedAt = DateTime.Now
        };

        if (request.ContentIds != null && request.ContentIds.Any())
        {
            task.TaskContents = request.ContentIds.Select(cid => new TaskContent { ContentId = cid }).ToList();
        }

        _context.LearningTasks.Add(task);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(task.Id);
    }

    public async Task UpdateAsync(int id, UpdateTaskRequest request)
    {
        var task = await _context.LearningTasks
            .Include(t => t.TaskContents)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            throw new BusinessException("任务不存在", 404);

        task.TaskName = request.TaskName;
        task.TargetOrgId = request.TargetOrgId;
        task.Deadline = request.Deadline;

        task.TaskContents.Clear();
        if (request.ContentIds != null && request.ContentIds.Any())
        {
            foreach (var cid in request.ContentIds)
                task.TaskContents.Add(new TaskContent { ContentId = cid });
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var task = await _context.LearningTasks.FindAsync(id);
        if (task == null)
            throw new BusinessException("任务不存在", 404);

        _context.LearningTasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TaskCompletionDetailDto>> GetCompletionDetailsAsync(int taskId)
    {
        var task = await _context.LearningTasks
            .Include(t => t.TaskContents)
            .Include(t => t.TargetOrg)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new BusinessException("任务不存在", 404);

        var totalContents = task.TaskContents.Count;
        var members = await _context.PartyMembers
            .Where(m => m.OrganizationId == task.TargetOrgId && m.IsEnabled)
            .ToListAsync();

        var result = new List<TaskCompletionDetailDto>();

        foreach (var member in members)
        {
            var progresses = await _context.MemberLearningProgress
                .Where(p => p.MemberId == member.Id && p.TaskId == taskId && p.IsCompleted)
                .ToListAsync();

            var totalSeconds = await _context.MemberLearningProgress
                .Where(p => p.MemberId == member.Id && p.TaskId == taskId)
                .SumAsync(p => (int?)p.DurationSeconds) ?? 0;

            result.Add(new TaskCompletionDetailDto
            {
                MemberId = member.Id,
                MemberName = member.Name,
                TotalContents = totalContents,
                CompletedContents = progresses.Count,
                CompletionRate = totalContents > 0 ? Math.Round((double)progresses.Count / totalContents * 100, 2) : 0,
                TotalLearningSeconds = totalSeconds
            });
        }

        return result.OrderByDescending(r => r.CompletionRate).ToList();
    }
}
