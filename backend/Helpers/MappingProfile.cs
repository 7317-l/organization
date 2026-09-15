using AutoMapper;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using SystemTextJson = System.Text.Json;

namespace PartySchoolApi.Helpers;

/// <summary>
/// JSON反序列化辅助类（包装带可选参数的方法，使表达式树可调用）
/// </summary>
public static class JsonMappingHelper
{
    public static List<string> ToStringList(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<string>();
        var result = SystemTextJson.JsonSerializer.Deserialize<List<string>>(json);
        return result ?? new List<string>();
    }

    public static List<int> ToIntList(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<int>();
        var result = SystemTextJson.JsonSerializer.Deserialize<List<int>>(json);
        return result ?? new List<int>();
    }

    public static string ToJson<T>(List<T> list)
    {
        return SystemTextJson.JsonSerializer.Serialize(list);
    }
}

/// <summary>
/// AutoMapper映射配置
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ===== 新增映射 =====

        // PointRecord
        CreateMap<LearningPoint, PointRecordDto>()
            .ForMember(d => d.SourceTypeName, opt => opt.MapFrom(s => s.SourceType.ToString()))
            .ForMember(d => d.MemberName, opt => opt.MapFrom(s =>
                s.PartyMember != null ? s.PartyMember.Name : string.Empty));

        // Notification
        CreateMap<MessageNotification, NotificationDto>()
            .ForMember(d => d.TypeName, opt => opt.MapFrom(s => s.Type.ToString()));

        // MeetingActivity -> ListItem
        CreateMap<MeetingActivity, MeetingActivityListItemDto>()
            .ForMember(d => d.TypeName, opt => opt.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.OrganizationName, opt => opt.MapFrom(s =>
                s.Organization != null ? s.Organization.Name : string.Empty))
            .ForMember(d => d.HeartCount, opt => opt.MapFrom(s => s.ActivityHearts.Count));

        // ActivityHeart
        CreateMap<ActivityHeart, ActivityHeartDto>()
            .ForMember(d => d.MemberName, opt => opt.MapFrom(s =>
                s.PartyMember != null ? s.PartyMember.Name : string.Empty));

        // CheckInRecord
        CreateMap<CheckInRecord, CheckInRecordDto>()
            .ForMember(d => d.MemberName, opt => opt.MapFrom(s =>
                s.PartyMember != null ? s.PartyMember.Name : string.Empty));

        // ===== Organization =====
        CreateMap<Organization, OrganizationTreeDto>()
            .ForMember(d => d.Children, opt => opt.MapFrom(s => s.Children));

        // ===== PartyMember =====
        CreateMap<PartyMember, MemberListItemDto>()
            .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.OrganizationName, opt => opt.MapFrom(s =>
                s.Organization != null ? s.Organization.Name : null));

        CreateMap<CreateMemberRequest, PartyMember>()
            .ForMember(d => d.PasswordHash, opt => opt.Ignore())
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.RefreshToken, opt => opt.Ignore())
            .ForMember(d => d.RefreshTokenExpiry, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.IsEnabled, opt => opt.Ignore())
            .ForMember(d => d.LearningProgresses, opt => opt.Ignore())
            .ForMember(d => d.TestRecords, opt => opt.Ignore())
            .ForMember(d => d.Organization, opt => opt.Ignore());

        CreateMap<UpdateMemberRequest, PartyMember>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.PasswordHash, opt => opt.Ignore())
            .ForMember(d => d.RefreshToken, opt => opt.Ignore())
            .ForMember(d => d.RefreshTokenExpiry, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.LearningProgresses, opt => opt.Ignore())
            .ForMember(d => d.TestRecords, opt => opt.Ignore())
            .ForMember(d => d.Organization, opt => opt.Ignore());

        CreateMap<PartyMember, UserInfoDto>()
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.OrganizationName, opt => opt.MapFrom(s =>
                s.Organization != null ? s.Organization.Name : null));

        // ===== LearningContent =====
        CreateMap<LearningContent, ContentListItemDto>()
            .ForMember(d => d.ContentTypeName, opt => opt.MapFrom(s => s.ContentType.ToString()))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s =>
                s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.Tags, opt => opt.MapFrom(s =>
                s.ContentTags
                    .Where(ct => ct.Tag != null)
                    .Select(ct => ct.Tag.Name)
                    .ToList()));

        CreateMap<LearningContent, ContentDetailDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s =>
                s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.Tags, opt => opt.MapFrom(s =>
                s.ContentTags
                    .Where(ct => ct.Tag != null)
                    .Select(ct => new TagDto { Id = ct.TagId, Name = ct.Tag.Name })
                    .ToList()));

        CreateMap<CreateContentRequest, LearningContent>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore())
            .ForMember(d => d.ContentTags, opt => opt.Ignore())
            .ForMember(d => d.TaskContents, opt => opt.Ignore())
            .ForMember(d => d.LearningProgresses, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());

        CreateMap<UpdateContentRequest, LearningContent>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore())
            .ForMember(d => d.ContentTags, opt => opt.Ignore())
            .ForMember(d => d.TaskContents, opt => opt.Ignore())
            .ForMember(d => d.LearningProgresses, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());

        // ===== Tag =====
        CreateMap<Tag, TagDto>();

        // ===== ContentCategory =====
        CreateMap<ContentCategory, ContentCategoryTreeDto>()
            .ForMember(d => d.Children, opt => opt.MapFrom(s => s.Children));

        // ===== LearningTask =====
        CreateMap<LearningTask, TaskListItemDto>()
            .ForMember(d => d.TargetOrgName, opt => opt.MapFrom(s =>
                s.TargetOrg != null ? s.TargetOrg.Name : null))
            .ForMember(d => d.ContentCount, opt => opt.MapFrom(s => s.TaskContents.Count));

        CreateMap<LearningTask, TaskDetailDto>()
            .ForMember(d => d.TargetOrgName, opt => opt.MapFrom(s =>
                s.TargetOrg != null ? s.TargetOrg.Name : null))
            .ForMember(d => d.Contents, opt => opt.Ignore());

        CreateMap<CreateTaskRequest, LearningTask>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.TargetOrg, opt => opt.Ignore())
            .ForMember(d => d.TaskContents, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());

        CreateMap<UpdateTaskRequest, LearningTask>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.TargetOrg, opt => opt.Ignore())
            .ForMember(d => d.TaskContents, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());

        // ===== Question =====
        CreateMap<Question, QuestionListItemDto>()
            .ForMember(d => d.QuestionTypeName, opt => opt.MapFrom(s => s.QuestionType.ToString()))
            .ForMember(d => d.Options, opt => opt.MapFrom(s => JsonMappingHelper.ToStringList(s.Options)))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s =>
                s.Category != null ? s.Category.Name : null));

        CreateMap<CreateQuestionRequest, Question>()
            .ForMember(d => d.Options, opt => opt.MapFrom(s => JsonMappingHelper.ToJson(s.Options)))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());

        CreateMap<UpdateQuestionRequest, Question>()
            .ForMember(d => d.Options, opt => opt.MapFrom(s => JsonMappingHelper.ToJson(s.Options)))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());

        // ===== QuestionCategory =====
        CreateMap<QuestionCategory, QuestionCategoryDto>()
            .ForMember(d => d.QuestionCount, opt => opt.MapFrom(s => s.Questions.Count));

        // ===== ExamPaper =====
        CreateMap<ExamPaper, ExamPaperListItemDto>()
            .ForMember(d => d.QuestionCount, opt => opt.MapFrom(s =>
                JsonMappingHelper.ToIntList(s.QuestionIds).Count));

        CreateMap<CreateExamPaperRequest, ExamPaper>()
            .ForMember(d => d.QuestionIds, opt => opt.MapFrom(s => JsonMappingHelper.ToJson(s.QuestionIds)))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.TotalScore, opt => opt.Ignore())
            .ForMember(d => d.ExamTests, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());

        CreateMap<UpdateExamPaperRequest, ExamPaper>()
            .ForMember(d => d.QuestionIds, opt => opt.MapFrom(s => JsonMappingHelper.ToJson(s.QuestionIds)))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.TotalScore, opt => opt.Ignore())
            .ForMember(d => d.ExamTests, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());

        // ===== ExamTest =====
        CreateMap<ExamTest, ExamTestListItemDto>()
            .ForMember(d => d.PaperName, opt => opt.MapFrom(s =>
                s.Paper != null ? s.Paper.Name : string.Empty))
            .ForMember(d => d.TargetOrgName, opt => opt.MapFrom(s =>
                s.TargetOrg != null ? s.TargetOrg.Name : null))
            .ForMember(d => d.ParticipantCount, opt => opt.MapFrom(s => s.TestRecords.Count));

        CreateMap<CreateExamTestRequest, ExamTest>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Paper, opt => opt.Ignore())
            .ForMember(d => d.Publisher, opt => opt.Ignore())
            .ForMember(d => d.TargetOrg, opt => opt.Ignore())
            .ForMember(d => d.TestRecords, opt => opt.Ignore())
            .ForMember(d => d.PublisherId, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore());

        // ===== MemberTestRecord =====
        CreateMap<MemberTestRecord, MemberTestRecordDto>()
            .ForMember(d => d.MemberName, opt => opt.MapFrom(s =>
                s.Member != null ? s.Member.Name : string.Empty));
    }
}
