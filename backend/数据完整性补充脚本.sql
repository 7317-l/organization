-- ============================================================
-- 党员学习平台 - 数据完整性补充 SQL 脚本
-- 数据库: MySQL (party_school)
-- 说明: 针对前端功能所需但数据不足的表进行补充
-- 方式: 使用 INSERT ... SELECT ... WHERE NOT EXISTS 避免重复
-- 现有基础数据: 组织7个、党员22人、内容10篇、题库20题、
--               试卷2份、测验2个、学习进度8条、考试记录5条
-- ============================================================

USE party_school;

-- ============================================================
-- 1. member_test_records（考试记录表）
-- 对应前端功能: 考试中心-历史试卷、错题本（通过answers与correct_answer对比提取错题）
-- 现有数据: 5条（ID 1-5），可能全部为正确答案，无法展示错题
-- 补充: 新增5条包含错误答案的记录，降低得分，确保错题本有数据可提取
-- 列名: id, member_id, test_id, answers(json), score, submitted_at
-- ============================================================

-- 记录6: 党员3参加测验1，部分答错（得分60，2题错误）
INSERT INTO member_test_records (id, member_id, test_id, answers, score, submitted_at)
SELECT 6, 3, 1, '{"1":"1","2":"0","3":"1","4":"0","5":"1"}', 60, '2026-08-10 14:30:00'
WHERE NOT EXISTS (SELECT 1 FROM member_test_records WHERE id = 6);

-- 记录7: 党员5参加测验1，多题答错（得分40，3题错误）
INSERT INTO member_test_records (id, member_id, test_id, answers, score, submitted_at)
SELECT 7, 5, 1, '{"1":"2","2":"1","3":"0","4":"1","5":"0"}', 40, '2026-08-12 09:15:00'
WHERE NOT EXISTS (SELECT 1 FROM member_test_records WHERE id = 7);

-- 记录8: 党员7参加测验2，部分答错（得分70，含多选错误）
INSERT INTO member_test_records (id, member_id, test_id, answers, score, submitted_at)
SELECT 8, 7, 2, '{"6":"0","7":"[0,1]","8":"1","9":"0","10":"[0,2]"}', 70, '2026-08-15 16:00:00'
WHERE NOT EXISTS (SELECT 1 FROM member_test_records WHERE id = 8);

-- 记录9: 党员9参加测验2，多题答错（得分30，判断题错误）
INSERT INTO member_test_records (id, member_id, test_id, answers, score, submitted_at)
SELECT 9, 9, 2, '{"6":"1","7":"[0]","8":"false","9":"1","10":"[1,2]"}', 30, '2026-08-18 11:45:00'
WHERE NOT EXISTS (SELECT 1 FROM member_test_records WHERE id = 9);

-- 记录10: 党员11参加测验1，1题错误（得分80）
INSERT INTO member_test_records (id, member_id, test_id, answers, score, submitted_at)
SELECT 10, 11, 1, '{"1":"0","2":"1","3":"0","4":"2","5":"0"}', 80, '2026-08-20 10:00:00'
WHERE NOT EXISTS (SELECT 1 FROM member_test_records WHERE id = 10);


-- ============================================================
-- 2. memberlearningreports（党员学习报告表）
-- 对应前端功能: 我的-完整AI学习报告（综合评分、维度雷达图、AI评语、改进建议）
-- 现有数据: 可能为空
-- 补充: 为5名党员生成报告JSON，包含overallScore、level、dimensions、comment、suggestions
-- 列名: Id, PartyMemberId, ReportJson, CreatedAt
-- ============================================================

INSERT INTO memberlearningreports (Id, PartyMemberId, ReportJson, CreatedAt)
SELECT 1, 1,
'{"overallScore":85,"level":"良好","dimensions":[{"name":"学习频率","score":90,"level":"优秀"},{"name":"测验得分","score":75,"level":"良好"},{"name":"完成率","score":82,"level":"良好"},{"name":"互动参与","score":60,"level":"一般"},{"name":"知识掌握","score":72,"level":"良好"}],"comment":"你整体学习表现良好，学习完成率和测验得分表现突出，展现出较强的学习能力和积极性。在学习频率和互动参与方面仍有提升空间。","suggestions":["建议养成每日学习习惯，保持稳定的学习频率。","积极参与讨论与答题活动，提升互动参与度。","针对薄弱知识点加强复习，定期进行测验巩固。"]}',
'2026-08-15 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM memberlearningreports WHERE Id = 1);

INSERT INTO memberlearningreports (Id, PartyMemberId, ReportJson, CreatedAt)
SELECT 2, 3,
'{"overallScore":72,"level":"中等","dimensions":[{"name":"学习频率","score":65,"level":"一般"},{"name":"测验得分","score":60,"level":"一般"},{"name":"完成率","score":70,"level":"良好"},{"name":"互动参与","score":55,"level":"一般"},{"name":"知识掌握","score":68,"level":"一般"}],"comment":"你的学习处于中等水平，测验得分和知识掌握方面有待加强。建议增加学习时长，重点攻克党史和党章等基础知识点。","suggestions":["每周至少安排3次集中学习，每次不少于30分钟。","针对错题本中的高频错题进行专项练习。","多参与支部讨论活动，通过交流加深理解。"]}',
'2026-08-16 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM memberlearningreports WHERE Id = 2);

INSERT INTO memberlearningreports (Id, PartyMemberId, ReportJson, CreatedAt)
SELECT 3, 5,
'{"overallScore":58,"level":"待提升","dimensions":[{"name":"学习频率","score":45,"level":"待提升"},{"name":"测验得分","score":40,"level":"待提升"},{"name":"完成率","score":55,"level":"一般"},{"name":"互动参与","score":35,"level":"待提升"},{"name":"知识掌握","score":50,"level":"待提升"}],"comment":"你的学习表现有待提升，学习频率较低且测验成绩不理想。建议制定学习计划，从基础内容开始系统学习，积极参加支部组织的各项学习活动。","suggestions":["制定每日学习计划，至少完成1篇文章或1个视频的学习。","优先学习党章和党史基础内容，打好理论基础。","主动向支部书记或老党员请教，积极参与互动。"]}',
'2026-08-17 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM memberlearningreports WHERE Id = 3);

INSERT INTO memberlearningreports (Id, PartyMemberId, ReportJson, CreatedAt)
SELECT 4, 7,
'{"overallScore":92,"level":"优秀","dimensions":[{"name":"学习频率","score":95,"level":"优秀"},{"name":"测验得分","score":88,"level":"优秀"},{"name":"完成率","score":95,"level":"优秀"},{"name":"互动参与","score":85,"level":"优秀"},{"name":"知识掌握","score":90,"level":"优秀"}],"comment":"你的学习表现非常优秀！在各维度均表现突出，是支部党员学习的榜样。建议继续保持学习热情，同时可以帮助其他党员共同进步。","suggestions":["继续保持当前的学习节奏和频率。","可以尝试分享学习心得，帮助其他党员提升。","挑战更高难度的学习内容和测验。"]}',
'2026-08-18 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM memberlearningreports WHERE Id = 4);

INSERT INTO memberlearningreports (Id, PartyMemberId, ReportJson, CreatedAt)
SELECT 5, 9,
'{"overallScore":78,"level":"良好","dimensions":[{"name":"学习频率","score":80,"level":"良好"},{"name":"测验得分","score":70,"level":"良好"},{"name":"完成率","score":75,"level":"良好"},{"name":"互动参与","score":65,"level":"一般"},{"name":"知识掌握","score":78,"level":"良好"}],"comment":"你的学习表现良好，学习频率稳定，知识掌握扎实。在互动参与和测验得分方面还有提升空间，建议多参与答题和讨论活动。","suggestions":["增加答题练习频率，提升测验正确率。","积极参与支部讨论和AI问答互动。","定期回顾错题，巩固薄弱知识点。"]}',
'2026-08-19 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM memberlearningreports WHERE Id = 5);


-- ============================================================
-- 3. learningpoints（积分记录表）
-- 对应前端功能: 我的-积分明细、首页-总积分统计卡片、积分排行榜
-- 现有数据: 可能不足
-- 补充: 15条积分记录，覆盖观看视频、完成答题、活动打卡、PK胜利等来源
-- 列名: Id, PartyMemberId, SourceType(int), SourceId, Points, EarnedAt
-- SourceType枚举: WatchVideo=0, CompleteExam=1, ActivityCheckIn=2, BattleVictory=3, Other=4
-- ============================================================

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 1, 1, 0, 1, 10, '2026-08-01 09:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 1);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 2, 1, 1, 1, 50, '2026-08-05 14:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 2);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 3, 1, 2, NULL, 5, '2026-08-10 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 3);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 4, 3, 0, 2, 10, '2026-08-02 11:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 4);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 5, 3, 1, 1, 30, '2026-08-10 14:30:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 5);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 6, 5, 0, 3, 10, '2026-08-03 15:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 6);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 7, 5, 1, 1, 20, '2026-08-12 09:15:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 7);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 8, 7, 0, 1, 10, '2026-08-01 08:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 8);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 9, 7, 0, 4, 10, '2026-08-04 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 9);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 10, 7, 1, 2, 35, '2026-08-15 16:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 10);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 11, 7, 3, 1, 20, '2026-08-18 19:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 11);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 12, 9, 0, 5, 10, '2026-08-06 13:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 12);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 13, 9, 1, 2, 15, '2026-08-18 11:45:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 13);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 14, 11, 0, 2, 10, '2026-08-08 16:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 14);

INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 15, 11, 1, 1, 40, '2026-08-20 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 15);


-- ============================================================
-- 4. checkinrecords（打卡记录表）
-- 对应前端功能: 我的-我的打卡记录、红色教育基地打卡
-- 现有数据: 可能为空
-- 补充: 5条打卡记录，覆盖不同红色教育基地
-- 列名: Id, PartyMemberId, LocationName, CheckInTime, Note, AiBackgroundInterpretation, PointsEarned
-- ============================================================

INSERT INTO checkinrecords (Id, PartyMemberId, LocationName, CheckInTime, Note, AiBackgroundInterpretation, PointsEarned)
SELECT 1, 1, '嘉兴南湖革命纪念馆', '2026-07-01 09:30:00', '参观中共一大会址，重温建党初心。', '嘉兴南湖是中国共产党的诞生地，1921年中共一大在南湖红船上胜利闭幕，宣告了中国共产党的正式成立。', 5
WHERE NOT EXISTS (SELECT 1 FROM checkinrecords WHERE Id = 1);

INSERT INTO checkinrecords (Id, PartyMemberId, LocationName, CheckInTime, Note, AiBackgroundInterpretation, PointsEarned)
SELECT 2, 3, '井冈山革命博物馆', '2026-07-15 14:00:00', '学习井冈山精神，坚定理想信念。', '井冈山是中国第一个农村革命根据地，被誉为"中国革命的摇篮"，井冈山精神是中国共产党人革命精神的重要组成部分。', 5
WHERE NOT EXISTS (SELECT 1 FROM checkinrecords WHERE Id = 2);

INSERT INTO checkinrecords (Id, PartyMemberId, LocationName, CheckInTime, Note, AiBackgroundInterpretation, PointsEarned)
SELECT 3, 7, '延安革命纪念馆', '2026-08-01 10:00:00', '追寻延安精神，感悟初心使命。', '延安是中国革命的圣地，党中央在延安战斗生活了13年，培育了伟大的延安精神，是中国共产党的宝贵精神财富。', 5
WHERE NOT EXISTS (SELECT 1 FROM checkinrecords WHERE Id = 3);

INSERT INTO checkinrecords (Id, PartyMemberId, LocationName, CheckInTime, Note, AiBackgroundInterpretation, PointsEarned)
SELECT 4, 9, '遵义会议会址', '2026-08-10 15:30:00', '学习遵义会议历史，理解党的伟大转折。', '遵义会议是中国共产党历史上一个生死攸关的转折点，确立了毛泽东同志在党中央和红军的领导地位，标志着中国共产党从幼年走向成熟。', 5
WHERE NOT EXISTS (SELECT 1 FROM checkinrecords WHERE Id = 4);

INSERT INTO checkinrecords (Id, PartyMemberId, LocationName, CheckInTime, Note, AiBackgroundInterpretation, PointsEarned)
SELECT 5, 5, '西柏坡纪念馆', '2026-08-15 11:00:00', '学习赶考精神，牢记两个务必。', '西柏坡是党中央进入北平、解放全中国的最后一个农村指挥所，党中央在此召开了七届二中全会，毛泽东同志提出了"两个务必"的重要论述。', 5
WHERE NOT EXISTS (SELECT 1 FROM checkinrecords WHERE Id = 5);


-- ============================================================
-- 5. messagenotifications（消息通知表）
-- 对应前端功能: 我的-消息通知（未读通知角标）、首页待办提醒
-- 现有数据: 可能为空
-- 补充: 8条通知，包含任务提醒、测验提醒、预警提醒、系统通知，部分未读
-- 列名: Id, PartyMemberId, Type(int), Title, Content, IsRead, CreatedAt
-- Type枚举: TaskReminder=0, ExamReminder=1, WarningReminder=2, SystemNotice=3
-- ============================================================

INSERT INTO messagenotifications (Id, PartyMemberId, Type, Title, Content, IsRead, CreatedAt)
SELECT 1, 1, 0, '学习任务提醒', '您有一项新的学习任务"8月主题党日学习"待完成，截止时间为2026-08-25。', 0, '2026-08-20 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM messagenotifications WHERE Id = 1);

INSERT INTO messagenotifications (Id, PartyMemberId, Type, Title, Content, IsRead, CreatedAt)
SELECT 2, 1, 1, '测验提醒', '新测验"党章知识竞赛"已发布，请在截止时间前完成答题。', 0, '2026-08-21 09:00:00'
WHERE NOT EXISTS (SELECT 1 FROM messagenotifications WHERE Id = 2);

INSERT INTO messagenotifications (Id, PartyMemberId, Type, Title, Content, IsRead, CreatedAt)
SELECT 3, 3, 0, '学习任务提醒', '您的学习任务"7月党课学习"即将到期，请尽快完成。', 0, '2026-08-18 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM messagenotifications WHERE Id = 3);

INSERT INTO messagenotifications (Id, PartyMemberId, Type, Title, Content, IsRead, CreatedAt)
SELECT 4, 3, 2, '学习预警', '您本周学习时长不足2小时，建议合理安排时间，保持学习连贯性。', 0, '2026-08-19 18:00:00'
WHERE NOT EXISTS (SELECT 1 FROM messagenotifications WHERE Id = 4);

INSERT INTO messagenotifications (Id, PartyMemberId, Type, Title, Content, IsRead, CreatedAt)
SELECT 5, 5, 1, '测验提醒', '您有一项测验"党史知识测验"待参加，截止时间为2026-08-30。', 0, '2026-08-22 10:00:00'
WHERE NOT EXISTS (SELECT 1 FROM messagenotifications WHERE Id = 5);

INSERT INTO messagenotifications (Id, PartyMemberId, Type, Title, Content, IsRead, CreatedAt)
SELECT 6, 7, 3, '系统通知', '恭喜您获得"学习标兵"称号，继续保持！', 1, '2026-08-15 12:00:00'
WHERE NOT EXISTS (SELECT 1 FROM messagenotifications WHERE Id = 6);

INSERT INTO messagenotifications (Id, PartyMemberId, Type, Title, Content, IsRead, CreatedAt)
SELECT 7, 9, 2, '学习预警', '您已连续7天未登录学习平台，建议尽快恢复学习。', 0, '2026-08-20 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM messagenotifications WHERE Id = 7);

INSERT INTO messagenotifications (Id, PartyMemberId, Type, Title, Content, IsRead, CreatedAt)
SELECT 8, 11, 0, '学习任务提醒', '支部发布了新的学习任务"二十大精神专题学习"，请及时查看。', 1, '2026-08-19 14:00:00'
WHERE NOT EXISTS (SELECT 1 FROM messagenotifications WHERE Id = 8);


-- ============================================================
-- 6. partydevelopmentprocesses（党员发展流程表）
-- 对应前端功能: 管理后台-党员发展流程管理、我的-发展进度
-- 现有数据: 可能为空
-- 补充: 5条发展流程记录，覆盖积极分子、发展对象、预备党员等不同阶段
-- 列名: Id, PartyMemberId, Stage(int), Status(int), MaterialsJson, ReportContent,
--       SubmittedAt, ReviewComment, ReviewedAt, IsReminderSent, CreatedAt
-- Stage枚举: Activist=0, DevelopmentTarget=1, ProbationaryMember=2, FullMember=3
-- Status枚举: PendingSubmit=0, UnderReview=1, Approved=2, Rejected=3
-- ============================================================

INSERT INTO partydevelopmentprocesses (Id, PartyMemberId, Stage, Status, MaterialsJson, ReportContent, SubmittedAt, ReviewComment, ReviewedAt, IsReminderSent, CreatedAt)
SELECT 1, 15, 0, 2,
'{"入党申请书":"已提交","思想汇报":["2026-Q1","2026-Q2"],"培养联系人":"李书记"}',
'本人自提交入党申请书以来，认真学习党的理论知识，积极参加支部各项活动，在思想上和行动上始终与党中央保持高度一致。',
'2026-03-15 10:00:00', '该同志思想觉悟高，学习认真，表现良好，同意确定为积极分子。', '2026-03-20 14:00:00', 0, '2026-03-10 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM partydevelopmentprocesses WHERE Id = 1);

INSERT INTO partydevelopmentprocesses (Id, PartyMemberId, Stage, Status, MaterialsJson, ReportContent, SubmittedAt, ReviewComment, ReviewedAt, IsReminderSent, CreatedAt)
SELECT 2, 16, 1, 1,
'{"政审材料":"已完成","入党志愿书":"待填写","短期集中培训":"已完成"}',
'经过一年的积极分子培养，我在思想认识、理论学习和工作实践等方面都取得了较大进步，现申请列为发展对象。',
'2026-06-10 09:00:00', NULL, NULL, 0, '2026-06-01 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM partydevelopmentprocesses WHERE Id = 2);

INSERT INTO partydevelopmentprocesses (Id, PartyMemberId, Stage, Status, MaterialsJson, ReportContent, SubmittedAt, ReviewComment, ReviewedAt, IsReminderSent, CreatedAt)
SELECT 3, 17, 2, 2,
'{"入党宣誓":"2026-07-01","预备期":"2026-07-01至2027-06-30","入党介绍人":"王书记"}',
'作为一名预备党员，我时刻以党员标准严格要求自己，认真学习党的二十大精神，积极参加组织生活，按期交纳党费。',
'2026-07-01 08:00:00', '该同志在预备期内表现优秀，同意按期转正。', '2026-07-05 10:00:00', 1, '2026-06-25 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM partydevelopmentprocesses WHERE Id = 3);

INSERT INTO partydevelopmentprocesses (Id, PartyMemberId, Stage, Status, MaterialsJson, ReportContent, SubmittedAt, ReviewComment, ReviewedAt, IsReminderSent, CreatedAt)
SELECT 4, 18, 0, 0,
'{"入党申请书":"待提交","思想汇报":[],"培养联系人":""}',
NULL, NULL, NULL, NULL, 0, '2026-08-01 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM partydevelopmentprocesses WHERE Id = 4);

INSERT INTO partydevelopmentprocesses (Id, PartyMemberId, Stage, Status, MaterialsJson, ReportContent, SubmittedAt, ReviewComment, ReviewedAt, IsReminderSent, CreatedAt)
SELECT 5, 19, 1, 3,
'{"政审材料":"已完成","入党志愿书":"已提交","短期集中培训":"已完成"}',
'本人在发展对象考察期间，认真学习党章党规，积极参与志愿服务，但在理论联系实际方面仍需加强。',
'2026-05-20 14:00:00', '该同志政审材料齐全，但思想汇报不够深刻，建议补充后重新提交。', '2026-05-25 10:00:00', 0, '2026-05-10 08:00:00'
WHERE NOT EXISTS (SELECT 1 FROM partydevelopmentprocesses WHERE Id = 5);


-- ============================================================
-- 7. meetingactivities（三会一课活动表）
-- 对应前端功能: 管理后台-三会一课管理、活动心得提交、AI总结
-- 现有数据: 可能为空
-- 补充: 4个活动，覆盖支部党员大会、党课、主题党日等类型
-- 列名: Id, OrganizationId, Type(int), Title, Description, ActivityTime,
--       CreatedAt, IsAiSummaryGenerated, AiSummaryContent
-- Type枚举: BranchGeneralMeeting=0, BranchCommittee=1, PartyGroupMeeting=2, PartyLecture=3, ThemePartyDay=4
-- ============================================================

INSERT INTO meetingactivities (Id, OrganizationId, Type, Title, Description, ActivityTime, CreatedAt, IsAiSummaryGenerated, AiSummaryContent)
SELECT 1, 3, 0, '第三党支部8月党员大会',
'讨论通过本月支部工作计划，通报上半年党员学习情况，部署下半年重点工作任务。',
'2026-08-05 14:00:00', '2026-08-01 08:00:00', 1,
'本次党员大会主要围绕三个方面展开：一是通报上半年支部党员学习完成情况，整体完成率达85%；二是讨论通过8月主题党日活动方案；三是部署下半年重点工作，包括二十大精神专题学习、党员发展工作等。会议要求全体党员继续保持学习热情，充分发挥先锋模范作用。'
WHERE NOT EXISTS (SELECT 1 FROM meetingactivities WHERE Id = 1);

INSERT INTO meetingactivities (Id, OrganizationId, Type, Title, Description, ActivityTime, CreatedAt, IsAiSummaryGenerated, AiSummaryContent)
SELECT 2, 3, 3, '二十大精神专题党课',
'由支部书记主讲，深入解读党的二十大报告核心要义，重点阐述中国式现代化的本质要求和重大原则。',
'2026-08-12 09:00:00', '2026-08-08 08:00:00', 0, NULL
WHERE NOT EXISTS (SELECT 1 FROM meetingactivities WHERE Id = 2);

INSERT INTO meetingactivities (Id, OrganizationId, Type, Title, Description, ActivityTime, CreatedAt, IsAiSummaryGenerated, AiSummaryContent)
SELECT 3, 2, 4, '第二党支部"七一"主题党日活动',
'组织党员参观红色教育基地，重温入党誓词，开展"我为群众办实事"志愿服务活动。',
'2026-07-01 08:30:00', '2026-06-25 08:00:00', 1,
'本次主题党日活动以"不忘初心、牢记使命"为主题，组织全体党员前往红色教育基地参观学习。通过重温入党誓词、聆听革命故事、开展志愿服务等环节，党员们深受教育和鼓舞，纷纷表示要继承和发扬党的优良传统，在本职岗位上发挥先锋模范作用。'
WHERE NOT EXISTS (SELECT 1 FROM meetingactivities WHERE Id = 3);

INSERT INTO meetingactivities (Id, OrganizationId, Type, Title, Description, ActivityTime, CreatedAt, IsAiSummaryGenerated, AiSummaryContent)
SELECT 4, 1, 3, '党章学习专题党课',
'系统学习《中国共产党章程》总纲和党员章节，重点解读党员义务和权利，引导党员自觉遵守党章。',
'2026-08-20 14:00:00', '2026-08-15 08:00:00', 0, NULL
WHERE NOT EXISTS (SELECT 1 FROM meetingactivities WHERE Id = 4);


-- ============================================================
-- 8. activityhearts（活动心得表）
-- 对应前端功能: 三会一课-提交心得、查看心得列表、AI润色
-- 现有数据: 可能为空
-- 补充: 6条心得，关联已存在的活动和党员
-- 列名: Id, MeetingActivityId, PartyMemberId, Content, SubmittedAt, AiPolishSuggestion
-- ============================================================

INSERT INTO activityhearts (Id, MeetingActivityId, PartyMemberId, Content, SubmittedAt, AiPolishSuggestion)
SELECT 1, 1, 1,
'参加8月党员大会后，我深受鼓舞。上半年支部学习完成率达到85%，作为一名党员，我感到自豪的同时也认识到自己的不足。下半年我将更加严格要求自己，认真完成每一项学习任务，积极参与支部建设，在工作中发挥党员的先锋模范作用。',
'2026-08-06 10:00:00', '心得内容充实，情感真挚。建议增加具体的学习计划和行动措施，使心得更具指导性。'
WHERE NOT EXISTS (SELECT 1 FROM activityhearts WHERE Id = 1);

INSERT INTO activityhearts (Id, MeetingActivityId, PartyMemberId, Content, SubmittedAt, AiPolishSuggestion)
SELECT 2, 1, 3,
'党员大会让我明确了下半年的学习方向。特别是二十大精神专题学习的部署，我将认真制定学习计划，逐字逐句研读二十大报告，做到学深悟透、融会贯通。同时，我将积极参与讨论，分享学习心得，与同志们共同进步。',
'2026-08-07 14:30:00', NULL
WHERE NOT EXISTS (SELECT 1 FROM activityhearts WHERE Id = 2);

INSERT INTO activityhearts (Id, MeetingActivityId, PartyMemberId, Content, SubmittedAt, AiPolishSuggestion)
SELECT 3, 2, 7,
'二十大精神专题党课让我对中国式现代化有了更深刻的理解。中国式现代化是人口规模巨大的现代化，是全体人民共同富裕的现代化，是物质文明和精神文明相协调的现代化，是人与自然和谐共生的现代化，是走和平发展道路的现代化。作为基层党员，我要立足本职岗位，为推进中国式现代化贡献自己的力量。',
'2026-08-13 11:00:00', '理论阐述准确，结合实际较好。建议增加个人在工作中如何践行中国式现代化的具体案例。'
WHERE NOT EXISTS (SELECT 1 FROM activityhearts WHERE Id = 3);

INSERT INTO activityhearts (Id, MeetingActivityId, PartyMemberId, Content, SubmittedAt, AiPolishSuggestion)
SELECT 4, 3, 5,
'"七一"主题党日活动让我深受教育。在红色教育基地，我看到了革命先辈们为了民族独立和人民解放不惜牺牲一切的崇高精神。重温入党誓词的那一刻，我更加坚定了自己的理想信念。在今后的工作中，我将继承革命传统，不忘初心、牢记使命，以实际行动践行入党誓言。',
'2026-07-02 16:00:00', NULL
WHERE NOT EXISTS (SELECT 1 FROM activityhearts WHERE Id = 4);

INSERT INTO activityhearts (Id, MeetingActivityId, PartyMemberId, Content, SubmittedAt, AiPolishSuggestion)
SELECT 5, 3, 9,
'参加"七一"主题党日活动，我最大的感受是感动和责任。革命先辈们用鲜血和生命换来了我们今天的幸福生活，我们没有理由不珍惜。作为新时代的党员，我要把对党的忠诚转化为工作的动力，在平凡的岗位上做出不平凡的业绩，不辜负党组织的培养和期望。',
'2026-07-03 09:00:00', '情感到位，表达流畅。建议增加具体的工作目标和改进措施。'
WHERE NOT EXISTS (SELECT 1 FROM activityhearts WHERE Id = 5);

INSERT INTO activityhearts (Id, MeetingActivityId, PartyMemberId, Content, SubmittedAt, AiPolishSuggestion)
SELECT 6, 4, 11,
'党章学习专题党课让我重新认识了党章的重要性。党章是党的总章程，是全党必须共同遵守的根本行为规范。通过系统学习党员义务和权利，我深刻认识到，党员不仅是一种身份，更是一种责任和担当。我将自觉遵守党章，履行党员义务，正确行使党员权利，做一名合格的共产党员。',
'2026-08-21 10:30:00', NULL
WHERE NOT EXISTS (SELECT 1 FROM activityhearts WHERE Id = 6);


-- ============================================================
-- 9. pairhelprecords（结对帮扶记录表）
-- 对应前端功能: 管理后台-党员互助结对帮扶、帮扶成果展示
-- 现有数据: 可能为空
-- 补充: 2对帮扶关系，老党员帮扶新党员/积极分子
-- 列名: Id, HelperId, HelpReceiverId, StartTime, EndTime, HelpContentJson, OutcomeSummary
-- ============================================================

INSERT INTO pairhelprecords (Id, HelperId, HelpReceiverId, StartTime, EndTime, HelpContentJson, OutcomeSummary)
SELECT 1, 1, 15, '2026-03-01 00:00:00', NULL,
'{"帮扶方式":"一对一结对","学习内容":["党的基本理论","党章党规","二十大精神"],"活动次数":8,"共同学习时长":16}',
'通过半年的结对帮扶，被帮扶人在思想认识和理论水平方面有了明显提升，已顺利确定为入党积极分子，学习积极性显著提高。'
WHERE NOT EXISTS (SELECT 1 FROM pairhelprecords WHERE Id = 1);

INSERT INTO pairhelprecords (Id, HelperId, HelpReceiverId, StartTime, EndTime, HelpContentJson, OutcomeSummary)
SELECT 2, 7, 16, '2026-06-01 00:00:00', NULL,
'{"帮扶方式":"一对一结对","学习内容":["发展对象培训","入党志愿书填写","党史学习"],"活动次数":5,"共同学习时长":10}',
'帮扶关系正在进行中，被帮扶人已完成发展对象短期集中培训，正在准备入党志愿书的填写，整体进展顺利。'
WHERE NOT EXISTS (SELECT 1 FROM pairhelprecords WHERE Id = 2);


-- ============================================================
-- 10. battlerecords（PK对战记录表）
-- 对应前端功能: 党史PK对战、对战记录展示
-- 现有数据: 可能为空
-- 补充: 3场对战记录，包含对战双方得分和用时
-- 列名: Id, ChallengerId, OpponentId, ResultJson, BattleTime
-- ============================================================

INSERT INTO battlerecords (Id, ChallengerId, OpponentId, ResultJson, BattleTime)
SELECT 1, 1, 3,
'{"challengerScore":85,"opponentScore":70,"challengerTime":"2:30","opponentTime":"3:15","winner":"challenger","questionsCount":10}',
'2026-08-10 20:00:00'
WHERE NOT EXISTS (SELECT 1 FROM battlerecords WHERE Id = 1);

INSERT INTO battlerecords (Id, ChallengerId, OpponentId, ResultJson, BattleTime)
SELECT 2, 7, 9,
'{"challengerScore":95,"opponentScore":80,"challengerTime":"2:10","opponentTime":"2:45","winner":"challenger","questionsCount":10}',
'2026-08-15 19:30:00'
WHERE NOT EXISTS (SELECT 1 FROM battlerecords WHERE Id = 2);

INSERT INTO battlerecords (Id, ChallengerId, OpponentId, ResultJson, BattleTime)
SELECT 3, 5, 11,
'{"challengerScore":60,"opponentScore":75,"challengerTime":"3:00","opponentTime":"2:50","winner":"opponent","questionsCount":10}',
'2026-08-18 21:00:00'
WHERE NOT EXISTS (SELECT 1 FROM battlerecords WHERE Id = 3);


-- ============================================================
-- 脚本执行完成
-- 补充数据汇总:
--   member_test_records: +5条（含错误答案，用于错题本展示）
--   memberlearningreports: +5条（AI学习报告）
--   learningpoints: +15条（积分明细）
--   checkinrecords: +5条（打卡记录）
--   messagenotifications: +8条（消息通知，含5条未读）
--   partydevelopmentprocesses: +5条（党员发展流程）
--   meetingactivities: +4条（三会一课活动）
--   activityhearts: +6条（活动心得）
--   pairhelprecords: +2条（结对帮扶）
--   battlerecords: +3条（PK对战）
-- 总计: +58条数据
-- ============================================================
