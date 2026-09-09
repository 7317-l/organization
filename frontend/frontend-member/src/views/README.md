# 党员端页面 (frontend-member/src/views/)

Vue3 + Element Plus，端口 5174。

## 页面清单（14个）

| 页面 | 路由 | 说明 |
|------|------|------|
| Login.vue | /login | 党员登录 |
| Home.vue | /home | 首页：待办任务、学习进度、总积分、AI推荐、积分排行前三 |
| LearningCenter.vue | /learning-center | 学习中心：公共素材/支部任务/AI定制路线/党史PK（四标签页） |
| ContentDetail.vue | /content-detail | 内容详情：文章/视频学习、进度上报、防挂机验证弹窗 |
| ExamCenter.vue | /exam-center | 考试中心：待考/历史成绩/错题本/专项练习 |
| Quiz.vue | /quiz | 答题页：在线答题、计时、提交 |
| Report.vue | /report | 学习报告：AI个人综合学习报告（量化评分+千问建议） |
| LearningRoadmap.vue | /roadmap | 学习路线图：AI三阶段规划、KPI指标、内容推荐 |
| Battle.vue | /battle | 党史PK：创建对战、应战、答题、计分、弃权退出、结果 |
| PairHelp.vue | /pair-help | 结对互助：AI薄弱点推荐、结对申请、帮扶记录、完成确认 |
| OrgLife.vue | /org-life | 组织生活：活动列表、报名、签到、提交心得 |
| EducationSites.vue | /education-sites | 红色基地：基地浏览、打卡、我的打卡记录 |
| AiChat.vue | /ai-chat | AI对话：RAG知识问答、语音输入（Web Speech API）、来源引用 |
| Profile.vue | /profile | 个人中心：学习记录、考试记录、打卡、积分、消息通知、账号设置 |

## 布局

- `MainLayout.vue` — 底部TabBar导航（首页/学习中心/考试中心/结对互助/组织生活/红色基地/我的）
- 顶部用户信息栏（姓名+退出登录）

## API封装

`src/api/` 目录下11个模块：ai, auth, checkin, content, exam, feature15, mobile, notification, points, request, task

`request.js` baseURL 使用相对路径 `/api/v1`，通过 vite 代理转发到后端 5091。
拦截器统一解包 `data` 字段，分页接口返回直接是列表（非包含total的对象）。

## 注意事项

- 学习进度上报：5秒后自动100%，再点上报（防挂机检测，超120秒需验证）
- 语音输入：仅 Chrome/Edge 支持 webkitSpeechRecognition，不支持时隐藏麦克风按钮
- 防挂机弹窗：ContentDetail.vue 中学习超120秒弹出验证，调用 /anti-cheat/challenge-v2 和 /verify-v2
