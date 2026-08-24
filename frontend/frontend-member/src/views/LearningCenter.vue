<template>
  <div class="learning-center">
    <div class="page-header">
      <div class="page-title">学习中心</div>
      <div class="page-actions">
        <el-input
          v-model="searchKeyword"
          placeholder="搜索素材..."
          clearable
          style="width: 280px"
          @keyup.enter="handleSearch"
          @clear="handleSearch"
        >
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>
      </div>
    </div>

    <el-tabs v-model="activeTab" class="learning-tabs" @tab-change="handleTabChange">
      <!-- Tab1 公共素材 -->
      <el-tab-pane label="公共素材" name="materials">
        <div class="filter-bar">
          <div class="filter-item">
            <span>分类:</span>
            <el-select v-model="filterType" placeholder="全部" style="width: 120px" @change="loadMaterials">
              <el-option label="全部" value="" />
              <el-option label="视频" value="video" />
              <el-option label="文章" value="article" />
            </el-select>
          </div>
          <div class="filter-item">
            <span>排序:</span>
            <el-select v-model="sortBy" placeholder="最新" style="width: 120px" @change="loadMaterials">
              <el-option label="最新" value="latest" />
              <el-option label="最热" value="hot" />
            </el-select>
          </div>
        </div>

        <div class="material-list" v-loading="materialsLoading">
          <template v-if="materials.length > 0">
            <div
              v-for="item in materials"
              :key="item.id"
              class="material-card"
              @click="goDetail(item.id)"
            >
              <div class="material-icon" :class="item.contentType === 1 ? 'v' : 'd'">
                <el-icon :size="28"><VideoPlay v-if="item.contentType === 1" /><Document v-else /></el-icon>
              </div>
              <div class="material-info">
                <div class="material-title">{{ item.title }}</div>
                <div class="material-tags">
                  <el-tag v-for="tag in (item.tags || [])" :key="tag" size="small" effect="light" class="tag-item">
                    {{ tag }}
                  </el-tag>
                  <el-tag v-if="item.contentTypeName" size="small" type="danger" effect="light">
                    {{ item.contentTypeName === 'Video' ? '视频' : '文章' }}
                  </el-tag>
                </div>
                <div class="material-date">{{ formatDate(item.createdAt || item.date) }}</div>
              </div>
              <div class="material-status">
                <template v-if="item.progress !== undefined && item.progress > 0 && item.progress < 100">
                  <div class="material-progress">
                    <el-progress :percentage="item.progress" :show-text="false" :stroke-width="6" color="#C8161D" />
                    <div class="progress-text">{{ item.progress }}%</div>
                  </div>
                </template>
                <template v-else-if="item.progress === 100 || item.status === 'done'">
                  <div class="status-done">已读完</div>
                </template>
                <template v-else>
                  <div class="status-new">未学</div>
                </template>
              </div>
              <div class="material-action">
                <el-button type="primary" plain size="small" @click.stop="goDetail(item.id)">
                  {{ getActionText(item) }}
                </el-button>
              </div>
            </div>
          </template>
          <el-empty v-else description="暂无学习内容" :image-size="100" />
        </div>

        <div class="pagination-wrap" v-if="materialsTotal > 0">
          <el-pagination
            v-model:current-page="materialsPage"
            v-model:page-size="materialsSize"
            :total="materialsTotal"
            :page-sizes="[10, 20, 50]"
            layout="total, sizes, prev, pager, next, jumper"
            background
            @size-change="loadMaterials"
            @current-change="loadMaterials"
          />
        </div>
      </el-tab-pane>

      <!-- Tab2 支部专属任务 -->
      <el-tab-pane label="支部专属任务" name="tasks">
        <el-tabs v-model="taskSubTab" class="sub-tabs">
          <el-tab-pane label="待完成" name="pending">
            <div class="material-list" v-loading="pendingTasksLoading">
              <template v-if="pendingTasks.length > 0">
                <div v-for="item in pendingTasks" :key="item.id" class="material-card">
                  <div class="material-icon v">
                    <el-icon :size="28"><List /></el-icon>
                  </div>
                  <div class="material-info">
                    <div class="material-title">{{ item.title || item.name }}</div>
                    <div class="material-tags">
                      <el-tag size="small" type="danger" effect="light">支部任务</el-tag>
                      <el-tag v-if="item.deadline" size="small" type="warning" effect="light">
                        {{ formatDeadline(item.deadline) }}
                      </el-tag>
                    </div>
                    <div class="material-date">
                      发布人: {{ item.publisher || item.creatorName || '支部书记' }}
                      <span v-if="item.contentCount"> · 包含{{ item.contentCount }}个素材</span>
                    </div>
                  </div>
                  <div class="material-status">
                    <div class="material-progress" v-if="item.progress !== undefined">
                      <el-progress :percentage="item.progress" :show-text="false" :stroke-width="6" color="#C8161D" />
                      <div class="progress-text">{{ item.progress }}%</div>
                    </div>
                  </div>
                  <div class="material-action">
                    <el-button
                      v-if="item.contentId"
                      type="primary"
                      size="small"
                      @click="goDetail(item.contentId)"
                    >
                      继续学习 →
                    </el-button>
                    <el-button v-else type="primary" plain size="small" @click="viewTaskDetail(item)">
                      查看详情
                    </el-button>
                  </div>
                </div>
              </template>
              <el-empty v-else description="暂无待完成任务" :image-size="100" />
            </div>
          </el-tab-pane>

          <el-tab-pane label="已完成" name="completed">
            <div class="material-list" v-loading="completedTasksLoading">
              <template v-if="completedTasks.length > 0">
                <div v-for="item in completedTasks" :key="item.id" class="material-card">
                  <div class="material-icon v">
                    <el-icon :size="28"><List /></el-icon>
                  </div>
                  <div class="material-info">
                    <div class="material-title">{{ item.title || item.name }}</div>
                    <div class="material-tags">
                      <el-tag size="small" type="danger" effect="light">支部任务</el-tag>
                      <el-tag size="small" type="success" effect="light">已完成</el-tag>
                    </div>
                    <div class="material-date">
                      完成时间: {{ formatDate(item.completedAt || item.finishTime) }}
                    </div>
                  </div>
                  <div class="material-status">
                    <div class="status-done">已完成</div>
                  </div>
                  <div class="material-action">
                    <el-button
                      v-if="item.contentId"
                      type="primary"
                      plain
                      size="small"
                      @click="goDetail(item.contentId)"
                    >
                      查看详情
                    </el-button>
                  </div>
                </div>
              </template>
              <el-empty v-else description="暂无已完成任务" :image-size="100" />
            </div>
          </el-tab-pane>
        </el-tabs>
      </el-tab-pane>

      <!-- Tab3 AI定制学习路线 -->
      <el-tab-pane label="AI定制学习路线" name="ai">
        <div class="ai-route-section" v-loading="aiRouteLoading">
          <div class="ai-route-header">
            <div class="ai-route-title">
              <el-icon :size="20" color="#C8161D"><MagicStick /></el-icon>
              AI 定制学习路线
            </div>
            <el-button type="primary" :loading="generating" @click="generateAiRoute">
              {{ aiRoutes.length > 0 ? '重新生成' : '生成学习路线' }}
            </el-button>
          </div>

          <div class="material-list" v-if="aiRoutes.length > 0">
            <div v-for="(route, index) in aiRoutes" :key="index" class="material-card">
              <div class="material-icon v">
                <el-icon :size="28"><MagicStick /></el-icon>
              </div>
              <div class="material-info">
                <div class="material-title">{{ route.title || route.stageName || `阶段${index + 1}` }}</div>
                <div class="material-tags">
                  <el-tag size="small" type="primary" effect="light">AI定制</el-tag>
                  <el-tag v-if="route.knowledgePoint" size="small" effect="light">
                    {{ route.knowledgePoint }}
                  </el-tag>
                </div>
                <div class="material-date">
                  <span v-if="route.contentCount">推荐素材：{{ route.contentCount }}个</span>
                  <span v-if="route.estimatedTime"> · 预计用时：{{ route.estimatedTime }}</span>
                </div>
              </div>
              <div class="material-status">
                <div class="material-progress" v-if="route.mastery !== undefined">
                  <el-progress :percentage="route.mastery" :show-text="false" :stroke-width="6" color="#C8161D" />
                  <div class="progress-text">掌握度 {{ route.mastery }}%</div>
                </div>
              </div>
              <div class="material-action">
                <el-button
                  v-if="route.contentId || route.contents?.length"
                  type="primary"
                  size="small"
                  @click="goDetail(route.contentId || route.contents[0]?.id)"
                >
                  开始学习 →
                </el-button>
                <el-button v-else type="primary" plain size="small" @click="goLearning">
                  去学习中心
                </el-button>
              </div>
            </div>
          </div>

          <el-empty v-else description="点击上方按钮生成AI定制学习路线" :image-size="120" />
        </div>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Search, VideoPlay, Document, List, MagicStick } from '@element-plus/icons-vue'
import { getContents } from '@/api/content'
import { getPendingTasks, getCompletedTasks } from '@/api/task'
import { getRecommendations, getAiAssessment } from '@/api/mobile'
import { formatDate } from '@/utils/format'

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
</style>
