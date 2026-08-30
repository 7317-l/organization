<template>
  <div class="home-page">
    <!-- 欢迎语 -->
    <div class="welcome">
      <h1>{{ greeting }}，{{ userName }}，欢迎回来！</h1>
      <el-tag v-if="userBranch || userRole" type="danger" effect="light" class="org-tag">
        {{ userBranch }} · {{ userRole || '党员' }}
      </el-tag>
    </div>

    <!-- 第一排：小统计卡片 + 待办提醒 并列 -->
    <div class="top-row">
      <!-- 三个小统计卡片 -->
      <div class="top-stats" v-loading="overviewLoading">
        <div class="mini-stat">
          <div class="mini-icon r">
            <el-icon :size="18"><List /></el-icon>
          </div>
          <div class="mini-info">
            <div class="mini-label">待办任务</div>
            <div class="mini-value">{{ overview.pendingCount || 0 }}<span class="unit">项</span></div>
          </div>
        </div>
        <div class="mini-stat clickable" @click="goReport" title="查看学习报告">
          <div class="mini-icon b">
            <el-icon :size="18"><Clock /></el-icon>
          </div>
          <div class="mini-info">
            <div class="mini-label">学习进度</div>
            <div class="mini-value">{{ overview.learningProgress || 0 }}<span class="unit">%</span></div>
          </div>
        </div>
        <div class="mini-stat">
          <div class="mini-icon g">
            <el-icon :size="18"><Star /></el-icon>
          </div>
          <div class="mini-info">
            <div class="mini-label">总积分</div>
            <div class="mini-value">{{ overview.totalPoints || 0 }}<span class="unit">分</span></div>
          </div>
        </div>
      </div>

      <!-- 待办提醒 -->
      <div class="top-todo">
        <div class="section-header" style="margin-bottom:10px">
          <div class="section-title">待办提醒</div>
        </div>
        <div class="todo-list compact" v-loading="todoLoading">
          <template v-if="todoList.length > 0">
            <div
              v-for="item in todoList.slice(0, 3)"
              :key="item.id"
              class="todo-item compact"
              @click="handleTodoClick(item)"
            >
              <div class="todo-icon sm" :class="item.type === 'exam' ? 'b' : 'r'">
                <el-icon :size="16">
                  <Document v-if="item.type === 'exam'" />
                  <Calendar v-else />
                </el-icon>
              </div>
              <div class="todo-content">
                <div class="todo-title sm">{{ item.title }}</div>
                <div class="todo-meta">
                  <el-tag v-if="item.deadline" type="danger" effect="light" size="small">
                    {{ formatDeadline(item.deadline) }}
                  </el-tag>
                  <el-tag v-else type="warning" effect="light" size="small">
                    {{ item.type === 'exam' ? '待参加' : '待完成' }}
                  </el-tag>
                </div>
              </div>
            </div>
          </template>
          <el-empty v-else description="暂无待办" :image-size="60" />
        </div>
      </div>
    </div>

    <!-- 第二排：AI推荐 + 积分排行 并列 -->
    <div class="bottom-row">
      <!-- AI 推荐 -->
      <div class="bottom-card">
        <div class="section-header" style="margin-bottom:10px">
          <div class="section-title">AI 为你推荐</div>
        </div>
        <div class="ai-section compact" v-loading="recommendLoading">
          <div class="ai-cards compact" v-if="recommendations.length > 0">
            <div
              v-for="item in recommendations.slice(0, 4)"
              :key="item.id"
              class="ai-card compact"
              @click="goContentDetail(item.id)"
            >
              <div class="ai-card-icon sm" :class="item.contentType === 1 ? 'v' : 'd'">
                <el-icon :size="18"><VideoPlay v-if="item.contentType === 1" /><Document v-else /></el-icon>
              </div>
              <div class="ai-card-content">
                <div class="ai-card-title sm">{{ item.title }}</div>
              </div>
            </div>
          </div>
          <el-empty v-else description="暂无推荐" :image-size="50" />
        </div>
      </div>

      <!-- 积分排行 -->
      <div class="bottom-card">
        <div class="section-header" style="margin-bottom:10px">
          <div class="section-title">积分排行</div>
          <span class="section-more" @click="goProfile">更多 →</span>
        </div>
        <div class="rank-section compact" v-loading="rankingLoading">
          <div class="rank-list compact" v-if="rankingList.length > 0">
            <div v-for="(item, index) in rankingList.slice(0, 3)" :key="item.memberId || item.id" class="rank-item compact">
              <div class="rank-badge sm" :class="'g' + (index + 1)">
                {{ index === 0 ? '1' : index === 1 ? '2' : '3' }}
              </div>
              <div class="rank-info">
                <div class="rank-name sm">{{ item.memberName || item.name }}</div>
              </div>
              <div class="rank-score sm">{{ item.totalPoints || item.points }}<span>分</span></div>
            </div>
          </div>
          <el-empty v-else description="暂无排行" :image-size="50" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { List, Clock, Star, Document, Calendar, VideoPlay } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/user'
import { getOverview, getRecommendations } from '@/api/mobile'
import { getPendingTasks } from '@/api/task'
import { getExams } from '@/api/exam'
import { getPointsRanking } from '@/api/points'
import { formatDate } from '@/utils/format'

const router = useRouter()
const userStore = useUserStore()

const userName = computed(() => userStore.userName || '同志')
const userBranch = computed(() => userStore.userBranch)
const userRole = computed(() => userStore.userRole)

const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour < 6) return '凌晨好'
  if (hour < 9) return '早上好'
  if (hour < 12) return '上午好'
  if (hour < 14) return '中午好'
  if (hour < 18) return '下午好'
  return '晚上好'
})

// 概览数据
const overview = reactive({
  pendingCount: 0,
  learningProgress: 0,
  totalPoints: 0
})
const overviewLoading = ref(false)

// 待办列表
const todoList = ref([])
const todoLoading = ref(false)

// AI 推荐
const recommendations = ref([])
const recommendLoading = ref(false)

// 积分排行
const rankingList = ref([])
const rankingLoading = ref(false)

function formatDeadline(deadline) {
  if (!deadline) return ''
  const now = new Date()
  const d = new Date(deadline)
  const diff = Math.ceil((d - now) / (1000 * 60 * 60 * 24))
  if (diff < 0) return '已过期'
  if (diff === 0) return '今天截止'
  if (diff === 1) return '明天截止'
  return `剩余${diff}天`
}

async function loadOverview() {
  overviewLoading.value = true
  try {
    const data = await getOverview()
    Object.assign(overview, {
      pendingCount: data.pendingCount ?? data.pendingTasks ?? 0,
      learningProgress: data.learningProgress ?? data.completionRate ?? 0,
      totalPoints: data.totalPoints ?? data.points ?? 0
    })
  } catch {
    // 错误已由拦截器处理
  } finally {
    overviewLoading.value = false
  }
}

async function loadTodos() {
  todoLoading.value = true
  try {
    const [tasksRes, examsRes] = await Promise.allSettled([
      getPendingTasks({ page: 1, size: 5 }),
      getExams({ page: 1, size: 5, status: 'pending' })
    ])

    const todos = []

    if (tasksRes.status === 'fulfilled') {
      const items = tasksRes.value?.items || tasksRes.value || []
      items.forEach(t => {
        todos.push({
          id: 'task-' + (t.id || t.taskId),
          type: 'task',
          title: t.title || t.name,
          deadline: t.deadline || t.endTime,
          taskId: t.id || t.taskId,
          contentId: t.contentId
        })
      })
    }

    if (examsRes.status === 'fulfilled') {
      const items = examsRes.value?.items || examsRes.value || []
      items.forEach(e => {
        if (e.status === 'pending' || !e.status) {
          todos.push({
            id: 'exam-' + (e.id || e.testId),
            type: 'exam',
            title: e.paperName || e.title || e.name,
            deadline: e.deadline || e.endTime,
            testId: e.id || e.testId
          })
        }
      })
    }

    todoList.value = todos.slice(0, 5)
  } catch {
    // 错误已由拦截器处理
  } finally {
    todoLoading.value = false
  }
}

async function loadRecommendations() {
  recommendLoading.value = true
  try {
    const data = await getRecommendations({ limit: 4 })
    recommendations.value = data?.contents || data?.items || data || []
  } catch {
    // 错误已由拦截器处理
  } finally {
    recommendLoading.value = false
  }
}

async function loadRanking() {
  rankingLoading.value = true
  try {
    const data = await getPointsRanking({ page: 1, size: 3 })
    rankingList.value = data?.items || data || []
  } catch {
    // 错误已由拦截器处理
  } finally {
    rankingLoading.value = false
  }
}

function handleTodoClick(item) {
  if (item.type === 'exam') {
    router.push(`/quiz/${item.testId}`)
  } else if (item.contentId) {
    router.push(`/content/${item.contentId}`)
  } else {
    router.push('/learning')
  }
}

function goContentDetail(id) {
  if (id) {
    router.push(`/content/${id}`)
  }
}

function goProfile() {
  router.push('/profile')
}

// 学习进度卡片 → AI学习报告（含学习进度详情）
function goReport() {
  router.push('/report')
}

onMounted(() => {
  if (!userStore.userInfo) {
    userStore.fetchUserInfo().catch(() => {})
  }
  loadOverview()
  loadTodos()
  loadRecommendations()
  loadRanking()
})
</script>

<style scoped>
.home-page {
  padding-bottom: 24px;
}

/* 欢迎语 */
.welcome {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 24px;
}

.welcome h1 {
  font-size: 24px;
  font-weight: 600;
}

.org-tag {
  font-size: 12px;
  padding: 4px 12px;
  border-radius: 12px;
}

/* 第一排：小统计卡片 + 待办提醒 */
.top-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 24px;
  align-items: start;
}

.top-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}

.mini-stat {
  background: var(--card);
  border-radius: var(--r10);
  padding: 14px 12px;
  box-shadow: var(--sh);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  text-align: center;
}

.mini-stat.clickable {
  cursor: pointer;
  transition: box-shadow 0.2s, transform 0.2s;
}

.mini-stat.clickable:hover {
  box-shadow: var(--sh-hover);
  transform: translateY(-2px);
}

.mini-icon {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.mini-icon.r { background: var(--red-10); color: var(--red); }
.mini-icon.g { background: var(--gold-50); color: #b07e0a; }
.mini-icon.b { background: rgba(30, 100, 200, 0.1); color: var(--blue); }

.mini-info { width: 100%; }

.mini-label {
  font-size: 12px;
  color: var(--t3);
  margin-bottom: 2px;
}

.mini-value {
  font-size: 20px;
  font-weight: 700;
}

.mini-value .unit {
  font-size: 12px;
  color: var(--t3);
  font-weight: 400;
  margin-left: 2px;
}

.top-todo {
  background: var(--card);
  border-radius: var(--r10);
  padding: 14px 16px;
  box-shadow: var(--sh);
}

.todo-list.compact {
  gap: 8px;
  margin-bottom: 0;
}

.todo-item.compact {
  padding: 10px 12px;
  gap: 10px;
}

.todo-icon.sm {
  width: 32px;
  height: 32px;
  border-radius: 8px;
}

.todo-title.sm {
  font-size: 13px;
  margin-bottom: 3px;
}

/* 统计卡片（保留旧类以防其他引用） */
.stats-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}

.stat-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 20px;
  box-shadow: var(--sh);
  display: flex;
  align-items: center;
  gap: 16px;
}

.stat-card.clickable {
  cursor: pointer;
  transition: box-shadow 0.2s, transform 0.2s;
}

.stat-card.clickable:hover {
  box-shadow: var(--sh-hover);
  transform: translateY(-2px);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-icon.r {
  background: var(--red-10);
  color: var(--red);
}

.stat-icon.g {
  background: var(--gold-50);
  color: #b07e0a;
}

.stat-icon.b {
  background: rgba(30, 100, 200, 0.1);
  color: var(--blue);
}

.stat-info {
  flex: 1;
}

.stat-label {
  font-size: 13px;
  color: var(--t3);
  margin-bottom: 4px;
}

.stat-value {
  font-size: 28px;
  font-weight: 700;
}

.stat-value .unit {
  font-size: 14px;
  color: var(--t3);
  font-weight: 400;
  margin-left: 4px;
}

.stat-progress {
  width: 100px;
}

/* 区块标题 */
.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.section-title {
  font-size: 16px;
  font-weight: 600;
}

.section-more {
  font-size: 13px;
  color: var(--red);
  cursor: pointer;
}

/* 待办卡片 */
.todo-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 24px;
  background: transparent;
  padding: 0;
}

.todo-item {
  background: var(--card);
  border-radius: var(--r10);
  padding: 16px 20px;
  box-shadow: var(--sh);
  display: flex;
  align-items: center;
  gap: 16px;
  transition: box-shadow 0.2s;
  cursor: pointer;
}

.todo-item:hover {
  box-shadow: var(--sh-hover);
}

.todo-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.todo-icon.r {
  background: var(--red-10);
  color: var(--red);
}

.todo-icon.b {
  background: rgba(30, 100, 200, 0.1);
  color: var(--blue);
}

.todo-content {
  flex: 1;
}

.todo-title {
  font-size: 15px;
  font-weight: 500;
  margin-bottom: 6px;
}

.todo-meta {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* 第二排：AI推荐 + 积分排行 并列 */
.bottom-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 24px;
  align-items: start;
}

.bottom-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 14px 16px;
  box-shadow: var(--sh);
}

/* AI推荐 */
.ai-section {
  margin-bottom: 24px;
}

.ai-section.compact {
  margin-bottom: 0;
}

.ai-cards {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.ai-cards.compact {
  grid-template-columns: 1fr;
  gap: 8px;
}

.ai-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 20px;
  box-shadow: var(--sh);
  display: flex;
  align-items: center;
  gap: 16px;
  border-left: 4px solid var(--red);
  transition: all 0.2s;
  cursor: pointer;
}

.ai-card.compact {
  background: transparent;
  box-shadow: none;
  border-left: none;
  padding: 8px 10px;
  gap: 10px;
  border-radius: 8px;
}

.ai-card.compact:hover {
  background: var(--red-5);
}

.ai-card:hover {
  box-shadow: var(--sh-hover);
  transform: translateY(-1px);
}

.ai-card.compact:hover {
  transform: none;
  box-shadow: none;
}

.ai-card-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  flex-shrink: 0;
}

.ai-card-icon.sm {
  width: 32px;
  height: 32px;
  border-radius: 8px;
}

.ai-card-icon.v {
  background: linear-gradient(135deg, var(--red), var(--red-d));
}

.ai-card-icon.d {
  background: linear-gradient(135deg, #1a5276, #154360);
}

.ai-card-content {
  flex: 1;
  min-width: 0;
}

.ai-card-title {
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 4px;
}

.ai-card-title.sm {
  font-size: 13px;
  font-weight: 500;
  margin-bottom: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.ai-card-desc {
  font-size: 12px;
  color: var(--t3);
}

/* 排行榜 */
.rank-section {
  margin-bottom: 24px;
}

.rank-section.compact {
  margin-bottom: 0;
}

.rank-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.rank-list.compact {
  gap: 6px;
}

.rank-item {
  background: var(--card);
  border-radius: var(--r10);
  padding: 14px 18px;
  box-shadow: var(--sh);
  display: flex;
  align-items: center;
  gap: 14px;
}

.rank-item.compact {
  background: transparent;
  box-shadow: none;
  padding: 8px 10px;
  gap: 10px;
  border-radius: 8px;
}

.rank-item.compact:hover {
  background: var(--red-5);
}

.rank-badge {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 700;
  flex-shrink: 0;
  color: #fff;
}

.rank-badge.sm {
  width: 26px;
  height: 26px;
  font-size: 12px;
}

.rank-badge.g1 {
  background: linear-gradient(135deg, #ffd700, #ffa500);
  color: #5a3a00;
}

.rank-badge.g2 {
  background: linear-gradient(135deg, #e8e8e8, #c0c0c0);
  color: #333;
}

.rank-badge.g3 {
  background: linear-gradient(135deg, #e8a87c, #cd7f32);
  color: #fff;
}

.rank-info {
  flex: 1;
}

.rank-name {
  font-size: 15px;
  font-weight: 500;
}

.rank-name.sm {
  font-size: 13px;
}

.rank-org {
  font-size: 12px;
  color: var(--t3);
  margin-top: 2px;
}

.rank-score {
  font-size: 18px;
  font-weight: 700;
  color: var(--red);
}

.rank-score.sm {
  font-size: 15px;
}

.rank-score span {
  font-size: 12px;
  color: var(--t3);
  font-weight: 400;
}

@media (max-width: 1200px) {
  .top-row {
    grid-template-columns: 1fr;
  }

  .bottom-row {
    grid-template-columns: 1fr;
  }

  .stats-row {
    grid-template-columns: 1fr;
  }

  .ai-cards {
    grid-template-columns: 1fr;
  }
}
</style>
