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
    private readonly INotificationService _notification;

    public LearningTaskService(AppDbContext context, IMapper mapper, ICurrentUserService currentUser, INotificationService notification)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
        _notification = notification;
    }

    public async Task<PagedResponse> GetPagedAsync(TaskQueryParams query)
    {
        var queryable = _context.LearningTasks
            .Include(t => t.TargetOrg)
            .Include(t => t.TaskContents)
            .Include(t => t.ExamPaper)
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

        var dtos = _mapper.Map<List<TaskListItemDto>>(items);
        foreach (var dto in dtos)
        {
            var task = items.FirstOrDefault(t => t.Id == dto.Id);
            dto.ExamPaperId = task?.ExamPaperId;
            dto.ExamPaperName = task?.ExamPaper?.Name;
        }
        return PagedResponse.Ok(dtos, query.Page, query.Size, total);
    }

    public async Task<TaskDetailDto> GetByIdAsync(int id)
    {
        var task = await _context.LearningTasks
            .Include(t => t.TargetOrg)
            .Include(t => t.ExamPaper)
            .Include(t => t.TaskContents).ThenInclude(tc => tc.Content)
                .ThenInclude(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            throw new BusinessException("任务不存在", 404);

        var dto = _mapper.Map<TaskDetailDto>(task);
        dto.Contents = task.TaskContents.Select(tc => _mapper.Map<ContentListItemDto>(tc.Content)).ToList();
        dto.ExamPaperId = task.ExamPaperId;
        dto.ExamPaperName = task.ExamPaper?.Name;
        return dto;
    }

    public async Task<TaskDetailDto> CreateAsync(CreateTaskRequest request)
    {
        var org = await _context.Organizations.FindAsync(request.TargetOrgId);
        if (org == null)
            throw new BusinessException("目标支部不存在", 404);

        if (request.ExamPaperId.HasValue)
        {
            var paper = await _context.ExamPapers.FindAsync(request.ExamPaperId.Value);
            if (paper == null)
                throw new BusinessException("关联试卷不存在", 404);
        }

        var task = new LearningTask
        {
            TaskName = request.TaskName,
            TargetOrgId = request.TargetOrgId,
            Deadline = request.Deadline,
            ExamPaperId = request.ExamPaperId,
            CreatedAt = DateTime.Now
        };

        if (request.ContentIds != null && request.ContentIds.Any())
        {
            task.TaskContents = request.ContentIds.Select(cid => new TaskContent { ContentId = cid }).ToList();
        }

        _context.LearningTasks.Add(task);
        await _context.SaveChangesAsync();

        // 创建任务时自动给目标支部党员发通知
        var members = await _context.PartyMembers
            .Where(m => m.OrganizationId == request.TargetOrgId && m.IsEnabled)
            .ToListAsync();
        foreach (var member in members)
        {
            try
            {
                await _notification.SendAsync(new SendNotificationRequest
                {
                    PartyMemberId = member.Id,
                    Type = NotificationType.TaskReminder,
                    Title = "新学习任务",
                    Content = $"您有新的学习任务「{request.TaskName}」，截止时间 {request.Deadline:yyyy-MM-dd HH:mm}，请及时完成。"
                });
            }
            catch { }
        }

        return await GetByIdAsync(task.Id);
    }

    public async Task UpdateAsync(int id, UpdateTaskRequest request)
    {
        var task = await _context.LearningTasks
            .Include(t => t.TaskContents)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            throw new BusinessException("任务不存在", 404);

        if (request.ExamPaperId.HasValue)
        {
            var paper = await _context.ExamPapers.FindAsync(request.ExamPaperId.Value);
            if (paper == null)
                throw new BusinessException("关联试卷不存在", 404);
        }

        task.TaskName = request.TaskName;
        task.TargetOrgId = request.TargetOrgId;
        task.Deadline = request.Deadline;
        task.ExamPaperId = request.ExamPaperId;

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

    /// <summary>催办：给未完成任务的党员发送通知</summary>
    public async Task<TaskUrgeResultDto> UrgeAsync(int taskId)
    {
        var task = await _context.LearningTasks
            .Include(t => t.TaskContents)
            .FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null)
            throw new BusinessException("任务不存在", 404);

        var totalContents = task.TaskContents.Count;
        var members = await _context.PartyMembers
            .Where(m => m.OrganizationId == task.TargetOrgId && m.IsEnabled)
            .ToListAsync();

        var result = new TaskUrgeResultDto
        {
            TaskId = taskId,
            TaskName = task.TaskName,
            TotalMembers = members.Count
        };

        foreach (var member in members)
        {
            var completedCount = await _context.MemberLearningProgress
                .CountAsync(p => p.MemberId == member.Id && p.TaskId == taskId && p.IsCompleted);

            // 未完成（完成数 < 总内容数，或总内容为0但无进度记录）
            var isIncomplete = totalContents > 0 ? completedCount < totalContents : completedCount == 0;
            if (!isIncomplete) continue;

            try
            {
                await _notification.SendAsync(new SendNotificationRequest
                {
                    PartyMemberId = member.Id,
                    Type = NotificationType.TaskReminder,
                    Title = "学习任务催办",
                    Content = $"您的学习任务「{task.TaskName}」尚未完成（{completedCount}/{totalContents}），截止时间 {task.Deadline:yyyy-MM-dd HH:mm}，请尽快完成。"
                });
                result.NotifiedCount++;
                result.NotifiedMembers.Add(member.Name);
            }
            catch { }
        }

        return result;
    }
}
