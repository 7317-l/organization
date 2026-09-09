# 管理端页面 (frontend-admin/src/views/)

Vue3 + Element Plus，端口 5173。

## 页面清单（12个）

| 页面 | 路由 | 说明 |
|------|------|------|
| Login.vue | /login | 管理员登录 |
| Dashboard.vue | /dashboard | 工作台：统计概览、学习趋势、预警提醒、快捷操作 |
| Organization.vue | /organization | 组织人员：党员管理（CRUD/导入导出/角色）、组织架构树 |
| LearningContent.vue | /learning-content | 学习内容：素材管理、任务派发（关联试卷+催办）、分类标签 |
| ExamManagement.vue | /exam-management | 题库测验：题目管理、试卷管理、考试发布与成绩 |
| OrgLife.vue | /org-life | 组织生活：活动发布、AI总结、审核归档上报 |
| DataAnalysis.vue | /data-analysis | 数据智能分析：NL2SQL自然语言查询、AI数据分析助手 |
| StarMembers.vue | /star-members | 学习标兵：AI五维评分、雷达图、排名榜单 |
| EducationSites.vue | /education-sites | 红色基地：基地管理、打卡记录 |
| Rectification.vue | /rectification | 整改闭环：整改项管理、状态跟踪 |
| PartyDevelopment.vue | /party-development | 党员发展：台账管理（创建/提交/审核/阶段推进/AI材料校验） |
| FullScreenDashboard.vue | /dashboard-fullscreen | 数字驾驶舱：全屏暗色主题、ECharts多图表联动 |

## 布局

- `MainLayout.vue` — 主布局（侧边栏菜单+顶部导航+内容区）
- 菜单结构：工作台 / 组织人员 / 学习内容 / 题库测验 / 组织生活 / 数据智能分析 / 学习标兵 / 红色基地 / 整改闭环 / 党员发展 / 数字驾驶舱

## API封装

`src/api/` 目录下17个模块：ai, auth, checkin, content, exam, feature15, meeting, member, mobile, notification, organization, partyDevelopment, points, question, request, statistics, task

`request.js` baseURL 使用相对路径 `/api/v1`，通过 vite 代理转发到后端 5091。
