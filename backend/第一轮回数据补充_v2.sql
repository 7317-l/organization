-- 第一轮回数据补充脚本（简化版）
-- 补充学习内容

INSERT IGNORE INTO learningcontents (Title, Body, VideoUrl, ContentType, CategoryId, IsPublic, CreatedAt) VALUES
('习近平新时代中国特色社会主义思想学习纲要', '习近平新时代中国特色社会主义思想是当代中国马克思主义、二十一世纪马克思主义，是中华文化和中国精神的时代精华，实现了马克思主义中国化新的飞跃。', NULL, 0, 1, 1, NOW()),
('中国共产党章程（2022年修订）全文解读', '中国共产党章程是党的总章程，对坚持党的全面领导、推进全面从严治党、加强党的建设具有根本性的规范和指导作用。', NULL, 0, 1, 1, NOW()),
('党的二十大报告全文学习辅导', '党的二十大是在全党全国各族人民迈上全面建设社会主义现代化国家新征程、向第二个百年奋斗目标进军的关键时刻召开的一次十分重要的大会。', NULL, 0, 2, 1, NOW()),
('党史故事100讲·建党伟业', '1921年7月，中国共产党第一次全国代表大会在上海召开，后转移到浙江嘉兴南湖的游船上继续举行。大会正式宣告中国共产党庄严诞生。', 'https://example.com/video/jiandang.mp4', 1, 3, 1, NOW()),
('中国共产党简史·新民主主义革命时期', '新民主主义革命时期，党面临的主要任务是，反对帝国主义、封建主义、官僚资本主义，争取民族独立、人民解放。', NULL, 0, 3, 1, NOW()),
('社会主义革命和建设时期党史', '社会主义革命和建设时期，党面临的主要任务是，实现从新民主主义到社会主义的转变，进行社会主义革命，推进社会主义建设。', NULL, 0, 3, 1, NOW()),
('改革开放和社会主义现代化建设新时期', '改革开放和社会主义现代化建设新时期，党面临的主要任务是，继续探索中国建设社会主义的正确道路，解放和发展社会生产力。', NULL, 0, 3, 1, NOW()),
('中国特色社会主义新时代', '党的十八大以来，中国特色社会主义进入新时代。党领导人民自信自强、守正创新，创造了新时代中国特色社会主义的伟大成就。', NULL, 0, 3, 1, NOW()),
('关于新形势下党内政治生活的若干准则', '办好中国的事情，关键在党，关键在党要管党、从严治党。党要管党必须从党内政治生活管起。', NULL, 0, 4, 1, NOW()),
('中国共产党廉洁自律准则', '中国共产党全体党员和各级党员领导干部必须坚定共产主义理想和中国特色社会主义信念，必须坚持全心全意为人民服务根本宗旨。', NULL, 0, 4, 1, NOW()),
('中国共产党纪律处分条例解读', '新修订的《中国共产党纪律处分条例》全面贯彻习近平新时代中国特色社会主义思想和党的二十大精神，以党章为根本遵循。', NULL, 0, 4, 1, NOW()),
('习近平关于全面从严治党论述摘编', '全面从严治党是新时代党的建设的鲜明主题。党的十八大以来，以习近平同志为核心的党中央把全面从严治党纳入"四个全面"战略布局。', NULL, 0, 4, 1, NOW()),
('习近平谈治国理政（第四卷）专题学习', '《习近平谈治国理政》第四卷收入了习近平总书记在2020年2月3日至2022年5月10日期间的讲话、谈话、演讲等109篇。', NULL, 0, 2, 1, NOW()),
('习近平总书记关于党的建设的重要思想', '习近平总书记关于党的建设的重要思想，是习近平新时代中国特色社会主义思想的重要组成部分，是新时代党的建设理论发展和实践经验的科学总结。', NULL, 0, 1, 1, NOW()),
('中国式现代化理论专题学习', '中国式现代化，是中国共产党领导的社会主义现代化，既有各国现代化的共同特征，更有基于自己国情的中国特色。', NULL, 0, 2, 1, NOW());

-- 补充打卡记录
INSERT IGNORE INTO checkinrecords (MemberId, CheckInDate, CreatedAt)
SELECT m.Id, DATE_SUB(CURDATE(), INTERVAL n.n DAY), NOW()
FROM partymembers m
CROSS JOIN (
  SELECT 0 as n UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 5
  UNION SELECT 7 UNION SELECT 10 UNION SELECT 12 UNION SELECT 15 UNION SELECT 18
  UNION SELECT 20 UNION SELECT 22 UNION SELECT 25 UNION SELECT 27 UNION SELECT 29
) n
WHERE m.Id BETWEEN 2 AND 10;

-- 补充积分记录
INSERT IGNORE INTO learningpoints (MemberId, Points, SourceType, Description, CreatedAt)
SELECT m.Id, 10, 0, '观看学习视频获得积分', DATE_SUB(CURDATE(), INTERVAL FLOOR(RAND()*30) DAY)
FROM partymembers m WHERE m.Id BETWEEN 2 AND 15 LIMIT 30;

INSERT IGNORE INTO learningpoints (MemberId, Points, SourceType, Description, CreatedAt)
SELECT m.Id, 20, 1, '完成测验获得积分', DATE_SUB(CURDATE(), INTERVAL FLOOR(RAND()*30) DAY)
FROM partymembers m WHERE m.Id BETWEEN 2 AND 12 LIMIT 20;

INSERT IGNORE INTO learningpoints (MemberId, Points, SourceType, Description, CreatedAt)
SELECT m.Id, 5, 2, '每日学习打卡获得积分', DATE_SUB(CURDATE(), INTERVAL FLOOR(RAND()*30) DAY)
FROM partymembers m WHERE m.Id BETWEEN 2 AND 20 LIMIT 40;

-- 同步更新党员总积分
UPDATE partymembers pm
SET pm.PointTotal = (SELECT COALESCE(SUM(lp.Points), 0) FROM learningpoints lp WHERE lp.MemberId = pm.Id)
WHERE pm.Id > 0;

-- 补充学习进度
INSERT IGNORE INTO member_learning_progress (member_id, content_id, duration_seconds, is_completed, completed_at, updated_at)
SELECT m.Id, c.Id, FLOOR(RAND()*600)+60, 1, DATE_SUB(NOW(), INTERVAL FLOOR(RAND()*30) DAY), DATE_SUB(NOW(), INTERVAL FLOOR(RAND()*30) DAY)
FROM partymembers m CROSS JOIN learningcontents c
WHERE m.Id BETWEEN 2 AND 10 AND c.Id BETWEEN 1 AND 20 LIMIT 50;

SELECT '第一轮回数据补充完成！' as result;
SELECT COUNT(*) as total_contents FROM learningcontents;
SELECT COUNT(*) as total_checkins FROM checkinrecords;
SELECT COUNT(*) as total_points FROM learningpoints;
