using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.Entities;

namespace PartySchoolApi.Data;

/// <summary>应用程序数据库上下文</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ===== 原有DbSet =====
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<PartyMember> PartyMembers => Set<PartyMember>();
    public DbSet<LearningContent> LearningContents => Set<LearningContent>();
    public DbSet<ContentCategory> ContentCategories => Set<ContentCategory>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ContentTag> ContentTags => Set<ContentTag>();
    public DbSet<LearningTask> LearningTasks => Set<LearningTask>();
    public DbSet<TaskContent> TaskContents => Set<TaskContent>();
    public DbSet<MemberLearningProgress> MemberLearningProgress => Set<MemberLearningProgress>();
    public DbSet<QuestionCategory> QuestionCategories => Set<QuestionCategory>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ExamPaper> ExamPapers => Set<ExamPaper>();
    public DbSet<ExamTest> ExamTests => Set<ExamTest>();
    public DbSet<MemberTestRecord> MemberTestRecords => Set<MemberTestRecord>();

    // ===== 新增DbSet =====
    public DbSet<MemberLearningReport> MemberLearningReports => Set<MemberLearningReport>();
    public DbSet<OrganizationQuarterlyReport> OrganizationQuarterlyReports => Set<OrganizationQuarterlyReport>();
    public DbSet<PartyDevelopmentProcess> PartyDevelopmentProcesses => Set<PartyDevelopmentProcess>();
    public DbSet<MeetingActivity> MeetingActivities => Set<MeetingActivity>();
    public DbSet<ActivityHeart> ActivityHearts => Set<ActivityHeart>();
    public DbSet<CheckInRecord> CheckInRecords => Set<CheckInRecord>();
    public DbSet<LearningPoint> LearningPoints => Set<LearningPoint>();
    public DbSet<MessageNotification> MessageNotifications => Set<MessageNotification>();
    public DbSet<BattleRecord> BattleRecords => Set<BattleRecord>();
    public DbSet<PairHelpRecord> PairHelpRecords => Set<PairHelpRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== 组织架构 =====
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== 党员 =====
        modelBuilder.Entity<PartyMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Phone).IsUnique();
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.Role);
            entity.HasOne(e => e.Organization)
                  .WithMany(e => e.Members)
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Role).HasConversion<int>();
            entity.Property(e => e.PointTotal).HasDefaultValue(0);
        });

        // ===== 内容分类 =====
        modelBuilder.Entity<ContentCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== 标签 =====
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ===== 内容-标签关联 =====
        modelBuilder.Entity<ContentTag>(entity =>
        {
            entity.HasKey(e => new { e.ContentId, e.TagId });
            entity.HasOne(e => e.Content).WithMany(e => e.ContentTags)
                  .HasForeignKey(e => e.ContentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag).WithMany(e => e.ContentTags)
                  .HasForeignKey(e => e.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        // ===== 学习内容 =====
        modelBuilder.Entity<LearningContent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.ContentType);
            entity.HasIndex(e => e.IsPublic);
            entity.HasOne(e => e.Category).WithMany(e => e.Contents)
                  .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.ContentType).HasConversion<int>();
            // 修正默认值：使用枚举成员
            entity.Property(e => e.SourceType).HasConversion<int>().HasDefaultValue(ContentSourceType.Manual);
        });

        // ===== 学习任务 =====
        modelBuilder.Entity<LearningTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TargetOrgId);
            entity.HasOne(e => e.TargetOrg).WithMany()
                  .HasForeignKey(e => e.TargetOrgId).OnDelete(DeleteBehavior.Restrict);
        });

        // ===== 任务-内容关联 =====
        modelBuilder.Entity<TaskContent>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.ContentId });
            entity.HasOne(e => e.Task).WithMany(e => e.TaskContents)
                  .HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Content).WithMany(e => e.TaskContents)
                  .HasForeignKey(e => e.ContentId).OnDelete(DeleteBehavior.Cascade);
        });

        // ===== 学习进度 =====
        modelBuilder.Entity<MemberLearningProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.MemberId, e.ContentId, e.TaskId });
            entity.HasOne(e => e.Member).WithMany(e => e.LearningProgresses)
                  .HasForeignKey(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Content).WithMany(e => e.LearningProgresses)
                  .HasForeignKey(e => e.ContentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Task).WithMany()
                  .HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.SetNull);
        });

        // ===== 题目分类 =====
        modelBuilder.Entity<QuestionCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ===== 题目 =====
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QuestionType);
            entity.HasIndex(e => e.CategoryId);
            entity.HasOne(e => e.Category).WithMany(e => e.Questions)
                  .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.QuestionType).HasConversion<int>();
        });

        // ===== 试卷 =====
        modelBuilder.Entity<ExamPaper>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
        });

        // ===== 测验 =====
        modelBuilder.Entity<ExamTest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PaperId);
            entity.HasIndex(e => e.TargetOrgId);
            entity.HasOne(e => e.Paper).WithMany(e => e.ExamTests)
                  .HasForeignKey(e => e.PaperId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Publisher).WithMany()
                  .HasForeignKey(e => e.PublisherId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.TargetOrg).WithMany()
                  .HasForeignKey(e => e.TargetOrgId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.IsAiGenerated).HasDefaultValue(false);
        });

        // ===== 考试记录 =====
        modelBuilder.Entity<MemberTestRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MemberId);
            entity.HasIndex(e => e.TestId);
            entity.HasIndex(e => new { e.MemberId, e.TestId });
            entity.HasOne(e => e.Member).WithMany(e => e.TestRecords)
                  .HasForeignKey(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Test).WithMany(e => e.TestRecords)
                  .HasForeignKey(e => e.TestId).OnDelete(DeleteBehavior.Cascade);
        });

        // ===== 新增表配置 =====

        // 党员学习报告
        modelBuilder.Entity<MemberLearningReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PartyMemberId);
            entity.HasOne(e => e.PartyMember).WithMany()
                  .HasForeignKey(e => e.PartyMemberId).OnDelete(DeleteBehavior.Cascade);
        });

        // 支部季度报告
        modelBuilder.Entity<OrganizationQuarterlyReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OrganizationId, e.Quarter });
            entity.HasOne(e => e.Organization).WithMany()
                  .HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        });

        // 党员发展流程
        modelBuilder.Entity<PartyDevelopmentProcess>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PartyMemberId);
            entity.HasIndex(e => e.Stage);
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.PartyMember).WithMany()
                  .HasForeignKey(e => e.PartyMemberId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Stage).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
        });

        // 三会一课活动
        modelBuilder.Entity<MeetingActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrganizationId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.ActivityTime);
            entity.HasOne(e => e.Organization).WithMany()
                  .HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Type).HasConversion<int>();
        });

        // 活动心得
        modelBuilder.Entity<ActivityHeart>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MeetingActivityId);
            entity.HasIndex(e => e.PartyMemberId);
            entity.HasOne(e => e.MeetingActivity).WithMany(e => e.ActivityHearts)
                  .HasForeignKey(e => e.MeetingActivityId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PartyMember).WithMany()
                  .HasForeignKey(e => e.PartyMemberId).OnDelete(DeleteBehavior.Cascade);
        });

        // 打卡记录
        modelBuilder.Entity<CheckInRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PartyMemberId);
            entity.HasIndex(e => e.LocationName);
            entity.HasOne(e => e.PartyMember).WithMany()
                  .HasForeignKey(e => e.PartyMemberId).OnDelete(DeleteBehavior.Cascade);
        });

        // 积分记录
        modelBuilder.Entity<LearningPoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PartyMemberId);
            entity.HasIndex(e => e.SourceType);
            entity.HasOne(e => e.PartyMember).WithMany()
                  .HasForeignKey(e => e.PartyMemberId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.SourceType).HasConversion<int>();
        });

        // 消息通知
        modelBuilder.Entity<MessageNotification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PartyMemberId);
            entity.HasIndex(e => e.IsRead);
            entity.HasOne(e => e.PartyMember).WithMany()
                  .HasForeignKey(e => e.PartyMemberId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Type).HasConversion<int>();
        });

        // PK对战记录
        modelBuilder.Entity<BattleRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ChallengerId);
            entity.HasIndex(e => e.OpponentId);
            entity.HasOne(e => e.Challenger).WithMany()
                  .HasForeignKey(e => e.ChallengerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Opponent).WithMany()
                  .HasForeignKey(e => e.OpponentId).OnDelete(DeleteBehavior.Restrict);
        });

        // 结对帮扶
        modelBuilder.Entity<PairHelpRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.HelperId);
            entity.HasIndex(e => e.HelpReceiverId);
            entity.HasOne(e => e.Helper).WithMany()
                  .HasForeignKey(e => e.HelperId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.HelpReceiver).WithMany()
                  .HasForeignKey(e => e.HelpReceiverId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}