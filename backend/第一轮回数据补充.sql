-- ============================================
-- 第一轮回数据补充脚本
-- 丰富学习内容、考试记录、打卡记录、积分记录
-- ============================================

-- 1. 补充学习内容（15篇）
INSERT INTO learningcontents (Title, Body, VideoUrl, ContentType, CategoryId, IsPublic, CreatedAt)
SELECT * FROM (
  SELECT '习近平新时代中国特色社会主义思想学习纲要' as Title, 
    '习近平新时代中国特色社会主义思想是当代中国马克思主义、二十一世纪马克思主义，是中华文化和中国精神的时代精华，实现了马克思主义中国化新的飞跃。本纲要系统阐述了这一思想的时代背景、核心要义、精神实质、丰富内涵和实践要求。' as Body,
    NULL as VideoUrl, 0 as ContentType, 1 as CategoryId, 1 as IsPublic, NOW() as CreatedAt
  UNION ALL SELECT '中国共产党章程（2022年修订）全文解读', 
    '中国共产党章程是党的总章程，对坚持党的全面领导、推进全面从严治党、加强党的建设具有根本性的规范和指导作用。2022年党的二十大通过的党章修正案，充分体现了马克思主义中国化时代化最新成果，充分体现了党的十九大以来党中央提出的治国理政新理念新思想新战略。',
    NULL, 0, 1, 1, NOW(), NOW()
  UNION ALL SELECT '党的二十大报告全文学习辅导', 
    '党的二十大是在全党全国各族人民迈上全面建设社会主义现代化国家新征程、向第二个百年奋斗目标进军的关键时刻召开的一次十分重要的大会。报告系统总结了过去五年的工作和新时代十年的伟大变革，科学谋划了未来一个时期党和国家事业发展的目标任务和大政方针。',
    NULL, 0, 2, 1, NOW(), NOW()
  UNION ALL SELECT '党史故事100讲·建党伟业', 
    '1921年7月，中国共产党第一次全国代表大会在上海召开，后转移到浙江嘉兴南湖的游船上继续举行。大会通过了中国共产党的第一个纲领和决议，正式宣告中国共产党庄严诞生。从此，中国革命的面貌焕然一新。',
    'https://example.com/video/jiandang.mp4', 1, 3, 1, NOW(), NOW()
  UNION ALL SELECT '中国共产党简史·新民主主义革命时期', 
    '新民主主义革命时期，党面临的主要任务是，反对帝国主义、封建主义、官僚资本主义，争取民族独立、人民解放，为实现中华民族伟大复兴创造根本社会条件。党领导人民浴血奋战、百折不挠，创造了新民主主义革命的伟大成就。',
    NULL, 0, 3, 1, NOW(), NOW()
  UNION ALL SELECT '社会主义革命和建设时期党史', 
    '社会主义革命和建设时期，党面临的主要任务是，实现从新民主主义到社会主义的转变，进行社会主义革命，推进社会主义建设，为实现中华民族伟大复兴奠定根本政治前提和制度基础。党领导人民自力更生、发愤图强，创造了社会主义革命和建设的伟大成就。',
    NULL, 0, 3, 1, NOW(), NOW()
  UNION ALL SELECT '改革开放和社会主义现代化建设新时期', 
    '改革开放和社会主义现代化建设新时期，党面临的主要任务是，继续探索中国建设社会主义的正确道路，解放和发展社会生产力，使人民摆脱贫困、尽快富裕起来，为实现中华民族伟大复兴提供充满新的活力的体制保证和快速发展的物质条件。',
    NULL, 0, 3, 1, NOW(), NOW()
  UNION ALL SELECT '中国特色社会主义新时代', 
    '党的十八大以来，中国特色社会主义进入新时代。党面临的主要任务是，实现第一个百年奋斗目标，开启实现第二个百年奋斗目标新征程，朝着实现中华民族伟大复兴的宏伟目标继续前进。党领导人民自信自强、守正创新，创造了新时代中国特色社会主义的伟大成就。',
    NULL, 0, 3, 1, NOW(), NOW()
  UNION ALL SELECT '关于新形势下党内政治生活的若干准则', 
    '办好中国的事情，关键在党，关键在党要管党、从严治党。党要管党必须从党内政治生活管起，从严治党必须从党内政治生活严起。开展严肃认真的党内政治生活，是我们党的优良传统和政治优势。',
    NULL, 0, 4, 1, NOW(), NOW()
  UNION ALL SELECT '中国共产党廉洁自律准则', 
    '中国共产党全体党员和各级党员领导干部必须坚定共产主义理想和中国特色社会主义信念，必须坚持全心全意为人民服务根本宗旨，必须继承发扬党的优良传统和作风，必须自觉培养高尚道德情操，努力弘扬中华民族传统美德，廉洁自律，接受监督，永葆党的先进性和纯洁性。',
    NULL, 0, 4, 1, NOW(), NOW()
  UNION ALL SELECT '中国共产党纪律处分条例解读', 
    '新修订的《中国共产党纪律处分条例》全面贯彻习近平新时代中国特色社会主义思想和党的二十大精神，以党章为根本遵循，将党的纪律建设的理论、实践和制度创新成果，以党规党纪形式固定下来，着力提高纪律建设的政治性、时代性、针对性。',
    NULL, 0, 4, 1, NOW(), NOW()
  UNION ALL SELECT '习近平关于全面从严治党论述摘编', 
    '全面从严治党是新时代党的建设的鲜明主题。党的十八大以来，以习近平同志为核心的党中央把全面从严治党纳入"四个全面"战略布局，以前所未有的勇气和定力推进党风廉政建设和反腐败斗争，刹住了一些多年未刹住的歪风邪气，解决了许多长期没有解决的顽瘴痼疾。',
    NULL, 0, 4, 1, NOW(), NOW()
  UNION ALL SELECT '习近平谈治国理政（第四卷）专题学习', 
    '《习近平谈治国理政》第四卷收入了习近平总书记在2020年2月3日至2022年5月10日期间的讲话、谈话、演讲、致辞、指示、贺信等109篇，分为21个专题。这部著作生动记录了以习近平同志为核心的党中央，面对百年变局和世纪疫情相互叠加、世界进入新的动荡变革期的复杂局面。',
    NULL, 0, 2, 1, NOW(), NOW()
  UNION ALL SELECT '习近平总书记关于党的建设的重要思想', 
    '习近平总书记关于党的建设的重要思想，是习近平新时代中国特色社会主义思想的重要组成部分，是新时代党的建设理论发展和实践经验的科学总结，是马克思主义建党学说中国化时代化的最新成果，是全面推进新时代党的建设新的伟大工程的根本遵循和行动指南。',
    NULL, 0, 1, 1, NOW(), NOW()
  UNION ALL SELECT '中国式现代化理论专题学习', 
    '中国式现代化，是中国共产党领导的社会主义现代化，既有各国现代化的共同特征，更有基于自己国情的中国特色。中国式现代化是人口规模巨大的现代化，是全体人民共同富裕的现代化，是物质文明和精神文明相协调的现代化，是人与自然和谐共生的现代化，是走和平发展道路的现代化。',
    NULL, 0, 2, 1, NOW(), NOW()
) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM learningcontents lc WHERE lc.Title = tmp.Title);

-- 2. 补充打卡记录（为党员1-10补充最近30天打卡）
INSERT INTO checkinrecords (MemberId, CheckInDate, CreatedAt)
SELECT m.Id, DATE_SUB(CURDATE(), INTERVAL n.n DAY), NOW()
FROM partymembers m
CROSS JOIN (
  SELECT 0 as n UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4
  UNION SELECT 5 UNION SELECT 6 UNION SELECT 7 UNION SELECT 10 UNION SELECT 12
  UNION SELECT 14 UNION SELECT 15 UNION SELECT 18 UNION SELECT 20 UNION SELECT 21
  UNION SELECT 23 UNION SELECT 25 UNION SELECT 27 UNION SELECT 28 UNION SELECT 29
) n
WHERE m.Id BETWEEN 2 AND 10
AND NOT EXISTS (
  SELECT 1 FROM checkinrecords cir 
  WHERE cir.MemberId = m.Id AND DATE(cir.CheckInDate) = DATE_SUB(CURDATE(), INTERVAL n.n DAY)
);

-- 3. 补充积分记录（观看视频、完成测验等）
INSERT INTO learningpoints (MemberId, Points, SourceType, SourceId, Description, CreatedAt)
SELECT m.Id, 10, 0, FLOOR(RAND()*10)+1, '观看学习视频获得积分', 
  DATE_SUB(CURDATE(), INTERVAL FLOOR(RAND()*30) DAY)
FROM partymembers m
WHERE m.Id BETWEEN 2 AND 15
AND NOT EXISTS (SELECT 1 FROM learningpoints lp WHERE lp.MemberId = m.Id AND lp.Description = '观看学习视频获得积分' AND lp.Points = 10 LIMIT 1)
LIMIT 30;

INSERT INTO learningpoints (MemberId, Points, SourceType, SourceId, Description, CreatedAt)
SELECT m.Id, 20, 1, FLOOR(RAND()*5)+1, '完成测验获得积分', 
  DATE_SUB(CURDATE(), INTERVAL FLOOR(RAND()*30) DAY)
FROM partymembers m
WHERE m.Id BETWEEN 2 AND 12
AND NOT EXISTS (SELECT 1 FROM learningpoints lp WHERE lp.MemberId = m.Id AND lp.Description = '完成测验获得积分' AND lp.Points = 20 LIMIT 1)
LIMIT 20;

INSERT INTO learningpoints (MemberId, Points, SourceType, SourceId, Description, CreatedAt)
SELECT m.Id, 5, 2, NULL, '每日学习打卡获得积分', 
  DATE_SUB(CURDATE(), INTERVAL FLOOR(RAND()*30) DAY)
FROM partymembers m
WHERE m.Id BETWEEN 2 AND 20
AND NOT EXISTS (SELECT 1 FROM learningpoints lp WHERE lp.MemberId = m.Id AND lp.Description = '每日学习打卡获得积分' AND lp.Points = 5 LIMIT 1)
LIMIT 40;

-- 4. 同步更新党员总积分
UPDATE partymembers pm
SET pm.PointTotal = (
  SELECT COALESCE(SUM(lp.Points), 0) 
  FROM learningpoints lp 
  WHERE lp.MemberId = pm.Id
)
WHERE pm.Id > 0;

-- 5. 补充学习进度记录
INSERT INTO member_learning_progress (member_id, content_id, task_id, duration_seconds, is_completed, completed_at, updated_at)
SELECT m.Id, c.Id, NULL, FLOOR(RAND()*600)+60, 1, 
  DATE_SUB(NOW(), INTERVAL FLOOR(RAND()*30) DAY),
  DATE_SUB(NOW(), INTERVAL FLOOR(RAND()*30) DAY)
FROM partymembers m
CROSS JOIN learningcontents c
WHERE m.Id BETWEEN 2 AND 10 AND c.Id BETWEEN 1 AND 15
AND NOT EXISTS (
  SELECT 1 FROM member_learning_progress mlp 
  WHERE mlp.member_id = m.Id AND mlp.content_id = c.Id
)
LIMIT 50;

-- 完成提示
SELECT '第一轮回数据补充完成！' as result;
SELECT COUNT(*) as total_contents FROM learningcontents;
SELECT COUNT(*) as total_checkins FROM checkinrecords;
SELECT COUNT(*) as total_points FROM learningpoints;
SELECT COUNT(*) as total_progress FROM member_learning_progress;

