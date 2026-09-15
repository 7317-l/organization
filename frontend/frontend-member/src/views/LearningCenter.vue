<template>
  <div class="learning-center">
    <el-tabs v-model="activeTab" class="learning-tabs" @tab-change="handleTabChange">
      <!-- 学习内容 -->
      <el-tab-pane label="学习内容" name="materials">
        <div class="filter-bar">
          <el-input v-model="searchKeyword" placeholder="搜索内容" clearable @keyup.enter="handleSearch" style="width: 240px">
            <template #prefix><el-icon><Search /></el-icon></template>
          </el-input>
          <el-select v-model="filterType" placeholder="全部类型" clearable style="width: 120px" @change="handleSearch">
            <el-option label="文章" value="article" />
            <el-option label="视频" value="video" />
          </el-select>
          <el-select v-model="sortBy" placeholder="排序" style="width: 120px" @change="handleSearch">
            <el-option label="最新发布" value="latest" />
            <el-option label="最多学习" value="popular" />
          </el-select>
        </div>

        <div v-loading="materialsLoading" class="material-list">
          <div v-for="item in materials" :key="item.id" class="material-card" @click="goDetail(item.id)">
            <div class="material-icon" :class="isVideoContent(item) ? 'v' : 'd'">
              <el-icon :size="28"><VideoPlay v-if="isVideoContent(item)" /><Document v-else /></el-icon>
            </div>
            <div class="material-info">
              <div class="material-title">{{ item.title }}</div>
              <div class="material-tags">
                <el-tag v-for="tag in item.tags || []" :key="tag" size="small" effect="light">{{ tag }}</el-tag>
                <el-tag v-if="item.categoryName" size="small" type="info" effect="light">{{ item.categoryName }}</el-tag>
              </div>
              <div class="material-date">{{ formatDate(item.createdAt) }}</div>
            </div>
            <div class="material-status">
              <div v-if="item.progress !== undefined" class="material-progress">
                <el-progress :percentage="item.progress" :stroke-width="6" :show-text="false" />
                <div class="progress-text">{{ item.progress }}%</div>
              </div>
              <div :class="['status-text', (item.status === 'done' || item.isCompleted) ? 'status-done' : 'status-new']">
                {{ getActionText(item) }}
              </div>
            </div>
          </div>
          <el-empty v-if="!materialsLoading && materials.length === 0" description="暂无学习内容" :image-size="80" />
        </div>

        <div class="pagination" v-if="materialsTotal > 0">
          <el-pagination
            v-model:current-page="materialsPage"
            v-model:page-size="materialsSize"
            :total="materialsTotal"
            layout="prev, pager, next"
            @current-change="loadMaterials"
          />
        </div>
      </el-tab-pane>

      <!-- 支部任务 -->
      <el-tab-pane label="支部任务" name="tasks">
        <el-tabs v-model="taskSubTab" class="sub-tabs">
          <el-tab-pane label="待完成" name="pending">
            <div v-loading="pendingTasksLoading" class="material-list">
              <div v-for="item in pendingTasks" :key="item.id" class="material-card" @click="viewTaskDetail(item)">
                <div class="material-icon v">
                  <el-icon :size="28"><List /></el-icon>
                </div>
                <div class="material-info">
                  <div class="material-title">{{ item.taskName || item.title }}</div>
                  <div class="material-tags">
                    <el-tag size="small" type="warning" effect="light">{{ formatDeadline(item.deadline) }}</el-tag>
                  </div>
                  <div class="material-date">{{ item.contentCount || 0 }} 个学习内容</div>
                </div>
                <div class="material-status">
                  <div class="status-new">开始学习 →</div>
                </div>
              </div>
              <el-empty v-if="!pendingTasksLoading && pendingTasks.length === 0" description="暂无待完成任务" :image-size="80" />
            </div>
          </el-tab-pane>

          <el-tab-pane label="已完成" name="completed">
            <div v-loading="completedTasksLoading" class="material-list">
              <div v-for="item in completedTasks" :key="item.id" class="material-card" @click="viewTaskDetail(item)">
                <div class="material-icon d">
                  <el-icon :size="28"><List /></el-icon>
                </div>
                <div class="material-info">
                  <div class="material-title">{{ item.taskName || item.title }}</div>
                  <div class="material-tags">
                    <el-tag size="small" type="success" effect="light">已完成</el-tag>
                  </div>
                  <div class="material-date">{{ formatDate(item.completedAt || item.updatedAt) }}</div>
                </div>
                <div class="material-status">
                  <div class="status-done">查看详情 →</div>
                </div>
              </div>
              <el-empty v-if="!completedTasksLoading && completedTasks.length === 0" description="暂无已完成任务" :image-size="80" />
            </div>
          </el-tab-pane>
        </el-tabs>
      </el-tab-pane>

      <!-- AI学习路线 -->
      <el-tab-pane label="AI学习路线" name="ai">
        <div class="ai-route-section">
          <el-button type="primary" :loading="generating" @click="generateAiRoute" style="margin-bottom: 16px">
            <el-icon><MagicStick /></el-icon>
            生成我的学习路线
          </el-button>

          <div v-loading="aiRouteLoading" class="material-list">
            <div v-for="(item, index) in aiRoutes" :key="item.id || index" class="material-card" @click="goDetail(item.id || item.contentId)">
              <div class="material-icon" :class="isVideoContent(item) ? 'v' : 'd'">
                <el-icon :size="28"><VideoPlay v-if="isVideoContent(item)" /><Document v-else /></el-icon>
              </div>
              <div class="material-info">
                <div class="material-title">{{ index + 1 }}. {{ item.title }}</div>
                <div class="material-tags">
                  <el-tag v-for="tag in item.tags || []" :key="tag" size="small" effect="light">{{ tag }}</el-tag>
                  <el-tag size="small" type="primary" effect="light">AI推荐</el-tag>
                </div>
                <div class="material-date">{{ item.reason || item.description || '基于你的学习情况推荐' }}</div>
              </div>
              <div class="material-status">
                <div class="status-new">开始学习 →</div>
              </div>
            </div>
            <el-empty v-if="!aiRouteLoading && aiRoutes.length === 0" description="点击上方按钮生成AI学习路线" :image-size="80" />
          </div>
        </div>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import { ref, onMounted, inject } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Search, VideoPlay, Document, List, MagicStick, Trophy } from '@element-plus/icons-vue'
import { getContents } from '@/api/content'
import { getPendingTasks, getCompletedTasks } from '@/api/task'
import { getRecommendations, getAiAssessment } from '@/api/mobile'
import { formatDate, isVideoContent } from '@/utils/format'

const openAIPanel = inject('openAIPanel')
function openAI(key) { openAIPanel?.(key) }

const router = useRouter()

const activeTab = ref('materials')
const searchKeyword = ref('')
const filterType = ref('')
const sortBy = ref('latest')

// 公共素材
const materials = ref([])
const materialsLoading = ref(false)
const materialsPage = ref(1)
const materialsSize = ref(10)
const materialsTotal = ref(0)

// 支部任务
const taskSubTab = ref('pending')
const pendingTasks = ref([])
const pendingTasksLoading = ref(false)
const completedTasks = ref([])
const completedTasksLoading = ref(false)

// AI路线
const aiRoutes = ref([])
const aiRouteLoading = ref(false)
const generating = ref(false)

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

function getActionText(item) {
  if (item.progress !== undefined && item.progress > 0 && item.progress < 100) return '继续学习 →'
  if (item.progress === 100 || item.status === 'completed') return '重新阅读 →'
  return '开始学习 →'
}

async function loadMaterials() {
  materialsLoading.value = true
  try {
    const params = {
      page: materialsPage.value,
      size: materialsSize.value
    }
    if (filterType.value) params.type = filterType.value
    if (searchKeyword.value) params.keyword = searchKeyword.value
    if (sortBy.value) params.sort = sortBy.value

    const data = await getContents(params)
    materials.value = data?.items || data || []
    materialsTotal.value = data?.total || materials.value.length
  } catch {
    // 错误已由拦截器处理
  } finally {
    materialsLoading.value = false
  }
}

async function loadPendingTasks() {
  pendingTasksLoading.value = true
  try {
    const data = await getPendingTasks({ page: 1, size: 50 })
    pendingTasks.value = data?.items || data || []
  } catch {
    // 错误已由拦截器处理
  } finally {
    pendingTasksLoading.value = false
  }
}

async function loadCompletedTasks() {
  completedTasksLoading.value = true
  try {
    const data = await getCompletedTasks({ page: 1, size: 50 })
    completedTasks.value = data?.items || data || []
  } catch {
    // 错误已由拦截器处理
  } finally {
    completedTasksLoading.value = false
  }
}

async function generateAiRoute() {
  generating.value = true
  aiRouteLoading.value = true
  try {
    // 优先调用 AI 评估生成学习路线
    const assessmentData = await getAiAssessment({})
    if (assessmentData?.learningPath || assessmentData?.routes || assessmentData?.recommendations) {
      aiRoutes.value = assessmentData.learningPath || assessmentData.routes || assessmentData.recommendations || []
    } else {
      // 降级使用推荐接口
      const recData = await getRecommendations({ limit: 10 })
      aiRoutes.value = recData?.contents || recData?.items || recData || []
    }
    ElMessage.success('学习路线生成成功')
  } catch {
    // 错误已由拦截器处理
  } finally {
    generating.value = false
    aiRouteLoading.value = false
  }
}

function goBattle() { router.push('/battle') }

function handleTabChange(name) {
  if (name === 'materials') {
    loadMaterials()
  } else if (name === 'tasks') {
    loadPendingTasks()
    loadCompletedTasks()
  }
}

function handleSearch() {
  materialsPage.value = 1
  loadMaterials()
}

function goDetail(id) {
  if (id) router.push(`/content/${id}`)
}

function goLearning() {
  activeTab.value = 'materials'
}

function viewTaskDetail(item) {
  if (item.contentId) {
    goDetail(item.contentId)
  } else {
    ElMessage.info('该任务暂无关联内容')
  }
}

onMounted(() => {
  loadMaterials()
})
</script>

<style scoped>
.learning-center {
  padding-bottom: 24px;
}

.learning-tabs {
  margin-bottom: 0;
}

.learning-tabs :deep(.el-tabs__item) {
  font-size: 15px;
  height: 48px;
  line-height: 48px;
}

.sub-tabs {
  margin-top: 16px;
}

.sub-tabs :deep(.el-tabs__item) {
  font-size: 14px;
  height: 40px;
  line-height: 40px;
}

.filter-bar {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}

.filter-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: var(--t2);
}

.material-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.material-card {
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

.material-card:hover {
  box-shadow: var(--sh-hover);
}

.material-icon {
  width: 64px;
  height: 64px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  color: #fff;
}

.material-icon.v {
  background: linear-gradient(135deg, var(--red), var(--red-d));
}

.material-icon.d {
  background: linear-gradient(135deg, #2c3e50, #1a252f);
}

.material-info {
  flex: 1;
  min-width: 0;
}

.material-title {
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 8px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.material-tags {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
  flex-wrap: wrap;
}

.tag-item {
  margin-right: 0;
}

.material-date {
  font-size: 12px;
  color: var(--t3);
}

.material-status {
  flex-shrink: 0;
  text-align: right;
  min-width: 100px;
}

.material-progress {
  width: 120px;
}

.progress-text {
  font-size: 12px;
  color: var(--t3);
  margin-top: 4px;
  text-align: right;
}

.status-done {
  color: var(--green);
  font-weight: 600;
}

.status-new {
  color: var(--t3);
}

.material-action {
  flex-shrink: 0;
}

.pagination-wrap {
  display: flex;
  justify-content: center;
  margin-top: 24px;
}

.ai-route-section {
  margin-top: 8px;
}

.ai-route-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  padding: 16px 20px;
  background: var(--card);
  border-radius: var(--r10);
  box-shadow: var(--sh);
}

.ai-route-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
  font-weight: 600;
}

.battle-embed { text-align: center; padding: 40px 0; }
</style>
