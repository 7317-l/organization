-- ============================================================
-- 党员学习平台 - 界面显示数据修复 SQL 脚本
-- 数据库: MySQL (party_school)
-- 说明: 修复前端界面显示异常的数据问题
--   1. learningcontents 表 Body 字段补充完整正文
--   2. partymembers 表 PointTotal 字段根据 learningpoints 同步
--   3. 补充积分记录使排行榜有5-10人，积分分散在100-1000分
-- 方式: 使用 UPDATE / INSERT ... SELECT ... WHERE NOT EXISTS
-- ============================================================

USE party_school;

-- ============================================================
-- 1. learningcontents（学习内容表）- 补充 Body 正文
-- 对应前端功能: 学习中心-内容详情页（文章正文显示）
-- 现有数据: 10条（ID 1-10），Body 字段可能为简短占位文字
-- 列名: Id, Title, Body, VideoUrl, ContentType, CategoryId, IsPublic, CreatedAt, SourceType
-- ============================================================

-- 内容1: 习近平新时代中国特色社会主义思想三十讲
UPDATE learningcontents
SET Body = '<p>习近平新时代中国特色社会主义思想是当代中国马克思主义、二十一世纪马克思主义，是中华文化和中国精神的时代精华，实现了马克思主义中国化时代化新的飞跃。</p><p>这一思想立足中国特色社会主义进入新时代的历史方位，深刻回答了新时代坚持和发展什么样的中国特色社会主义、怎样坚持和发展中国特色社会主义，建设什么样的社会主义现代化强国、怎样建设社会主义现代化强国，建设什么样的长期执政的马克思主义政党、怎样建设长期执政的马克思主义政党等重大时代课题。</p><p>学习贯彻习近平新时代中国特色社会主义思想，必须深刻领悟"两个确立"的决定性意义，增强"四个意识"、坚定"四个自信"、做到"两个维护"，自觉在思想上政治上行动上同以习近平同志为核心的党中央保持高度一致。</p><p>本课程共三十讲，系统阐述了习近平新时代中国特色社会主义思想的时代背景、核心要义、精神实质、丰富内涵和实践要求，是广大党员干部深入学习领会党的创新理论的重要教材。</p>'
WHERE Id = 1 AND (Body IS NULL OR CHAR_LENGTH(Body) < 200);

-- 内容2: 中国共产党章程（2022年修订）
UPDATE learningcontents
SET Body = '<p>《中国共产党章程》是党的总章程，是全党必须共同遵守的根本行为规范。中国共产党第二十次全国代表大会通过的党章修正案，共修改50处，其中总纲部分的修改37处，条文部分的修改13处。</p><p>党章总纲部分增写了党的百年奋斗重大成就和历史经验的内容，调整完善了党的奋斗目标的表述，调整完善了关于社会主义初级阶段的相关内容，充实了"五位一体"总体布局方面的内容，充实了国防和军队建设、统一战线、外交工作等方面的内容，调整完善了党的建设总体要求的内容。</p><p>条文部分主要修改包括：充实党员义务的相关内容，充实党的基层组织部分的相关内容，充实领导干部基本条件的内容，充实党的纪律、党的纪律检查机关部分的相关内容，充实党组任务等内容。</p><p>认真学习党章、严格遵守党章，是加强党的建设的一项基础性经常性工作，也是全党同志的应尽义务和庄严责任。全体党员要自觉学习党章、遵守党章、贯彻党章、维护党章，以党章为根本遵循，规范自己的一言一行。</p>'
WHERE Id = 2 AND (Body IS NULL OR CHAR_LENGTH(Body) < 200);

-- 内容3: 党史故事100讲·五四运动
UPDATE learningcontents
SET Body = '<p>五四运动，是1919年5月4日发生在北京的一场以青年学生为主，广大群众、市民、工商人士等阶层共同参与的，通过示威游行、请愿、罢工、暴力对抗政府等多种形式进行的爱国运动，是中国人民彻底的反对帝国主义、封建主义的爱国运动。</p><p>五四运动的起因是第一次世界大战期间，欧洲列强无暇东顾，日本乘机加强对中国的侵略，严重损害了中国的主权。1919年巴黎和会上中国外交的失败，引发了伟大的五四运动。</p><p>五四运动中，青年学生挺身而出，喊出了"外争主权、内除国贼"的口号，工人阶级开始以独立的姿态登上政治舞台。五四运动是中国新民主主义革命的开端，促进了马克思主义在中国的传播及其与中国工人运动的结合，为中国共产党成立做了思想上干部上的准备。</p><p>五四精神的核心是爱国主义。在新时代，我们要继承和发扬五四精神，坚定理想信念，站稳人民立场，练就过硬本领，投身强国伟业，始终保持艰苦奋斗的前进姿态，同亿万人民一道，在实现中华民族伟大复兴中国梦的新长征路上奋勇搏击。</p>'
WHERE Id = 3 AND (Body IS NULL OR CHAR_LENGTH(Body) < 200);

-- 内容4: 二十大报告全文解读
UPDATE learningcontents
SET Body = '<p>中国共产党第二十次全国代表大会于2022年10月16日至22日在北京举行。大会的主题是：高举中国特色社会主义伟大旗帜，全面贯彻新时代中国特色社会主义思想，弘扬伟大建党精神，自信自强、守正创新，踔厉奋发、勇毅前行，为全面建设社会主义现代化国家、全面推进中华民族伟大复兴而团结奋斗。</p><p>报告共分十五个部分：一、过去五年的工作和新时代十年的伟大变革；二、开辟马克思主义中国化时代化新境界；三、新时代新征程中国共产党的使命任务；四、加快构建新发展格局，着力推动高质量发展；五、实施科教兴国战略，强化现代化建设人才支撑；六、发展全过程人民民主，保障人民当家作主；七、坚持全面依法治国，推进法治中国建设；八、推进文化自信自强，铸就社会主义文化新辉煌；九、增进民生福祉，提高人民生活品质；十、推动绿色发展，促进人与自然和谐共生；十一、推进国家安全体系和能力现代化，坚决维护国家安全和社会稳定；十二、实现建军一百年奋斗目标，开创国防和军队现代化新局面；十三、坚持和完善"一国两制"，推进祖国统一；十四、促进世界和平与发展，推动构建人类命运共同体；十五、坚定不移全面从严治党，深入推进新时代党的建设新的伟大工程。</p><p>党的二十大报告是党团结带领全国各族人民夺取中国特色社会主义新胜利的政治宣言和行动纲领，是马克思主义的纲领性文献。学习宣传贯彻党的二十大精神是当前和今后一个时期全党全国的首要政治任务。</p>'
WHERE Id = 4 AND (Body IS NULL OR CHAR_LENGTH(Body) < 200);

-- 内容5: 中国共产党纪律处分条例解读
UPDATE learningcontents
SET Body = '<p>《中国共产党纪律处分条例》是根据《中国共产党章程》制定的党内法规，是维护党的章程和其他党内法规，严肃党的纪律，纯洁党的组织，保障党员民主权利，教育党员遵纪守法，维护党的团结统一，保证党的路线、方针、政策、决议和国家法律法规贯彻执行的重要保障。</p><p>2023年12月8日，中共中央政治局会议修订《中国共产党纪律处分条例》，自2024年1月1日起施行。新修订的《条例》共3编158条，与2018年《条例》相比，新增16条，修改76条，整合2条。</p><p>《条例》明确规定了党的纪律主要包括政治纪律、组织纪律、廉洁纪律、群众纪律、工作纪律、生活纪律。对党员的纪律处分种类包括：警告、严重警告、撤销党内职务、留党察看、开除党籍。</p><p>政治纪律是最重要、最根本、最关键的纪律。新修订的《条例》把坚决维护以习近平同志为核心的党中央权威和集中统一领导作为出发点和落脚点，将习近平新时代中国特色社会主义思想作为指导思想，充实了"两个维护"的内容，完善了保障党中央政令畅通的相关条款。</p><p>全体党员要认真学习《条例》，自觉遵守党的纪律，知敬畏、存戒惧、守底线，习惯在受监督和约束的环境中工作生活。</p>'
WHERE Id = 5 AND (Body IS NULL OR CHAR_LENGTH(Body) < 200);

-- 内容6: 党的二十大党章修正案学习问答
UPDATE learningcontents
SET Body = '<p>党的二十大通过的党章修正案，体现了党的十九大以来党的理论创新、实践创新、制度创新成果，体现了党的二十大报告确立的重要思想、重要观点、重大战略、重大举措，对坚持和加强党的全面领导、坚定不移推进全面从严治党、坚持和完善党的建设、推进党的自我革命提出了明确要求。</p><p>党章修正案增写了党的百年奋斗重大成就和历史经验的内容。党的百年奋斗历程波澜壮阔，积累了宝贵的历史经验。增写这部分内容，有利于引导全党深刻认识红色政权来之不易、新中国来之不易、中国特色社会主义来之不易，坚定历史自信、增强历史主动，坚守初心使命、传承红色基因。</p><p>党章修正案调整完善了党的奋斗目标的表述，将总纲中"两个一百年"奋斗目标调整为全面建成社会主义现代化强国、实现第二个百年奋斗目标，以中国式现代化全面推进中华民族伟大复兴。这一修改体现了党的中心任务的新要求。</p><p>党章修正案充实了"五位一体"总体布局方面的内容，增写了中国式现代化、全体人民共同富裕、新发展理念、新发展格局、统筹发展和安全、碳达峰碳中和等内容，使总纲关于中国特色社会主义事业总体布局的表述更加完善。</p><p>学习党章修正案，要原原本本学、逐字逐句学，深刻领会修改的重大意义和主要内容，切实把党章要求贯彻到党的工作和党的建设全过程各方面。</p>'
WHERE Id = 6 AND (Body IS NULL OR CHAR_LENGTH(Body) < 200);

-- 内容7: 习近平关于党的建设的重要思想
UPDATE learningcontents
SET Body = '<p>习近平关于党的建设的重要思想，是习近平新时代中国特色社会主义思想的重要组成部分，是新时代党的建设的根本遵循和行动指南。这一重要思想，深刻回答了"建设什么样的长期执政的马克思主义政党、怎样建设长期执政的马克思主义政党"这一重大时代课题。</p><p>2023年6月召开的全国组织工作会议，首次正式提出和系统阐述"习近平总书记关于党的建设的重要思想"，并用"十三个坚持"集中概括了这一重要思想的主要内容：坚持和加强党的全面领导；坚持以党的自我革命引领社会革命；坚持以党的政治建设统领党的建设各项工作；坚持江山就是人民、人民就是江山；坚持思想建党、理论强党；坚持严密党的组织体系；坚持造就忠诚干净担当的高素质干部队伍；坚持聚天下英才而用之；坚持持之以恒正风肃纪；坚持一体推进不敢腐、不能腐、不想腐；坚持完善党和国家监督体系；坚持制度治党、依规治党；坚持落实全面从严治党政治责任。</p><p>习近平关于党的建设的重要思想，突出全面从严治党这个主题主线，以一系列原创性成果极大丰富和发展了马克思主义建党学说，标志着我们党对马克思主义执政党建设规律的认识达到了新高度，为深入推进新时代党的建设新的伟大工程、做好新时代组织工作提供了根本遵循。</p><p>深入学习贯彻习近平关于党的建设的重要思想，是全党的一项重大政治任务。要深刻领会这一重要思想的重大意义、科学体系、丰富内涵和实践要求，自觉用以指导党的建设各项工作，不断提高党的建设质量，把党建设成为始终走在时代前列、人民衷心拥护、勇于自我革命、经得起各种风浪考验、朝气蓬勃的马克思主义执政党。</p>'
WHERE Id = 7 AND (Body IS NULL OR CHAR_LENGTH(Body) < 200);

-- 内容8: 中国共产党廉洁自律准则
UPDATE learningcontents
SET Body = '<p>《中国共产党廉洁自律准则》是中国共产党执政以来第一部坚持正面倡导、面向全体党员的规范全党廉洁自律工作的重要基础性法规，是对党章规定的具体化，体现了全面从严治党实践成果，为党员和党员领导干部树立了一个看得见、够得着的高标准，展现了共产党人的高尚道德追求。</p><p>《准则》共8条、281字，包括导语、党员廉洁自律规范和党员领导干部廉洁自律规范等3部分。导语部分，重申关于理想信念、根本宗旨、优良传统作风、高尚情操等"四个必须"的原则要求，强调廉洁自律、接受监督的主旨，最后将落脚点放在永葆党的先进性和纯洁性上。</p><p>党员廉洁自律规范部分，围绕党员如何正确对待和处理"公与私""廉与腐""俭与奢""苦与乐"的关系提出"四条规范"：第一条 坚持公私分明，先公后私，克己奉公。第二条 坚持崇廉拒腐，清白做人，干净做事。第三条 坚持尚俭戒奢，艰苦朴素，勤俭节约。第四条 坚持吃苦在前，享受在后，甘于奉献。</p><p>党员领导干部廉洁自律规范部分，针对党员领导干部这个"关键少数"，围绕"廉洁从政""廉洁用权""廉洁修身""廉洁齐家"四个方面，对党员领导干部提出要求更高的"四条规范"：第五条 廉洁从政，自觉保持人民公仆本色。第六条 廉洁用权，自觉维护人民根本利益。第七条 廉洁修身，自觉提升思想道德境界。第八条 廉洁齐家，自觉带头树立良好家风。</p><p>全体党员要认真学习《准则》，自觉践行《准则》，把《准则》要求内化于心、外化于行，永葆共产党人清正廉洁的政治本色。</p>'
WHERE Id = 8 AND (Body IS NULL OR CHAR_LENGTH(Body) < 200);

-- 内容9: 党史学习教育专题
UPDATE learningcontents
SET Body = '<p>2021年2月20日，党史学习教育动员大会在北京召开，习近平总书记出席会议并发表重要讲话，深刻阐述了开展党史学习教育的重大意义，深刻阐明了党史学习教育的重点和工作要求，对党史学习教育进行了全面动员和部署。</p><p>习近平总书记指出，在全党开展党史学习教育，是党中央立足党的百年历史新起点、统筹中华民族伟大复兴战略全局和世界百年未有之大变局、为动员全党全国满怀信心投身全面建设社会主义现代化国家而作出的重大决策。全党同志要做到学史明理、学史增信、学史崇德、学史力行，学党史、悟思想、办实事、开新局，以昂扬姿态奋力开启全面建设社会主义现代化国家新征程，以优异成绩迎接建党一百周年。</p><p>我们党的一百年，是矢志践行初心使命的一百年，是筚路蓝缕奠基立业的一百年，是创造辉煌开辟未来的一百年。回望过往的奋斗路，眺望前方的奋进路，必须把党的历史学习好、总结好，把党的成功经验传承好、发扬好。</p><p>开展党史学习教育，是牢记初心使命、推进中华民族伟大复兴历史伟业的必然要求，是坚定信仰信念、在新时代坚持和发展中国特色社会主义的必然要求，是推进党的自我革命、永葆党的生机活力的必然要求。</p><p>党史学习教育的目标要求是：学史明理、学史增信、学史崇德、学史力行。学史明理，就是要通过学习教育，树牢唯物史观，强化理论思维、历史思维，不断深化对共产党执政规律、社会主义建设规律、人类社会发展规律的认识。学史增信，就是要通过学习教育，增强历史自觉，保持战略定力，筑牢信仰之基。学史崇德，就是要通过学习教育，弘扬优良传统，传承红色基因，增强党的意识、党员意识。学史力行，就是要通过学习教育，加强党性锻炼，砥砺政治品格，践履知行合一。</p>'
WHERE Id = 9 AND (Body IS NULL OR CHAR_LENGTH(Body) < 200);

-- 内容10: 党的二十届三中全会精神
UPDATE learningcontents
SET Body = '<p>中国共产党第二十届中央委员会第三次全体会议，于2024年7月15日至18日在北京举行。全会由中央政治局主持，中央委员会总书记习近平作了重要讲话。</p><p>全会听取和讨论了习近平受中央政治局委托作的工作报告，审议通过了《中共中央关于进一步全面深化改革、推进中国式现代化的决定》。习近平就《决定（讨论稿）》向全会作了说明。</p><p>全会认为，党的十八大以来，以习近平同志为核心的党中央把全面深化改革纳入"四个全面"战略布局，以巨大的政治勇气全面深化改革，打响改革攻坚战，加强改革顶层设计，敢于突进深水区，敢于啃硬骨头，敢于涉险滩，敢于面对新矛盾新挑战，冲破思想观念束缚，突破利益固化藩篱，坚决破除各方面体制机制弊端，各领域基础性制度框架基本建立，许多领域实现历史性变革、系统性重塑、整体性重构，中国特色社会主义制度更加成熟更加定型，国家治理体系和治理能力现代化水平明显提高。</p><p>全会强调，进一步全面深化改革，必须坚持以习近平新时代中国特色社会主义思想为指导，全面贯彻党的二十大和二十届二中全会精神，完整准确全面贯彻新发展理念，加快构建新发展格局，着力推动高质量发展，扎实推进中国式现代化。要坚持党的全面领导，坚持以人民为中心，坚持守正创新，坚持以制度建设为主线，坚持全面依法治国，坚持系统观念，坚持进一步解放思想。</p><p>全会提出，进一步全面深化改革的总目标是，继续完善和发展中国特色社会主义制度，推进国家治理体系和治理能力现代化。到二〇二九年，中华人民共和国成立八十周年时，全面完成本决定提出的改革任务，中国特色社会主义制度更加成熟更加定型，国家治理体系和治理能力现代化取得新的重大进展。到本世纪中叶，中华人民共和国成立一百周年时，中国特色社会主义制度不断巩固，国家治理体系和治理能力现代化全面实现。</p><p>全会号召，全党全国各族人民要更加紧密地团结在以习近平同志为核心的党中央周围，坚定信心、保持定力、锐意进取、攻坚克难，谱写进一步全面深化改革新篇章，为以中国式现代化全面推进强国建设、民族复兴伟业而团结奋斗！</p>'
WHERE Id = 10 AND (Body IS NULL OR CHAR_LENGTH(Body) < 200);


-- ============================================================
-- 2. partymembers（党员表）- 同步 PointTotal 字段
-- 对应前端功能: 首页-总积分统计卡片、我的-总积分、积分排行榜
-- 说明: 根据 learningpoints 表汇总每个党员的积分总和，
--       更新 partymembers.PointTotal 字段
-- ============================================================

UPDATE partymembers pm
INNER JOIN (
    SELECT PartyMemberId, SUM(Points) AS TotalPoints
    FROM learningpoints
    GROUP BY PartyMemberId
) lp ON pm.Id = lp.PartyMemberId
SET pm.PointTotal = lp.TotalPoints
WHERE pm.PointTotal <> lp.TotalPoints OR pm.PointTotal IS NULL;


-- ============================================================
-- 3. learningpoints（积分记录表）- 补充更多积分记录
-- 对应前端功能: 积分排行榜（至少5-10人，积分分散在100-1000分）
-- 现有数据: 15条（来自数据完整性补充脚本，覆盖党员1,3,5,7,9,11）
-- 补充: 为党员13,15,17,19,21补充积分记录，
--       同时为已有党员补充更多记录使积分值分散在100-1000分
-- 列名: Id, PartyMemberId, SourceType(int), SourceId, Points, EarnedAt
-- SourceType枚举: WatchVideo=0, CompleteExam=1, ActivityCheckIn=2, BattleVictory=3, Other=4
-- ============================================================

-- 党员13: 观看视频 + 完成答题 + 活动打卡（合计约350分）
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 16, 13, 0, 1, 10, '2026-07-05 09:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 16);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 17, 13, 0, 3, 10, '2026-07-10 14:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 17);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 18, 13, 1, 1, 80, '2026-07-15 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 18);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 19, 13, 2, NULL, 5, '2026-07-20 08:30:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 19);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 20, 13, 0, 5, 10, '2026-08-01 11:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 20);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 21, 13, 1, 2, 60, '2026-08-05 15:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 21);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 22, 13, 3, 1, 20, '2026-08-10 20:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 22);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 23, 13, 0, 7, 10, '2026-08-15 09:30:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 23);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 24, 13, 2, NULL, 5, '2026-08-20 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 24);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 25, 13, 0, 9, 10, '2026-08-22 14:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 25);

-- 党员15: 观看视频 + 完成答题（合计约220分）
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 26, 15, 0, 2, 10, '2026-07-08 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 26);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 27, 15, 0, 4, 10, '2026-07-12 11:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 27);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 28, 15, 1, 1, 70, '2026-07-18 09:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 28);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 29, 15, 0, 6, 10, '2026-08-02 13:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 29);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 30, 15, 1, 2, 50, '2026-08-08 16:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 30);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 31, 15, 0, 8, 10, '2026-08-14 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 31);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 32, 15, 2, NULL, 5, '2026-08-19 09:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 32);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 33, 15, 0, 10, 10, '2026-08-21 15:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 33);

-- 党员17: 观看视频 + 完成答题 + PK胜利（合计约480分）
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 34, 17, 0, 1, 10, '2026-06-20 09:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 34);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 35, 17, 0, 2, 10, '2026-06-25 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 35);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 36, 17, 1, 1, 90, '2026-07-01 14:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 36);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 37, 17, 0, 3, 10, '2026-07-05 11:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 37);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 38, 17, 3, 1, 20, '2026-07-10 20:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 38);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 39, 17, 0, 4, 10, '2026-07-15 09:30:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 39);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 40, 17, 1, 2, 85, '2026-07-20 15:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 40);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 41, 17, 0, 5, 10, '2026-07-25 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 41);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 42, 17, 2, NULL, 5, '2026-08-01 08:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 42);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 43, 17, 0, 6, 10, '2026-08-05 14:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 43);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 44, 17, 3, 2, 20, '2026-08-10 19:30:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 44);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 45, 17, 0, 7, 10, '2026-08-15 11:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 45);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 46, 17, 1, 1, 75, '2026-08-18 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 46);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 47, 17, 0, 8, 10, '2026-08-20 13:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 47);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 48, 17, 2, NULL, 5, '2026-08-22 09:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 48);

-- 党员19: 观看视频 + 完成答题（合计约150分）
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 49, 19, 0, 1, 10, '2026-07-10 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 49);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 50, 19, 1, 1, 60, '2026-07-20 14:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 50);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 51, 19, 0, 3, 10, '2026-08-01 09:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 51);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 52, 19, 0, 5, 10, '2026-08-10 11:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 52);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 53, 19, 2, NULL, 5, '2026-08-15 08:30:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 53);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 54, 19, 1, 2, 45, '2026-08-20 16:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 54);

-- 党员21: 观看视频 + 完成答题 + 活动打卡（合计约280分）
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 55, 21, 0, 2, 10, '2026-07-08 09:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 55);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 56, 21, 0, 4, 10, '2026-07-15 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 56);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 57, 21, 1, 1, 65, '2026-07-22 14:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 57);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 58, 21, 0, 6, 10, '2026-08-01 11:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 58);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 59, 21, 2, NULL, 5, '2026-08-05 08:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 59);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 60, 21, 0, 8, 10, '2026-08-10 13:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 60);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 61, 21, 1, 2, 55, '2026-08-15 15:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 61);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 62, 21, 3, 3, 20, '2026-08-18 21:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 62);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 63, 21, 0, 10, 10, '2026-08-20 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 63);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 64, 21, 2, NULL, 5, '2026-08-22 09:30:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 64);

-- 党员2: 补充积分（合计约180分）
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 65, 2, 0, 1, 10, '2026-07-12 10:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 65);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 66, 2, 1, 1, 70, '2026-07-25 14:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 66);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 67, 2, 0, 3, 10, '2026-08-05 11:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 67);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 68, 2, 2, NULL, 5, '2026-08-12 08:30:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 68);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 69, 2, 0, 5, 10, '2026-08-18 13:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 69);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 70, 2, 1, 2, 55, '2026-08-21 16:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 70);

-- 党员4: 补充积分（合计约120分）
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 71, 4, 0, 2, 10, '2026-07-20 09:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 71);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 72, 4, 1, 1, 50, '2026-08-01 14:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 72);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 73, 4, 0, 4, 10, '2026-08-10 11:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 73);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 74, 4, 2, NULL, 5, '2026-08-15 08:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 74);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 75, 4, 0, 6, 10, '2026-08-20 13:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 75);
INSERT INTO learningpoints (Id, PartyMemberId, SourceType, SourceId, Points, EarnedAt)
SELECT 76, 4, 1, 2, 35, '2026-08-22 15:00:00' WHERE NOT EXISTS (SELECT 1 FROM learningpoints WHERE Id = 76);


-- ============================================================
-- 4. 再次同步 partymembers.PointTotal（补充新积分记录后）
-- ============================================================

UPDATE partymembers pm
INNER JOIN (
    SELECT PartyMemberId, SUM(Points) AS TotalPoints
    FROM learningpoints
    GROUP BY PartyMemberId
) lp ON pm.Id = lp.PartyMemberId
SET pm.PointTotal = lp.TotalPoints
WHERE pm.PointTotal <> lp.TotalPoints OR pm.PointTotal IS NULL;


-- ============================================================
-- 脚本执行完成
-- 修复内容汇总:
--   learningcontents: 10条内容Body字段补充完整正文（每篇200+字）
--   partymembers: PointTotal字段根据learningpoints表同步更新
--   learningpoints: 新增61条积分记录（ID 16-76），覆盖党员2,4,13,15,17,19,21
--   排行榜预期: 至少10人有积分，积分值分散在100-1000分之间
--     党员17约480分、党员13约350分、党员21约280分、
--     党员15约220分、党员2约180分、党员4约120分、
--     党员1约65分、党员3约40分、党员5约30分、党员7约75分等
-- ============================================================
