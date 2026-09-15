using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Helpers;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.Entities;
using System.Text.Json;

namespace PartySchoolApi.Helpers;

/// <summary>
/// 种子数据初始化器
/// 在程序启动时自动检查并初始化基础数据，确保从仓库克隆后运行即可有完整数据
/// </summary>
public static class SeedDataInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // 确保数据库已创建
        await context.Database.EnsureCreatedAsync();

        await SeedOrganizationsAsync(context);
        await SeedPartyMembersAsync(context);
        await SeedContentCategoriesAsync(context);
        await SeedTagsAsync(context);
        await SeedLearningContentsAsync(context);
        await SeedContentTagsAsync(context);
        await SeedQuestionCategoriesAsync(context);
        await SeedQuestionsAsync(context);
        await SeedExamPapersAsync(context);
        await SeedExamTestsAsync(context);
        await SeedLearningTasksAsync(context);
        await SeedTaskContentsAsync(context);
        await SeedLearningPointsAsync(context);
        await SeedLearningProgressAsync(context);
        await SeedTestRecordsAsync(context);
    }

    #region 组织架构
    private static async Task SeedOrganizationsAsync(AppDbContext context)
    {
        if (await context.Organizations.AnyAsync()) return;

        var orgs = new List<Organization>
        {
            new() { Id = 1, Name = "中共党校委员会", ParentId = null, CreatedAt = new DateTime(2024,1,1) },
            new() { Id = 2, Name = "第一党支部", ParentId = 1, CreatedAt = new DateTime(2024,1,1) },
            new() { Id = 3, Name = "第二党支部", ParentId = 1, CreatedAt = new DateTime(2024,1,1) },
            new() { Id = 4, Name = "第三党支部", ParentId = 1, CreatedAt = new DateTime(2024,1,1) },
            new() { Id = 5, Name = "第四党支部", ParentId = 1, CreatedAt = new DateTime(2024,1,1) },
            new() { Id = 6, Name = "第五党支部", ParentId = 1, CreatedAt = new DateTime(2024,1,1) },
            new() { Id = 7, Name = "离退休党支部", ParentId = 1, CreatedAt = new DateTime(2024,1,1) }
        };
        context.Organizations.AddRange(orgs);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 党员用户
    private static async Task SeedPartyMembersAsync(AppDbContext context)
    {
        if (await context.PartyMembers.AnyAsync()) return;

        var pwdHash = PasswordHelper.HashPassword("123456");
        var members = new List<PartyMember>
        {
            new() { Id = 1, Name = "系统管理员", Phone = "13800000000", PasswordHash = pwdHash, Role = UserRole.SystemAdmin, OrganizationId = 1, IsEnabled = true, PointTotal = 0, CreatedAt = new DateTime(2024,1,1) },
            new() { Id = 2, Name = "李建国", Phone = "13800000001", PasswordHash = pwdHash, Role = UserRole.BranchSecretary, OrganizationId = 2, IsEnabled = true, PointTotal = 1280, CreatedAt = new DateTime(2024,1,1) },
            new() { Id = 3, Name = "王芳", Phone = "13800000002", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 2, IsEnabled = true, PointTotal = 1150, CreatedAt = new DateTime(2024,2,1) },
            new() { Id = 4, Name = "张明", Phone = "13800000003", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 3, IsEnabled = true, PointTotal = 980, CreatedAt = new DateTime(2024,2,15) },
            new() { Id = 5, Name = "刘红", Phone = "13800000004", PasswordHash = pwdHash, Role = UserRole.BranchSecretary, OrganizationId = 3, IsEnabled = true, PointTotal = 850, CreatedAt = new DateTime(2024,3,1) },
            new() { Id = 6, Name = "陈伟", Phone = "13800000005", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 4, IsEnabled = true, PointTotal = 720, CreatedAt = new DateTime(2024,3,10) },
            new() { Id = 7, Name = "赵丽", Phone = "13800000006", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 4, IsEnabled = true, PointTotal = 650, CreatedAt = new DateTime(2024,3,20) },
            new() { Id = 8, Name = "孙强", Phone = "13800000007", PasswordHash = pwdHash, Role = UserRole.BranchSecretary, OrganizationId = 4, IsEnabled = true, PointTotal = 580, CreatedAt = new DateTime(2024,4,1) },
            new() { Id = 9, Name = "周敏", Phone = "13800000008", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 5, IsEnabled = true, PointTotal = 520, CreatedAt = new DateTime(2024,4,10) },
            new() { Id = 10, Name = "吴涛", Phone = "13800000009", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 5, IsEnabled = true, PointTotal = 450, CreatedAt = new DateTime(2024,4,20) },
            new() { Id = 11, Name = "郑雪", Phone = "13800000010", PasswordHash = pwdHash, Role = UserRole.BranchSecretary, OrganizationId = 5, IsEnabled = true, PointTotal = 380, CreatedAt = new DateTime(2024,5,1) },
            new() { Id = 12, Name = "冯磊", Phone = "13800000011", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 6, IsEnabled = true, PointTotal = 320, CreatedAt = new DateTime(2024,5,10) },
            new() { Id = 13, Name = "蒋琳", Phone = "13800000012", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 6, IsEnabled = true, PointTotal = 280, CreatedAt = new DateTime(2024,5,20) },
            new() { Id = 14, Name = "沈浩", Phone = "13800000013", PasswordHash = pwdHash, Role = UserRole.BranchSecretary, OrganizationId = 6, IsEnabled = true, PointTotal = 250, CreatedAt = new DateTime(2024,6,1) },
            new() { Id = 15, Name = "韩梅", Phone = "13800000014", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 7, IsEnabled = true, PointTotal = 200, CreatedAt = new DateTime(2024,6,10) },
            new() { Id = 16, Name = "杨帆", Phone = "13800000015", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 2, IsEnabled = true, PointTotal = 180, CreatedAt = new DateTime(2024,6,20) },
            new() { Id = 17, Name = "朱婷", Phone = "13800000016", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 3, IsEnabled = true, PointTotal = 480, CreatedAt = new DateTime(2024,7,1) },
            new() { Id = 18, Name = "秦刚", Phone = "13800000017", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 4, IsEnabled = true, PointTotal = 150, CreatedAt = new DateTime(2024,7,10) },
            new() { Id = 19, Name = "许静", Phone = "13800000018", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 5, IsEnabled = true, PointTotal = 220, CreatedAt = new DateTime(2024,7,20) },
            new() { Id = 20, Name = "何军", Phone = "13800000019", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 6, IsEnabled = true, PointTotal = 120, CreatedAt = new DateTime(2024,8,1) },
            new() { Id = 21, Name = "吕萍", Phone = "13800000020", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 7, IsEnabled = true, PointTotal = 280, CreatedAt = new DateTime(2024,8,10) },
            new() { Id = 22, Name = "施伟", Phone = "13800000021", PasswordHash = pwdHash, Role = UserRole.PartyMember, OrganizationId = 2, IsEnabled = false, PointTotal = 50, CreatedAt = new DateTime(2024,8,20) }
        };
        context.PartyMembers.AddRange(members);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 内容分类
    private static async Task SeedContentCategoriesAsync(AppDbContext context)
    {
        if (await context.ContentCategories.AnyAsync()) return;

        var cats = new List<ContentCategory>
        {
            new() { Id = 1, Name = "理论学习", ParentId = null },
            new() { Id = 2, Name = "党章党规", ParentId = null },
            new() { Id = 3, Name = "党史教育", ParentId = null },
            new() { Id = 4, Name = "二十大专题", ParentId = null },
            new() { Id = 5, Name = "廉政建设", ParentId = null }
        };
        context.ContentCategories.AddRange(cats);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 标签
    private static async Task SeedTagsAsync(AppDbContext context)
    {
        if (await context.Tags.AnyAsync()) return;

        var tags = new List<Tag>
        {
            new() { Id = 1, Name = "视频" },
            new() { Id = 2, Name = "文章" },
            new() { Id = 3, Name = "党章" },
            new() { Id = 4, Name = "党史" },
            new() { Id = 5, Name = "二十大" },
            new() { Id = 6, Name = "廉政" },
            new() { Id = 7, Name = "习近平思想" },
            new() { Id = 8, Name = "必修" }
        };
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 学习内容
    private static async Task SeedLearningContentsAsync(AppDbContext context)
    {
        if (await context.LearningContents.AnyAsync()) return;

        var contents = new List<LearningContent>
        {
            new() { Id = 1, Title = "习近平新时代中国特色社会主义思想三十讲", ContentType = ContentType.Video, CategoryId = 1, IsPublic = true, VideoUrl = "https://example.com/video1.mp4",
                Body = "<p>习近平新时代中国特色社会主义思想是当代中国马克思主义、二十一世纪马克思主义，是中华文化和中国精神的时代精华，实现了马克思主义中国化时代化新的飞跃。</p><p>这一思想立足中国特色社会主义进入新时代的历史方位，深刻回答了新时代坚持和发展什么样的中国特色社会主义、怎样坚持和发展中国特色社会主义等重大时代课题。</p><p>学习贯彻习近平新时代中国特色社会主义思想，必须深刻领悟\"两个确立\"的决定性意义，增强\"四个意识\"、坚定\"四个自信\"、做到\"两个维护\"。</p>",
                CreatedAt = new DateTime(2026,8,10) },
            new() { Id = 2, Title = "中国共产党章程（2022年修订）", ContentType = ContentType.Article, CategoryId = 2, IsPublic = true,
                Body = "<p>《中国共产党章程》是党的总章程，是全党必须共同遵守的根本行为规范。中国共产党第二十次全国代表大会通过的党章修正案，共修改50处。</p><p>党章总纲部分增写了党的百年奋斗重大成就和历史经验的内容，调整完善了党的奋斗目标的表述，充实了\"五位一体\"总体布局方面的内容。</p><p>认真学习党章、严格遵守党章，是加强党的建设的一项基础性经常性工作，也是全党同志的应尽义务和庄严责任。</p>",
                CreatedAt = new DateTime(2026,8,8) },
            new() { Id = 3, Title = "党史故事100讲·五四运动", ContentType = ContentType.Video, CategoryId = 3, IsPublic = true, VideoUrl = "https://example.com/video3.mp4",
                Body = "<p>五四运动，是1919年5月4日发生在北京的一场以青年学生为主，广大群众、市民、工商人士等阶层共同参与的爱国运动。</p><p>五四运动是中国新民主主义革命的开端，促进了马克思主义在中国的传播及其与中国工人运动的结合，为中国共产党成立做了思想上干部上的准备。</p><p>五四精神的核心是爱国主义。在新时代，我们要继承和发扬五四精神，坚定理想信念，投身强国伟业。</p>",
                CreatedAt = new DateTime(2026,8,5) },
            new() { Id = 4, Title = "二十大报告全文解读", ContentType = ContentType.Article, CategoryId = 4, IsPublic = true,
                Body = "<p>中国共产党第二十次全国代表大会于2022年10月16日至22日在北京举行。大会的主题是：高举中国特色社会主义伟大旗帜，全面贯彻新时代中国特色社会主义思想，弘扬伟大建党精神，自信自强、守正创新，踔厉奋发、勇毅前行，为全面建设社会主义现代化国家、全面推进中华民族伟大复兴而团结奋斗。</p><p>报告共分十五个部分，系统阐述了新时代新征程中国共产党的使命任务，对全面建成社会主义现代化强国两步走战略安排进行了宏观展望。</p><p>党的二十大报告是党团结带领全国各族人民夺取中国特色社会主义新胜利的政治宣言和行动纲领。</p>",
                CreatedAt = new DateTime(2026,8,1) },
            new() { Id = 5, Title = "中国共产党纪律处分条例解读", ContentType = ContentType.Article, CategoryId = 5, IsPublic = true,
                Body = "<p>《中国共产党纪律处分条例》是根据《中国共产党章程》制定的党内法规，是维护党的章程和其他党内法规，严肃党的纪律，纯洁党的组织的重要保障。</p><p>2023年12月修订的《条例》共3编158条，与2018年《条例》相比，新增16条，修改76条，整合2条。</p><p>《条例》明确规定了党的纪律主要包括政治纪律、组织纪律、廉洁纪律、群众纪律、工作纪律、生活纪律。政治纪律是最重要、最根本、最关键的纪律。</p>",
                CreatedAt = new DateTime(2026,7,28) },
            new() { Id = 6, Title = "党的二十大党章修正案学习问答", ContentType = ContentType.Article, CategoryId = 2, IsPublic = true,
                Body = "<p>党的二十大通过的党章修正案，体现了党的十九大以来党的理论创新、实践创新、制度创新成果，对坚持和加强党的全面领导、坚定不移推进全面从严治党提出了明确要求。</p><p>党章修正案增写了党的百年奋斗重大成就和历史经验的内容，调整完善了党的奋斗目标的表述，充实了\"五位一体\"总体布局方面的内容。</p><p>学习党章修正案，要原原本本学、逐字逐句学，深刻领会修改的重大意义和主要内容。</p>",
                CreatedAt = new DateTime(2026,7,25) },
            new() { Id = 7, Title = "习近平关于党的建设的重要思想", ContentType = ContentType.Video, CategoryId = 1, IsPublic = true, VideoUrl = "https://example.com/video7.mp4",
                Body = "<p>习近平关于党的建设的重要思想，是习近平新时代中国特色社会主义思想的重要组成部分，是新时代党的建设的根本遵循和行动指南。</p><p>2023年6月召开的全国组织工作会议，首次正式提出和系统阐述\"习近平总书记关于党的建设的重要思想\"，并用\"十三个坚持\"集中概括了这一重要思想的主要内容。</p><p>这一重要思想突出全面从严治党这个主题主线，以一系列原创性成果极大丰富和发展了马克思主义建党学说。</p>",
                CreatedAt = new DateTime(2026,7,20) },
            new() { Id = 8, Title = "中国共产党廉洁自律准则", ContentType = ContentType.Article, CategoryId = 5, IsPublic = true,
                Body = "<p>《中国共产党廉洁自律准则》是中国共产党执政以来第一部坚持正面倡导、面向全体党员的规范全党廉洁自律工作的重要基础性法规。</p><p>《准则》共8条、281字，包括导语、党员廉洁自律规范和党员领导干部廉洁自律规范等3部分。</p><p>党员廉洁自律规范：坚持公私分明，先公后私，克己奉公；坚持崇廉拒腐，清白做人，干净做事；坚持尚俭戒奢，艰苦朴素，勤俭节约；坚持吃苦在前，享受在后，甘于奉献。</p>",
                CreatedAt = new DateTime(2026,7,15) },
            new() { Id = 9, Title = "党史学习教育专题", ContentType = ContentType.Article, CategoryId = 3, IsPublic = true,
                Body = "<p>2021年2月20日，党史学习教育动员大会在北京召开，习近平总书记出席会议并发表重要讲话，深刻阐述了开展党史学习教育的重大意义。</p><p>全党同志要做到学史明理、学史增信、学史崇德、学史力行，学党史、悟思想、办实事、开新局。</p><p>我们党的一百年，是矢志践行初心使命的一百年，是筚路蓝缕奠基立业的一百年，是创造辉煌开辟未来的一百年。</p>",
                CreatedAt = new DateTime(2026,7,10) },
            new() { Id = 10, Title = "党的二十届三中全会精神", ContentType = ContentType.Article, CategoryId = 4, IsPublic = true,
                Body = "<p>中国共产党第二十届中央委员会第三次全体会议，于2024年7月15日至18日在北京举行。全会审议通过了《中共中央关于进一步全面深化改革、推进中国式现代化的决定》。</p><p>全会认为，党的十八大以来，以习近平同志为核心的党中央把全面深化改革纳入\"四个全面\"战略布局，以巨大的政治勇气全面深化改革，各领域基础性制度框架基本建立。</p><p>全会提出，进一步全面深化改革的总目标是，继续完善和发展中国特色社会主义制度，推进国家治理体系和治理能力现代化。</p>",
                CreatedAt = new DateTime(2026,7,5) }
        };
        context.LearningContents.AddRange(contents);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 内容标签关联
    private static async Task SeedContentTagsAsync(AppDbContext context)
    {
        if (await context.ContentTags.AnyAsync()) return;

        var tags = new List<ContentTag>
        {
            new() { ContentId = 1, TagId = 1 }, new() { ContentId = 1, TagId = 7 }, new() { ContentId = 1, TagId = 8 },
            new() { ContentId = 2, TagId = 2 }, new() { ContentId = 2, TagId = 3 },
            new() { ContentId = 3, TagId = 1 }, new() { ContentId = 3, TagId = 4 },
            new() { ContentId = 4, TagId = 2 }, new() { ContentId = 4, TagId = 5 },
            new() { ContentId = 5, TagId = 2 }, new() { ContentId = 5, TagId = 6 },
            new() { ContentId = 6, TagId = 2 }, new() { ContentId = 6, TagId = 3 }, new() { ContentId = 6, TagId = 5 },
            new() { ContentId = 7, TagId = 1 }, new() { ContentId = 7, TagId = 7 },
            new() { ContentId = 8, TagId = 2 }, new() { ContentId = 8, TagId = 6 },
            new() { ContentId = 9, TagId = 2 }, new() { ContentId = 9, TagId = 4 },
            new() { ContentId = 10, TagId = 2 }, new() { ContentId = 10, TagId = 5 }
        };
        context.ContentTags.AddRange(tags);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 题目分类
    private static async Task SeedQuestionCategoriesAsync(AppDbContext context)
    {
        if (await context.QuestionCategories.AnyAsync()) return;

        var cats = new List<QuestionCategory>
        {
            new() { Id = 1, Name = "党史" },
            new() { Id = 2, Name = "党章" },
            new() { Id = 3, Name = "二十大" },
            new() { Id = 4, Name = "廉政" },
            new() { Id = 5, Name = "习近平思想" }
        };
        context.QuestionCategories.AddRange(cats);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 题目
    private static async Task SeedQuestionsAsync(AppDbContext context)
    {
        if (await context.Questions.AnyAsync()) return;

        var questions = new List<Question>
        {
            new() { Id = 1, QuestionType = QuestionType.SingleChoice, Stem = "中国新民主主义革命的开端是（ ）。", Options = "[\"辛亥革命\",\"五四运动\",\"中国共产党成立\",\"新文化运动\"]", CorrectAnswer = "1", Score = 5, CategoryId = 1 },
            new() { Id = 2, QuestionType = QuestionType.SingleChoice, Stem = "中国共产党第一次全国代表大会召开的时间是（ ）。", Options = "[\"1919年\",\"1920年\",\"1921年\",\"1922年\"]", CorrectAnswer = "2", Score = 5, CategoryId = 1 },
            new() { Id = 3, QuestionType = QuestionType.SingleChoice, Stem = "中国共产党的根本组织原则是（ ）。", Options = "[\"民主集中制\",\"集体领导制\",\"个人负责制\",\"党领导下的校长负责制\"]", CorrectAnswer = "0", Score = 5, CategoryId = 2 },
            new() { Id = 4, QuestionType = QuestionType.SingleChoice, Stem = "党的最高理想和最终目标是（ ）。", Options = "[\"实现共产主义\",\"建设社会主义现代化强国\",\"实现中华民族伟大复兴\",\"全面建成小康社会\"]", CorrectAnswer = "0", Score = 5, CategoryId = 2 },
            new() { Id = 5, QuestionType = QuestionType.SingleChoice, Stem = "\"四个意识\"不包括以下哪一项（ ）。", Options = "[\"政治意识\",\"大局意识\",\"核心意识\",\"纪律意识\"]", CorrectAnswer = "3", Score = 5, CategoryId = 5 },
            new() { Id = 6, QuestionType = QuestionType.SingleChoice, Stem = "党的二十大召开的时间是（ ）。", Options = "[\"2021年\",\"2022年\",\"2023年\",\"2024年\"]", CorrectAnswer = "1", Score = 5, CategoryId = 3 },
            new() { Id = 7, QuestionType = QuestionType.SingleChoice, Stem = "中国式现代化的本质要求不包括（ ）。", Options = "[\"坚持中国共产党领导\",\"坚持中国特色社会主义\",\"实现高质量发展\",\"实行西方民主制度\"]", CorrectAnswer = "3", Score = 5, CategoryId = 3 },
            new() { Id = 8, QuestionType = QuestionType.SingleChoice, Stem = "党的纪律处分不包括以下哪一项（ ）。", Options = "[\"警告\",\"严重警告\",\"撤销党内职务\",\"罚款\"]", CorrectAnswer = "3", Score = 5, CategoryId = 4 },
            new() { Id = 9, QuestionType = QuestionType.TrueFalse, Stem = "党章是党的总章程，对全党具有最高的约束力。", Options = "[\"正确\",\"错误\"]", CorrectAnswer = "0", Score = 5, CategoryId = 2 },
            new() { Id = 10, QuestionType = QuestionType.TrueFalse, Stem = "中国共产党成立于1921年7月1日。", Options = "[\"正确\",\"错误\"]", CorrectAnswer = "0", Score = 5, CategoryId = 1 },
            new() { Id = 11, QuestionType = QuestionType.MultiChoice, Stem = "党的基层组织的基本任务包括（ ）。", Options = "[\"宣传和执行党的路线方针政策\",\"组织党员学习\",\"对党员进行教育管理和监督\",\"密切联系群众\"]", CorrectAnswer = "[0,1,2,3]", Score = 10, CategoryId = 2 },
            new() { Id = 12, QuestionType = QuestionType.MultiChoice, Stem = "\"四个自信\"包括（ ）。", Options = "[\"道路自信\",\"理论自信\",\"制度自信\",\"文化自信\"]", CorrectAnswer = "[0,1,2,3]", Score = 10, CategoryId = 5 },
            new() { Id = 13, QuestionType = QuestionType.SingleChoice, Stem = "习近平新时代中国特色社会主义思想的核心内容是（ ）。", Options = "[\"八个明确\",\"十个明确\",\"十四个坚持\",\"十个明确和十四个坚持\"]", CorrectAnswer = "3", Score = 5, CategoryId = 5 },
            new() { Id = 14, QuestionType = QuestionType.SingleChoice, Stem = "全面从严治党的核心是（ ）。", Options = "[\"加强党的领导\",\"加强党的建设\",\"加强党内监督\",\"加强纪律建设\"]", CorrectAnswer = "0", Score = 5, CategoryId = 4 },
            new() { Id = 15, QuestionType = QuestionType.TrueFalse, Stem = "\"两个维护\"是指坚决维护习近平总书记党中央的核心、全党的核心地位，坚决维护党中央权威和集中统一领导。", Options = "[\"正确\",\"错误\"]", CorrectAnswer = "0", Score = 5, CategoryId = 5 },
            new() { Id = 16, QuestionType = QuestionType.SingleChoice, Stem = "红军长征胜利结束的时间是（ ）。", Options = "[\"1934年\",\"1935年\",\"1936年\",\"1937年\"]", CorrectAnswer = "2", Score = 5, CategoryId = 1 },
            new() { Id = 17, QuestionType = QuestionType.SingleChoice, Stem = "党员的党龄从（ ）之日算起。", Options = "[\"递交入党申请书\",\"被确定为入党积极分子\",\"预备期满转为正式党员\",\"被批准为预备党员\"]", CorrectAnswer = "2", Score = 5, CategoryId = 2 },
            new() { Id = 18, QuestionType = QuestionType.MultiChoice, Stem = "党的二十大报告提出的\"三个务必\"包括（ ）。", Options = "[\"务必不忘初心、牢记使命\",\"务必谦虚谨慎、艰苦奋斗\",\"务必敢于斗争、善于斗争\",\"务必实事求是、与时俱进\"]", CorrectAnswer = "[0,1,2]", Score = 10, CategoryId = 3 },
            new() { Id = 19, QuestionType = QuestionType.TrueFalse, Stem = "党风廉政建设和反腐败斗争永远在路上。", Options = "[\"正确\",\"错误\"]", CorrectAnswer = "0", Score = 5, CategoryId = 4 },
            new() { Id = 20, QuestionType = QuestionType.SingleChoice, Stem = "新时代我国社会主要矛盾是（ ）。", Options = "[\"人民日益增长的物质文化需要同落后的社会生产之间的矛盾\",\"人民日益增长的美好生活需要和不平衡不充分的发展之间的矛盾\",\"无产阶级和资产阶级的矛盾\",\"社会主义和资本主义的矛盾\"]", CorrectAnswer = "1", Score = 5, CategoryId = 5 }
        };
        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 试卷
    private static async Task SeedExamPapersAsync(AppDbContext context)
    {
        if (await context.ExamPapers.AnyAsync()) return;

        var papers = new List<ExamPaper>
        {
            new() { Id = 1, Name = "党章知识测验", Description = "考查党员对党章的理解和掌握程度", QuestionIds = "[1,2,3,4,9,11,17]", TotalScore = 45, CreatedAt = new DateTime(2026,7,1) },
            new() { Id = 2, Name = "党史知识竞赛", Description = "考查党员对党史的了解和掌握", QuestionIds = "[1,2,10,16,5,6,7,13,15,18,20]", TotalScore = 65, CreatedAt = new DateTime(2026,7,15) }
        };
        context.ExamPapers.AddRange(papers);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 测验
    private static async Task SeedExamTestsAsync(AppDbContext context)
    {
        if (await context.ExamTests.AnyAsync()) return;

        var tests = new List<ExamTest>
        {
            new() { Id = 1, PaperId = 1, PublisherId = 2, TargetOrgId = 1, TimeLimitMinutes = 30, Deadline = new DateTime(2026,9,30,23,59,59), CreatedAt = new DateTime(2026,8,1) },
            new() { Id = 2, PaperId = 2, PublisherId = 5, TargetOrgId = 1, TimeLimitMinutes = 45, Deadline = new DateTime(2026,10,15,23,59,59), CreatedAt = new DateTime(2026,8,15) }
        };
        context.ExamTests.AddRange(tests);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 学习任务
    private static async Task SeedLearningTasksAsync(AppDbContext context)
    {
        if (await context.LearningTasks.AnyAsync()) return;

        var tasks = new List<LearningTask>
        {
            new() { Id = 1, TaskName = "8月主题党日学习任务", TargetOrgId = 1, Deadline = new DateTime(2026,8,31,23,59,59), CreatedAt = new DateTime(2026,8,1) },
            new() { Id = 2, TaskName = "7月党课学习", TargetOrgId = 1, Deadline = new DateTime(2026,7,31,23,59,59), CreatedAt = new DateTime(2026,7,1) }
        };
        context.LearningTasks.AddRange(tasks);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 任务内容关联
    private static async Task SeedTaskContentsAsync(AppDbContext context)
    {
        if (await context.TaskContents.AnyAsync()) return;

        var taskContents = new List<TaskContent>
        {
            new() { TaskId = 1, ContentId = 1 }, new() { TaskId = 1, ContentId = 4 },
            new() { TaskId = 1, ContentId = 7 }, new() { TaskId = 1, ContentId = 10 },
            new() { TaskId = 1, ContentId = 2 },
            new() { TaskId = 2, ContentId = 2 }, new() { TaskId = 2, ContentId = 3 },
            new() { TaskId = 2, ContentId = 5 }, new() { TaskId = 2, ContentId = 8 },
            new() { TaskId = 2, ContentId = 6 }, new() { TaskId = 2, ContentId = 9 },
            new() { TaskId = 2, ContentId = 1 }, new() { TaskId = 2, ContentId = 4 }
        };
        context.TaskContents.AddRange(taskContents);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 积分记录
    private static async Task SeedLearningPointsAsync(AppDbContext context)
    {
        if (await context.LearningPoints.AnyAsync()) return;

        var points = new List<LearningPoint>
        {
            // 党员2（李建国）
            new() { Id = 1, PartyMemberId = 2, SourceType = PointSourceType.WatchVideo, SourceId = 1, Points = 10, EarnedAt = new DateTime(2026,7,5,9,0,0) },
            new() { Id = 2, PartyMemberId = 2, SourceType = PointSourceType.CompleteExam, SourceId = 1, Points = 80, EarnedAt = new DateTime(2026,7,15,10,0,0) },
            new() { Id = 3, PartyMemberId = 2, SourceType = PointSourceType.ActivityCheckIn, Points = 5, EarnedAt = new DateTime(2026,7,20,8,30,0) },
            new() { Id = 4, PartyMemberId = 2, SourceType = PointSourceType.WatchVideo, SourceId = 7, Points = 10, EarnedAt = new DateTime(2026,8,1,11,0,0) },
            new() { Id = 5, PartyMemberId = 2, SourceType = PointSourceType.CompleteExam, SourceId = 2, Points = 60, EarnedAt = new DateTime(2026,8,5,15,0,0) },
            new() { Id = 6, PartyMemberId = 2, SourceType = PointSourceType.BattleVictory, SourceId = 1, Points = 20, EarnedAt = new DateTime(2026,8,10,20,0,0) },
            // 党员3（王芳）
            new() { Id = 7, PartyMemberId = 3, SourceType = PointSourceType.WatchVideo, SourceId = 1, Points = 10, EarnedAt = new DateTime(2026,7,8,10,0,0) },
            new() { Id = 8, PartyMemberId = 3, SourceType = PointSourceType.CompleteExam, SourceId = 1, Points = 70, EarnedAt = new DateTime(2026,7,18,14,0,0) },
            new() { Id = 9, PartyMemberId = 3, SourceType = PointSourceType.WatchVideo, SourceId = 3, Points = 10, EarnedAt = new DateTime(2026,8,2,11,0,0) },
            new() { Id = 10, PartyMemberId = 3, SourceType = PointSourceType.CompleteExam, SourceId = 2, Points = 55, EarnedAt = new DateTime(2026,8,8,16,0,0) },
            // 党员4（张明）
            new() { Id = 11, PartyMemberId = 4, SourceType = PointSourceType.WatchVideo, SourceId = 2, Points = 10, EarnedAt = new DateTime(2026,7,10,9,0,0) },
            new() { Id = 12, PartyMemberId = 4, SourceType = PointSourceType.CompleteExam, SourceId = 1, Points = 60, EarnedAt = new DateTime(2026,7,20,14,0,0) },
            new() { Id = 13, PartyMemberId = 4, SourceType = PointSourceType.WatchVideo, SourceId = 4, Points = 10, EarnedAt = new DateTime(2026,8,1,11,0,0) },
            new() { Id = 14, PartyMemberId = 4, SourceType = PointSourceType.ActivityCheckIn, Points = 5, EarnedAt = new DateTime(2026,8,12,8,0,0) },
            new() { Id = 15, PartyMemberId = 4, SourceType = PointSourceType.WatchVideo, SourceId = 6, Points = 10, EarnedAt = new DateTime(2026,8,18,13,0,0) },
            new() { Id = 16, PartyMemberId = 4, SourceType = PointSourceType.CompleteExam, SourceId = 2, Points = 35, EarnedAt = new DateTime(2026,8,22,15,0,0) }
        };
        context.LearningPoints.AddRange(points);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 学习进度
    private static async Task SeedLearningProgressAsync(AppDbContext context)
    {
        if (await context.MemberLearningProgress.AnyAsync()) return;

        var progresses = new List<MemberLearningProgress>
        {
            new() { Id = 1, MemberId = 2, ContentId = 1, TaskId = 1, DurationSeconds = 1800, IsCompleted = true, CompletedAt = new DateTime(2026,8,5,10,0,0), UpdatedAt = new DateTime(2026,8,5,10,0,0) },
            new() { Id = 2, MemberId = 2, ContentId = 4, TaskId = 1, DurationSeconds = 900, IsCompleted = false, UpdatedAt = new DateTime(2026,8,10,14,0,0) },
            new() { Id = 3, MemberId = 3, ContentId = 1, TaskId = 1, DurationSeconds = 2400, IsCompleted = true, CompletedAt = new DateTime(2026,8,3,9,0,0), UpdatedAt = new DateTime(2026,8,3,9,0,0) },
            new() { Id = 4, MemberId = 3, ContentId = 2, TaskId = 2, DurationSeconds = 1200, IsCompleted = true, CompletedAt = new DateTime(2026,7,20,11,0,0), UpdatedAt = new DateTime(2026,7,20,11,0,0) },
            new() { Id = 5, MemberId = 4, ContentId = 2, TaskId = 2, DurationSeconds = 600, IsCompleted = false, UpdatedAt = new DateTime(2026,7,15,10,0,0) },
            new() { Id = 6, MemberId = 5, ContentId = 1, TaskId = 1, DurationSeconds = 3000, IsCompleted = true, CompletedAt = new DateTime(2026,8,8,15,0,0), UpdatedAt = new DateTime(2026,8,8,15,0,0) },
            new() { Id = 7, MemberId = 6, ContentId = 3, DurationSeconds = 450, IsCompleted = false, UpdatedAt = new DateTime(2026,8,12,9,0,0) },
            new() { Id = 8, MemberId = 7, ContentId = 5, DurationSeconds = 1500, IsCompleted = true, CompletedAt = new DateTime(2026,8,6,14,0,0), UpdatedAt = new DateTime(2026,8,6,14,0,0) }
        };
        context.MemberLearningProgress.AddRange(progresses);
        await context.SaveChangesAsync();
    }
    #endregion

    #region 考试记录
    private static async Task SeedTestRecordsAsync(AppDbContext context)
    {
        if (await context.MemberTestRecords.AnyAsync()) return;

        var records = new List<MemberTestRecord>
        {
            new() { Id = 1, MemberId = 2, TestId = 1, Answers = "[{\"questionId\":1,\"answer\":\"1\"},{\"questionId\":2,\"answer\":\"2\"},{\"questionId\":3,\"answer\":\"0\"},{\"questionId\":4,\"answer\":\"0\"},{\"questionId\":9,\"answer\":\"0\"},{\"questionId\":11,\"answer\":\"[0,1,2]\"},{\"questionId\":17,\"answer\":\"2\"}]", Score = 35, SubmittedAt = new DateTime(2026,8,15,10,30,0) },
            new() { Id = 2, MemberId = 3, TestId = 1, Answers = "[{\"questionId\":1,\"answer\":\"1\"},{\"questionId\":2,\"answer\":\"2\"},{\"questionId\":3,\"answer\":\"0\"},{\"questionId\":4,\"answer\":\"0\"},{\"questionId\":9,\"answer\":\"0\"},{\"questionId\":11,\"answer\":\"[0,1,2,3]\"},{\"questionId\":17,\"answer\":\"2\"}]", Score = 45, SubmittedAt = new DateTime(2026,8,16,14,0,0) },
            new() { Id = 3, MemberId = 4, TestId = 1, Answers = "[{\"questionId\":1,\"answer\":\"0\"},{\"questionId\":2,\"answer\":\"2\"},{\"questionId\":3,\"answer\":\"1\"},{\"questionId\":4,\"answer\":\"0\"},{\"questionId\":9,\"answer\":\"1\"},{\"questionId\":11,\"answer\":\"[0,1]\"},{\"questionId\":17,\"answer\":\"3\"}]", Score = 15, SubmittedAt = new DateTime(2026,8,18,9,0,0) },
            new() { Id = 4, MemberId = 5, TestId = 2, Answers = "[{\"questionId\":1,\"answer\":\"1\"},{\"questionId\":2,\"answer\":\"2\"},{\"questionId\":10,\"answer\":\"0\"},{\"questionId\":16,\"answer\":\"2\"},{\"questionId\":5,\"answer\":\"3\"},{\"questionId\":6,\"answer\":\"1\"},{\"questionId\":7,\"answer\":\"3\"},{\"questionId\":13,\"answer\":\"3\"},{\"questionId\":15,\"answer\":\"0\"},{\"questionId\":18,\"answer\":\"[0,1,2]\"},{\"questionId\":20,\"answer\":\"1\"}]", Score = 55, SubmittedAt = new DateTime(2026,8,20,11,0,0) },
            new() { Id = 5, MemberId = 6, TestId = 2, Answers = "[{\"questionId\":1,\"answer\":\"1\"},{\"questionId\":2,\"answer\":\"1\"},{\"questionId\":10,\"answer\":\"0\"},{\"questionId\":16,\"answer\":\"1\"},{\"questionId\":5,\"answer\":\"3\"},{\"questionId\":6,\"answer\":\"1\"},{\"questionId\":7,\"answer\":\"0\"},{\"questionId\":13,\"answer\":\"0\"},{\"questionId\":15,\"answer\":\"0\"},{\"questionId\":18,\"answer\":\"[0,1]\"},{\"questionId\":20,\"answer\":\"0\"}]", Score = 30, SubmittedAt = new DateTime(2026,8,22,15,0,0) }
        };
        context.MemberTestRecords.AddRange(records);
        await context.SaveChangesAsync();
    }
    #endregion
}
