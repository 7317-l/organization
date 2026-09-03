-- Seed data for new features
USE party_school;

-- 1. Education sites (10+)
INSERT IGNORE INTO education_sites (name, address, description, historical_facts, ai_interpretation, cover_url, latitude, longitude, created_at) VALUES
('井冈山革命根据地', '江西省吉安市井冈山市', '中国革命的摇篮，第一个农村革命根据地', '1927年毛泽东率领秋收起义部队到达井冈山，创建了第一个农村革命根据地', '井冈山精神是中国革命精神的重要组成部分，体现了坚定信念、艰苦奋斗、实事求是、敢闯新路、依靠群众、勇于胜利的精神', NULL, 26.5768, 114.1708, NOW()),
('延安革命纪念馆', '陕西省延安市宝塔区', '中共中央在延安13年的革命历史见证', '1935年中央红军长征到达陕北，延安成为中国革命的指挥中心', '延安精神是中国共产党在延安时期形成的革命精神，核心是坚定正确的政治方向、解放思想实事求是的思想路线、全心全意为人民服务的根本宗旨', NULL, 36.5853, 109.4898, NOW()),
('遵义会议会址', '贵州省遵义市红花岗区', '中国革命历史上生死攸关的转折点', '1935年遵义会议确立了毛泽东在党和红军中的领导地位', '遵义会议是中国共产党历史上一个生死攸关的转折点，标志着中国共产党从幼年走向成熟', NULL, 27.6847, 106.9279, NOW()),
('西柏坡纪念馆', '河北省石家庄市平山县', '解放战争时期中央工委、中共中央和解放军总部的所在地', '1948年中共中央移驻西柏坡，在这里指挥了辽沈、淮海、平津三大战役', '西柏坡精神体现了谦虚谨慎、艰苦奋斗、实事求是、一心为民的精神，是中国革命精神的重要组成部分', NULL, 38.2786, 113.9689, NOW()),
('南湖革命纪念馆', '浙江省嘉兴市南湖区', '中国共产党第一次全国代表大会闭幕地', '1921年中共一大在南湖红船上胜利闭幕，宣告中国共产党成立', '红船精神是中国革命精神之源，体现了开天辟地、敢为人先的首创精神，坚定理想、百折不挠的奋斗精神，立党为公、忠诚为民的奉献精神', NULL, 30.7458, 120.7564, NOW()),
('韶山毛泽东同志故居', '湖南省湘潭市韶山市', '毛泽东同志的出生地和早期革命活动地', '1893年毛泽东诞生于此，在这里度过了童年和少年时代', '韶山是中国人民的伟大领袖毛泽东的故乡，是全国著名的红色旅游景区和爱国主义教育示范基地', NULL, 27.9139, 112.5136, NOW()),
('中共一大会址纪念馆', '上海市黄浦区', '中国共产党第一次全国代表大会召开地', '1921年7月23日中共一大在上海召开，标志着中国共产党正式成立', '中共一大会址是中国共产党的诞生地，是中国共产党人的精神家园', NULL, 31.2203, 121.4756, NOW()),
('南昌八一起义纪念馆', '江西省南昌市西湖区', '南昌八一起义总指挥部旧址', '1927年8月1日南昌起义打响了武装反抗国民党反动派的第一枪', '南昌起义标志着中国共产党独立领导武装斗争和创建人民军队的开始，8月1日被定为中国人民解放军建军节', NULL, 28.6747, 115.8839, NOW()),
('百色起义纪念馆', '广西壮族自治区百色市右江区', '百色起义和右江革命根据地的历史见证', '1929年邓小平、张云逸等领导百色起义，创建了右江革命根据地', '百色起义是中国共产党在少数民族地区实行"工农武装割据"的一次光辉实践', NULL, 23.9028, 106.6183, NOW()),
('狼牙山五勇士陈列馆', '河北省保定市易县', '狼牙山五壮士英勇事迹纪念地', '1941年马宝玉、葛振林、宋学义、胡德林、胡福才五位战士为掩护主力转移，将日军引上狼牙山棋盘陀峰顶，打完最后一颗子弹后跳崖', '狼牙山五壮士的英雄事迹体现了崇高的爱国主义、革命英雄主义和坚贞不屈的民族气节', NULL, 39.1833, 115.1667, NOW()),
('邱少云烈士纪念馆', '重庆市铜梁区', '抗美援朝英雄邱少云烈士纪念地', '1952年邱少云在抗美援朝战争中，为了不暴露潜伏部队，忍受烈火焚烧直至壮烈牺牲', '邱少云烈士用生命诠释了军人的纪律和忠诚，是抗美援朝战争中涌现的英雄模范代表', NULL, 29.8389, 106.0583, NOW());

-- 2. Org rectifications (10+)
INSERT INTO org_rectifications (organization_id, quarter, issue, suggestion, status, remark, created_at, completed_at) VALUES
(4, '2026Q1', '部分党员学习积极性不高，学习任务完成率偏低', '加强学习督促，建立学习打卡制度，定期通报学习进度', 1, '已完成整改，学习完成率提升至85%', NOW(), DATE_ADD(NOW(), INTERVAL -30 DAY)),
(4, '2026Q1', '组织生活形式单一，党员参与度不高', '丰富组织生活形式，增加主题党日、实地研学等活动', 1, '已开展3次主题党日活动', NOW(), DATE_ADD(NOW(), INTERVAL -25 DAY)),
(5, '2026Q1', '考试平均分偏低，错题率较高', '建立错题本制度，开展专项练习和错题讲解', 1, '平均分从62提升至78', NOW(), DATE_ADD(NOW(), INTERVAL -20 DAY)),
(5, '2026Q2', '新党员培养进度滞后，材料不完整', '指定培养联系人，定期检查材料完整性', 0, '整改中，已补齐3名党员材料', NOW(), NULL),
(6, '2026Q1', '支部委员会会议记录不规范', '规范会议记录格式，明确记录责任人', 1, '已规范会议记录模板', NOW(), DATE_ADD(NOW(), INTERVAL -15 DAY)),
(6, '2026Q2', '党员发展工作进展缓慢', '制定发展计划，明确时间节点和责任人', 0, '已制定年度发展计划', NOW(), NULL),
(7, '2026Q1', '党费缴纳不及时', '建立党费缴纳提醒机制，每月5日前完成缴纳', 1, '缴纳率提升至100%', NOW(), DATE_ADD(NOW(), INTERVAL -10 DAY)),
(7, '2026Q2', '主题党日活动次数不足', '制定年度活动计划，每月至少开展1次主题党日', 0, '已开展4次，计划全年12次', NOW(), NULL),
(4, '2026Q2', '学习笔记检查流于形式', '建立学习笔记互评制度，每月抽查并通报', 0, '已开展2次抽查', NOW(), NULL),
(5, '2026Q2', '谈心谈话制度落实不到位', '明确谈心谈话频次要求，支部书记每季度与党员谈话不少于1次', 0, '已完成第一轮谈话', NOW(), NULL),
(6, '2026Q2', '志愿服务活动参与率低', '建立志愿服务积分制度，与评优评先挂钩', 0, '已组织2次志愿服务', NOW(), NULL);

-- 3. Battle games (10+ finished games)
INSERT INTO battle_games (challenger_id, opponent_id, status, question_ids, challenger_score, opponent_score, current_question_index, timeout_minutes, created_at, started_at, finished_at) VALUES
(2, 3, 2, '[1,2,3,4,5]', 35, 25, 5, 10, DATE_ADD(NOW(), INTERVAL -30 DAY), DATE_ADD(NOW(), INTERVAL -30 DAY), DATE_ADD(NOW(), INTERVAL -30 DAY)),
(3, 4, 2, '[1,2,3,4,5]', 30, 40, 5, 10, DATE_ADD(NOW(), INTERVAL -28 DAY), DATE_ADD(NOW(), INTERVAL -28 DAY), DATE_ADD(NOW(), INTERVAL -28 DAY)),
(4, 5, 2, '[1,2,3,4,5]', 45, 20, 5, 10, DATE_ADD(NOW(), INTERVAL -25 DAY), DATE_ADD(NOW(), INTERVAL -25 DAY), DATE_ADD(NOW(), INTERVAL -25 DAY)),
(5, 6, 2, '[1,2,3,4,5]', 25, 35, 5, 10, DATE_ADD(NOW(), INTERVAL -22 DAY), DATE_ADD(NOW(), INTERVAL -22 DAY), DATE_ADD(NOW(), INTERVAL -22 DAY)),
(6, 7, 2, '[1,2,3,4,5]', 40, 30, 5, 10, DATE_ADD(NOW(), INTERVAL -20 DAY), DATE_ADD(NOW(), INTERVAL -20 DAY), DATE_ADD(NOW(), INTERVAL -20 DAY)),
(7, 2, 2, '[1,2,3,4,5]', 30, 30, 5, 10, DATE_ADD(NOW(), INTERVAL -18 DAY), DATE_ADD(NOW(), INTERVAL -18 DAY), DATE_ADD(NOW(), INTERVAL -18 DAY)),
(2, 5, 2, '[1,2,3,4,5]', 50, 15, 5, 10, DATE_ADD(NOW(), INTERVAL -15 DAY), DATE_ADD(NOW(), INTERVAL -15 DAY), DATE_ADD(NOW(), INTERVAL -15 DAY)),
(3, 6, 2, '[1,2,3,4,5]', 20, 45, 5, 10, DATE_ADD(NOW(), INTERVAL -12 DAY), DATE_ADD(NOW(), INTERVAL -12 DAY), DATE_ADD(NOW(), INTERVAL -12 DAY)),
(4, 7, 2, '[1,2,3,4,5]', 35, 35, 5, 10, DATE_ADD(NOW(), INTERVAL -10 DAY), DATE_ADD(NOW(), INTERVAL -10 DAY), DATE_ADD(NOW(), INTERVAL -10 DAY)),
(5, 2, 2, '[1,2,3,4,5]', 40, 25, 5, 10, DATE_ADD(NOW(), INTERVAL -8 DAY), DATE_ADD(NOW(), INTERVAL -8 DAY), DATE_ADD(NOW(), INTERVAL -8 DAY)),
(6, 3, 2, '[1,2,3,4,5]', 30, 40, 5, 10, DATE_ADD(NOW(), INTERVAL -5 DAY), DATE_ADD(NOW(), INTERVAL -5 DAY), DATE_ADD(NOW(), INTERVAL -5 DAY)),
(7, 4, 2, '[1,2,3,4,5]', 45, 30, 5, 10, DATE_ADD(NOW(), INTERVAL -3 DAY), DATE_ADD(NOW(), INTERVAL -3 DAY), DATE_ADD(NOW(), INTERVAL -3 DAY));

-- 4. Battle records (10+)
INSERT INTO battlerecords (ChallengerId, OpponentId, ResultJson, BattleTime) VALUES
(2, 3, '{"winnerId":2,"challengerScore":35,"opponentScore":25,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -30 DAY)),
(3, 4, '{"winnerId":4,"challengerScore":30,"opponentScore":40,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -28 DAY)),
(4, 5, '{"winnerId":4,"challengerScore":45,"opponentScore":20,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -25 DAY)),
(5, 6, '{"winnerId":6,"challengerScore":25,"opponentScore":35,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -22 DAY)),
(6, 7, '{"winnerId":6,"challengerScore":40,"opponentScore":30,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -20 DAY)),
(7, 2, '{"winnerId":null,"challengerScore":30,"opponentScore":30,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -18 DAY)),
(2, 5, '{"winnerId":2,"challengerScore":50,"opponentScore":15,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -15 DAY)),
(3, 6, '{"winnerId":6,"challengerScore":20,"opponentScore":45,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -12 DAY)),
(4, 7, '{"winnerId":null,"challengerScore":35,"opponentScore":35,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -10 DAY)),
(5, 2, '{"winnerId":5,"challengerScore":40,"opponentScore":25,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -8 DAY)),
(6, 3, '{"winnerId":3,"challengerScore":30,"opponentScore":40,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -5 DAY)),
(7, 4, '{"winnerId":7,"challengerScore":45,"opponentScore":30,"questions":[1,2,3,4,5]}', DATE_ADD(NOW(), INTERVAL -3 DAY));

-- 5. Pair help records (10+)
INSERT INTO pairhelprecords (HelperId, HelpReceiverId, StartTime, EndTime, HelpContentJson, OutcomeSummary) VALUES
(2, 3, DATE_ADD(NOW(), INTERVAL -60 DAY), DATE_ADD(NOW(), INTERVAL -30 DAY), '{"focus":"党史理论","sessions":6,"materials":["党章","党史简史"]}', '帮扶效果良好，受助者考试成绩提升15分'),
(3, 4, DATE_ADD(NOW(), INTERVAL -55 DAY), DATE_ADD(NOW(), INTERVAL -25 DAY), '{"focus":"时政热点","sessions":5,"materials":["时事政治","政策解读"]}', '受助者时政题正确率从55%提升至80%'),
(4, 5, DATE_ADD(NOW(), INTERVAL -50 DAY), DATE_ADD(NOW(), INTERVAL -20 DAY), '{"focus":"党的理论","sessions":8,"materials":["习近平新时代中国特色社会主义思想"]}', '理论水平显著提升，能独立完成理论学习任务'),
(5, 6, DATE_ADD(NOW(), INTERVAL -45 DAY), DATE_ADD(NOW(), INTERVAL -15 DAY), '{"focus":"党史知识","sessions":4,"materials":["中国共产党简史"]}', '党史知识掌握更加扎实，PK对战胜率提升'),
(6, 7, DATE_ADD(NOW(), INTERVAL -40 DAY), DATE_ADD(NOW(), INTERVAL -10 DAY), '{"focus":"党规党纪","sessions":5,"materials":["中国共产党纪律处分条例"]}', '纪律意识增强，未出现违规情况'),
(2, 4, DATE_ADD(NOW(), INTERVAL -35 DAY), DATE_ADD(NOW(), INTERVAL -5 DAY), '{"focus":"综合提升","sessions":7,"materials":["党章","党史","时政"]}', '综合能力提升明显，学习标兵评选排名上升5位'),
(3, 5, DATE_ADD(NOW(), INTERVAL -30 DAY), NULL, '{"focus":"考试技巧","sessions":3,"materials":[]}', '帮扶进行中，已完成3次辅导'),
(4, 6, DATE_ADD(NOW(), INTERVAL -25 DAY), NULL, '{"focus":"写作能力","sessions":2,"materials":["思想汇报范文"]}', '帮扶进行中，思想汇报质量提升'),
(5, 7, DATE_ADD(NOW(), INTERVAL -20 DAY), NULL, '{"focus":"理论学习","sessions":4,"materials":[]}', '帮扶进行中，每周1次学习交流'),
(6, 2, DATE_ADD(NOW(), INTERVAL -15 DAY), NULL, '{"focus":"新党员培养","sessions":2,"materials":["入党培训教材"]}', '帮扶进行中，指导新党员熟悉组织生活'),
(7, 3, DATE_ADD(NOW(), INTERVAL -10 DAY), NULL, '{"focus":"实践能力","sessions":1,"materials":[]}', '帮扶进行中，已开展1次实践指导');

-- 6. Pair help requests (some)
INSERT INTO pair_help_requests (helper_id, help_receiver_id, status, match_reason, created_at, updated_at) VALUES
(2, 6, 1, '薄弱点匹配：党史理论薄弱，帮扶者党史成绩优秀', NOW(), NOW()),
(3, 7, 0, '薄弱点匹配：时政热点薄弱，帮扶者时政成绩优秀', NOW(), NOW()),
(4, 2, 2, '主动申请：希望提升综合能力', DATE_ADD(NOW(), INTERVAL -5 DAY), DATE_ADD(NOW(), INTERVAL -3 DAY));
