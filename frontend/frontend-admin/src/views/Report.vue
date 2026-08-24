<template>
  <div class="report-page">
    <div class="page-header">
      <div class="page-title">AI 学习报告</div>
      <el-tag type="danger" effect="dark" class="report-badge">AI 分析</el-tag>
    </div>

    <div v-loading="loading">
      <!-- 未生成时的空状态 -->
      <div v-if="!reportData" class="report-empty">
        <el-empty description="点击下方按钮生成你的 AI 学习报告" :image-size="120" />
        <el-button type="primary" size="large" :loading="generating" @click="generateReport">
          生成 AI 学习报告
        </el-button>
      </div>

      <template v-else>
        <!-- 个人信息 -->
        <div class="report-personal">
          <div class="report-avatar">{{ avatarChar }}</div>
          <div class="report-personal-info">
            <h3>{{ userName }}</h3>
            <el-tag type="danger" effect="light" size="small">
              {{ userBranch }} · {{ userRole || '党员' }}
            </el-tag>
          </div>
          <div class="report-personal-stats">
            <div class="profile-stat">
              <div class="profile-stat-value gold">{{ reportData.totalPoints || overview.totalPoints || 0 }}</div>
              <div class="profile-stat-label">累计积分（分）</div>
            </div>
            <div class="profile-stat">
              <div class="profile-stat-value" :style="{ color: levelColor }">{{ levelText }}</div>
              <div class="profile-stat-label">评级</div>
            </div>
          </div>
        </div>

        <!-- 综合评分 + 雷达图 -->
        <div class="report-score-section">
          <div class="report-score-card">
            <div class="report-score-value">{{ reportData.score || reportData.overallScore || 0 }}</div>
            <div class="report-score-label">综合评分</div>
            <div class="report-score-level" :style="{ color: levelColor }">{{ levelText }}</div>
          </div>
          <div class="report-radar-card">
            <div class="report-radar-title">学习维度</div>
            <div ref="radarRef" class="report-radar"></div>
          </div>
        </div>

        <!-- 维度分析 -->
        <div class="report-dimensions">
          <h4>学习维度分析</h4>
          <div
            v-for="(dim, idx) in dimensions"
            :key="idx"
            class="dimension-item"
          >
            <div class="dimension-name">{{ dim.name }}</div>
            <div class="dimension-progress">
              <el-progress :percentage="dim.value" :show-text="false" :stroke-width="8" color="#C8161D" />
            </div>
            <div class="dimension-value">{{ dim.value }}%</div>
            <div class="dimension-level" :class="dim.levelClass">{{ dim.levelText }}</div>
          </div>
        </div>

        <!-- AI 评语 -->
        <div class="report-ai">
          <div class="report-ai-title">
            <el-icon :size="18"><MagicStick /></el-icon>
            AI 评语：
          </div>
          <div class="report-ai-text">{{ reportData.comment || reportData.assessment || reportData.aiComment || '暂无评语' }}</div>
        </div>

        <!-- 改进建议 -->
        <div class="report-suggestions" v-if="suggestions.length > 0">
          <div class="report-suggestions-title">改进建议：</div>
          <ol>
            <li v-for="(s, idx) in suggestions" :key="idx">{{ s }}</li>
          </ol>
        </div>

        <!-- 底部按钮 -->
        <div class="report-footer">
          <el-button type="primary" size="large" :loading="generating" @click="generateReport">
            重新生成报告
          </el-button>
          <el-button type="success" size="large" @click="generateLearningPath">
            生成学习路线
          </el-button>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { MagicStick } from '@element-plus/icons-vue'
import * as echarts from 'echarts'
import { getAiAssessment, getRecommendations, getOverview } from '@/api/mobile'
import { useUserStore } from '@/stores/user'
import { getAvatarChar } from '@/utils/format'

const router = useRouter()
const userStore = useUserStore()

const loading = ref(false)
const generating = ref(false)
const reportData = ref(null)
const overview = ref({ totalPoints: 0 })
const radarRef = ref(null)
let radarChart = null

const userName = computed(() => userStore.userName || '用户')
const userBranch = computed(() => userStore.userBranch || '')
const userRole = computed(() => userStore.userRole || '')
const avatarChar = computed(() => getAvatarChar(userName.value))

const score = computed(() => reportData.value?.score || reportData.value?.overallScore || 0)

const levelColor = computed(() => {
  const s = score.value
  if (s >= 90) return 'var(--green)'
  if (s >= 75) return 'var(--blue)'
  if (s >= 60) return 'var(--orange)'
  return 'var(--red)'
})

const levelText = computed(() => {
  const s = score.value
  if (s >= 90) return '优秀'
  if (s >= 75) return '良好'
  if (s >= 60) return '一般'
  return '待提升'
})

// 维度数据
const dimensions = computed(() => {
  const data = reportData.value?.dimensions || reportData.value?.dimensionScores || {}
  const defaultDims = [
    { key: 'frequency', name: '学习频率', value: 0 },
    { key: 'score', name: '测验得分', value: 0 },
    { key: 'mastery', name: '知识掌握', value: 0 },
    { key: 'participation', name: '互动参与', value: 0 },
    { key: 'completion', name: '完成率', value: 0 }
  ]

  return defaultDims.map(dim => {
    let value = 0
    if (typeof data === 'object' && data !== null) {
      if (Array.isArray(data)) {
        const found = data.find(d => d.name === dim.name || d.key === dim.key)
        value = found?.value || found?.score || 0
      } else {
        value = data[dim.key] || data[dim.name] || 0
      }
    }
    value = Math.round(value)
    let levelClass = 'normal'
    let levelText = '一般'
    if (value >= 90) { levelClass = 'excellent'; levelText = '优秀' }
    else if (value >= 75) { levelClass = 'good'; levelText = '良好' }
    else if (value >= 60) { levelClass = 'normal'; levelText = '一般' }
    else { levelClass = 'poor'; levelText = '待提升' }

    return { ...dim, value, levelClass, levelText }
  })
})

const suggestions = computed(() => {
  const s = reportData.value?.suggestions || reportData.value?.improvementSuggestions || []
  if (Array.isArray(s)) return s
  if (typeof s === 'string') return s.split('\n').filter(Boolean)
  return []
})

function initRadarChart() {
  if (!radarRef.value || !reportData.value) return

  if (radarChart) {
    radarChart.dispose()
  }

  radarChart = echarts.init(radarRef.value)

  const dims = dimensions.value
  const option = {
    radar: {
      indicator: dims.map(d => ({ name: d.name, max: 100 })),
      shape: 'polygon',
      splitNumber: 4,
      axisName: {
        color: '#555',
        fontSize: 12
      },
      splitLine: {
        lineStyle: { color: '#E8E8EC' }
      },
      splitArea: {
        areaStyle: { color: ['rgba(200, 22, 29, 0.02)', 'rgba(200, 22, 29, 0.05)'] }
      },
      axisLine: {
        lineStyle: { color: '#E8E8EC' }
      }
    },
    series: [
      {
        type: 'radar',
        data: [
          {
            value: dims.map(d => d.value),
            name: '学习能力',
            areaStyle: { color: 'rgba(200, 22, 29, 0.15)' },
            lineStyle: { color: '#C8161D', width: 2 },
            itemStyle: { color: '#C8161D' }
          }
        ]
      }
    ]
  }

  radarChart.setOption(option)
}

async function generateReport() {
  generating.value = true
  loading.value = true
  try {
    const data = await getAiAssessment({})
    reportData.value = data || {}
    ElMessage.success('学习报告生成成功')
    await nextTick()
    initRadarChart()
  } catch {
    // 错误已由拦截器处理
  } finally {
    generating.value = false
    loading.value = false
  }
}

async function generateLearningPath() {
  try {
    ElMessage.info('正在生成学习路线...')
    await getRecommendations({ page: 1, size: 10 })
    ElMessage.success('学习路线已生成，前往学习中心查看')
    router.push('/learning')
  } catch {
    // 错误已由拦截器处理
  }
}

async function loadOverview() {
  try {
    const data = await getOverview()
    overview.value = data || {}
  } catch {
    // 错误已由拦截器处理
  }
}

watch(reportData, () => {
  nextTick(() => {
    initRadarChart()
  })
})

onMounted(() => {
  if (!userStore.userInfo) {
    userStore.fetchUserInfo().catch(() => {})
  }
  loadOverview()
  // 自动生成报告
  generateReport()

  // 响应式调整
  window.addEventListener('resize', () => {
    radarChart?.resize()
  })
})
</script>

<style scoped>
.report-page {
  padding-bottom: 40px;
}

.report-badge {
  font-size: 12px;
  padding: 4px 12px;
  border-radius: 12px;
}

.report-empty {
  text-align: center;
  padding: 60px 0;
}

.report-empty .el-button {
  margin-top: 20px;
}

/* 个人信息 */
.report-personal {
  background: var(--card);
  border-radius: var(--r10);
  padding: 24px;
  box-shadow: var(--sh);
  display: flex;
  align-items: center;
  gap: 24px;
  margin-bottom: 20px;
}

.report-avatar {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--red), var(--red-d));
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  font-weight: 600;
  color: #fff;
  flex-shrink: 0;
}

.report-personal-info h3 {
  font-size: 18px;
  font-weight: 600;
  margin-bottom: 6px;
}

.report-personal-stats {
  margin-left: auto;
  display: flex;
  gap: 40px;
}

.profile-stat {
  text-align: center;
}

.profile-stat-value {
  font-size: 24px;
  font-weight: 700;
}

.profile-stat-value.gold {
  color: var(--gold);
}

.profile-stat-label {
  font-size: 12px;
  color: var(--t3);
  margin-top: 4px;
}

/* 评分区 */
.report-score-section {
  display: flex;
  gap: 20px;
  margin-bottom: 20px;
}

.report-score-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 32px;
  box-shadow: var(--sh);
  text-align: center;
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.report-score-value {
  font-size: 56px;
  font-weight: 700;
  color: var(--red);
  line-height: 1;
}

.report-score-label {
  font-size: 14px;
  color: var(--t3);
  margin-top: 8px;
}

.report-score-level {
  display: inline-block;
  margin-top: 12px;
  padding: 4px 16px;
  background: rgba(46, 139, 87, 0.1);
  border-radius: 12px;
  font-size: 13px;
  font-weight: 500;
}

.report-radar-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 24px;
  box-shadow: var(--sh);
  flex: 2;
}

.report-radar-title {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 8px;
  text-align: center;
}

.report-radar {
  width: 100%;
  height: 280px;
}

/* 维度分析 */
.report-dimensions {
  background: var(--card);
  border-radius: var(--r10);
  padding: 24px;
  box-shadow: var(--sh);
  margin-bottom: 20px;
}

.report-dimensions h4 {
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 16px;
}

.dimension-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 12px 0;
  border-bottom: 1px solid var(--bd);
}

.dimension-item:last-child {
  border-bottom: none;
}

.dimension-name {
  width: 100px;
  font-size: 14px;
  flex-shrink: 0;
}

.dimension-progress {
  flex: 1;
}

.dimension-value {
  width: 60px;
  text-align: right;
  font-size: 14px;
  font-weight: 600;
  flex-shrink: 0;
}

.dimension-level {
  width: 60px;
  text-align: right;
  font-size: 13px;
  flex-shrink: 0;
}

.dimension-level.excellent {
  color: var(--green);
}

.dimension-level.good {
  color: var(--blue);
}

.dimension-level.normal {
  color: var(--orange);
}

.dimension-level.poor {
  color: var(--red);
}

/* AI 评语 */
.report-ai {
  background: var(--bg);
  border-radius: var(--r10);
  padding: 20px;
  margin-bottom: 20px;
}

.report-ai-title {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 12px;
  display: flex;
  align-items: center;
  gap: 6px;
  color: var(--red);
}

.report-ai-text {
  font-size: 14px;
  color: var(--t2);
  line-height: 1.8;
}

/* 改进建议 */
.report-suggestions {
  background: var(--card);
  border-radius: var(--r10);
  padding: 20px;
  box-shadow: var(--sh);
  margin-bottom: 24px;
}

.report-suggestions-title {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 12px;
}

.report-suggestions ol {
  padding-left: 20px;
}

.report-suggestions li {
  font-size: 13px;
  color: var(--t2);
  line-height: 2;
  margin-bottom: 8px;
}

/* 底部 */
.report-footer {
  display: flex;
  gap: 16px;
  justify-content: center;
}

.report-footer .el-button {
  min-width: 180px;
  height: 48px;
  font-size: 15px;
}
</style>
