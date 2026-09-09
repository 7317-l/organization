# 数据模型 (Models/)

## 目录结构

```
Models/
├── Entities/     # EF Core实体类（映射数据库表）
├── DTOs/         # 数据传输对象（请求/响应模型）
└── Common/       # 通用模型（ApiResponse、PagedResponse、枚举）
```

## 实体命名约定

项目采用混合命名风格：
- **PascalCase表**（EF默认，无[Table]注解）：partymembers, partydevelopmentprocesses, meetingactivities, organizations, learningcontents, learningpoints, checkinrecords, battlerecords, pairhelprecords, memberlearningreports, organizationquarterlyreports, nl2sql_sessions, org_rectifications, organization_quarterly_ratings, battle_games, member_learning_progress, member_test_records, activityhearts, messagenotifications, education_sites, exam_papers, examtests, learning_tasks, questions, content_categories, content_tags, question_categories, tags, task_contents, anticheat_records, pair_help_requests, party_development_reminders
- **snake_case表**（带[Table]/[Column]注解）：learning_tasks, questions, learningcontents, content_categories, content_tags, question_categories, tags, task_contents, education_sites, exam_papers, examtests, anticheat_records, messagenotifications, activityhearts, member_learning_progress, member_test_records, battle_games, pair_help_requests, org_rectifications, organization_quarterly_ratings, nl2sql_sessions, party_development_reminders

> 注意：meetingactivities表新增的6个字段（status/reviewer_id等）使用snake_case，实体已加[Column]注解映射。

## 核心实体

| 实体 | 表名 | 说明 |
|------|------|------|
| PartyMember | partymembers | 党员信息 |
| Organization | organizations | 组织架构（树形，ParentId） |
| LearningContent | learningcontents | 学习内容（文章/视频） |
| LearningTask | learning_tasks | 学习任务派发 |
| ExamPaper | exam_papers | 试卷 |
| ExamTest | examtests | 考试发布 |
| Question | questions | 题库 |
| MeetingActivity | meetingactivities | 组织生活活动 |
| PartyDevelopmentProcess | partydevelopmentprocesses | 党员发展记录 |
| MessageNotification | messagenotifications | 消息通知 |
| BattleGame | battle_games | 党史PK对局 |
| EducationSite | education_sites | 红色教育基地 |
| AntiCheatRecord | anticheat_records | 防挂机验证记录 |

## 通用模型

- `ApiResponse` — 统一响应格式 `{ code, message, data }`
- `PagedResponse` — 分页响应 `{ code, message, data, page, size, total }`
- `UserRole` — 角色枚举：PartyMember=0, BranchSecretary=1, SystemAdmin=2
