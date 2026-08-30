<template>
  <div class="dashboard">
    <!-- 5张KPI卡片（flex均分） -->
    <div class="kpi-row" v-loading="loading">
      <div class="kpi-flex">
        <div class="kpi-col" v-for="(item, idx) in kpiList" :key="idx">
          <el-card class="kpi-card" shadow="hover">
            <div class="kpi-inner">
              <div class="kpi-icon" :style="{ background: item.bgColor, color: item.iconColor }">
                <el-icon :size="22"><component :is="item.icon" /></el-icon>
              </div>
              <div class="kpi-info">
                <div class="kpi-value" :class="{ 'text-red': item.highlight }">
                  {{ item.value }}<span class="kpi-unit">{{ item.unit }}</span>
                </div>
                <div class="kpi-label">{{ item.label }}</div>
              </div>
            </div>
          </el-card>
        </div>
      </div>
      <el-empty v-if="!loading && !hasDashboardData" description="暂无工作台数据" :image-size="100" />
    </div>

    <!-- 预警提醒 + 快捷操作 + 学习趋势 三列并列 -->
    <el-row :gutter="16" class="content-row">
      <!-- 预警提醒 -->
      <el-col :span="8">
        <el-card shadow="never" class="warn-card equal-card">
          <template #header>
            <div class="card-header">
              <el-icon color="#FA8C16"><Warning /></el-icon>
              <span>预警提醒</span>
            </div>
          </template>
          <div v-loading="loading">
            <div v-if="warnings.length > 0" class="warn-list">
              <div v-for="(w, idx) in warnings" :key="idx" class="warn-item" @click="handleWarnClick(w)">
                <span class="warn-dot" :class="w.level"></span>
                <div class="warn-content">
                  <span class="warn-title">{{ w.title }}</span>
                  <span class="warn-desc">{{ w.desc }}</span>
                </div>
                <span class="warn-action">去查看 →</span>
              </div>
            </div>
            <el-empty v-else description="暂无预警" :image-size="80" />
          </div>
        </el-card>
      </el-col>

      <!-- 快捷操作 -->
      <el-col :span="8">
        <el-card shadow="never" class="equal-card">
          <template #header>
            <div class="card-header">
              <el-icon color="#C8161D"><Operation /></el-icon>
              <span>快捷操作</span>
            </div>
          </template>
          <div class="quick-actions">
            <div class="action-item b1" @click="$router.push('/organization')">
              <div class="action-icon"><el-icon :size="26"><User /></el-icon></div>
              <div class="action-text">+ 新增党员</div>
            </div>
            <div class="action-item b2" @click="$router.push('/learning-content')">
              <div class="action-icon"><el-icon :size="26"><Upload /></el-icon></div>
              <div class="action-text">下发任务</div>
            </div>
            <div class="action-item b3" @click="$router.push('/exam-management')">
              <div class="action-icon"><el-icon :size="26"><Document /></el-icon></div>
              <div class="action-text">新建试卷</div>
            </div>
            <div class="action-item b4" @click="$router.push('/data-analysis')">
              <div class="action-icon"><el-icon :size="26"><Cpu /></el-icon></div>
              <div class="action-text">AI数据查询</div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 学习趋势图 -->
      <el-col :span="8">
        <el-card shadow="never" class="equal-card">
          <template #header>
            <div class="card-header">
              <el-icon color="#C8161D"><TrendCharts /></el-icon>
              <span>学习趋势</span>
              <el-button link type="primary" style="margin-left:auto" @click="refresh">
                <el-icon><Refresh /></el-icon>刷新
              </el-button>
            </div>
          </template>
          <div ref="chartRef" class="chart-container" v-loading="loading"></div>
          <el-empty v-if="!loading && !hasTrendData" description="暂无趋势数据" :image-size="80" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import * as echarts from 'echarts'
import { getDashboard } from '@/api/statistics'
import { formatDate } from '@/utils/format'

const router = useRouter()
const loading = ref(false)
const dashboard = ref(null)
const chartRef = ref(null)
let chartInstance = null

// 从后端数据中灵活提取字段
function pickField(obj, keys, defaultValue = 0) {
  if (!obj) return defaultValue
  for (const k of keys) {
    if (obj[k] !== undefined && obj[k] !== null) return obj[k]
  }
  return defaultValue
}

const hasDashboardData = computed(() => {
  const d = dashboard.value
  if (!d) return false
  return Object.values(d).some((v) => v !== undefined && v !== null && v !== '' && !(Array.isArray(v) && v.length === 0))
})

const hasTrendData = computed(() => {
  const d = dashboard.value
  if (!d || !d.trend) return false
  const t = d.trend
  return (t.dates && t.dates.length > 0) || (t.learners && t.learners.length > 0)
})

const kpiList = computed(() => {
  const d = dashboard.value || {}
  const totalMembers = pickField(d, ['totalMembers', 'memberCount', 'memberTotal', 'members'], 0)
  const pendingTasks = pickField(d, ['pendingTasks', 'taskCount', 'todoTasks', 'tasks'], 0)
  const pendingExams = pickField(d, ['pendingExams', 'examCount', 'todoExams', 'exams'], 0)
  const afkMembers = pickField(d, ['afkMembers', 'idleCount', 'idleMembers', 'afkCount'], 0)
  const avgCompletion = pickField(d, ['avgCompletionRate', 'completionRate', 'avgCompletion', 'averageRate'], 0)

  return [
    {
      label: '党员总数',
      value: totalMembers,
      unit: '人',
      icon: 'User',
      bgColor: '#FFF1F0',
      iconColor: '#C8161D',
      highlight: false
    },
    {
      label: '待办任务',
      value: pendingTasks,
      unit: '项',
      icon: 'List',
      bgColor: '#FFF7E6',
      iconColor: '#FA8C16',
      highlight: false
    },
    {
      label: '待批阅测验',
      value: pendingExams,
      unit: '份',
      icon: 'Document',
      bgColor: '#E6F7FF',
      iconColor: '#1890FF',
      highlight: false
    },
    {
      label: '挂机人员',
      value: afkMembers,
      unit: '人',
      icon: 'View',
      bgColor: '#FFF1F0',
      iconColor: '#F5222D',
      highlight: false
    },
    {
      label: '支部完成率均值',
      value: avgCompletion,
      unit: '%',
      icon: 'CircleCheck',
      bgColor: '#F6FFED',
      iconColor: '#52C41A',
      highlight: true
    }
  ]
})

const warnings = computed(() => {
  const d = dashboard.value
  if (!d) return []
  const list = d.warnings || d.alerts || d.notices || []
  if (Array.isArray(list) && list.length > 0) {
    return list.map((w) => ({
      title: w.title || w.type || w.name || '提醒',
      desc: w.content || w.desc || w.description || w.message || '',
      level: w.level === 'high' || w.level === 'danger' || w.priority === 'high' ? 'red' : w.level === 'medium' || w.priority === 'medium' ? 'yellow' : 'orange',
      route: w.route || w.path || '',
      tab: w.tab || ''
    }))
  }
  // 如果后端没有返回warnings，根据KPI数据生成默认预警
  const generated = []
  const afk = pickField(d, ['afkMembers', 'idleCount', 'idleMembers'], 0)
  if (afk > 0) {
    generated.push({
      title: '挂机学习预警',
      desc: `检测到 ${afk} 名党员存在挂机学习行为，建议及时提醒`,
      level: 'red',
      route: '/organization',
      tab: 'anticheat'
    })
  }
  const pending = pickField(d, ['pendingTasks', 'taskCount'], 0)
  if (pending > 0) {
    generated.push({
      title: '待办任务提醒',
      desc: `当前有 ${pending} 项学习任务待处理`,
      level: 'yellow',
      route: '/learning-content',
      tab: 'task'
    })
  }
  const exams = pickField(d, ['pendingExams', 'examCount'], 0)
  if (exams > 0) {
    generated.push({
      title: '测验待批阅',
      desc: `有 ${exams} 份测验答卷待批阅`,
      level: 'orange',
      route: '/exam-management',
      tab: 'test'
    })
  }
  return generated
})

function handleWarnClick(w) {
  if (w.route) {
    router.push(w.route)
  }
}

async function loadData() {
  loading.value = true
  try {
    const res = await getDashboard()
    dashboard.value = res
    await nextTick()
    renderChart()
  } catch (e) {
    // 错误已由拦截器提示
  } finally {
    loading.value = false
  }
}

function renderChart() {
  if (!chartRef.value) return
  if (!chartInstance) {
    chartInstance = echarts.init(chartRef.value)
  }
  const d = dashboard.value || {}
  const trend = d.trend || d.trendData || {}
  const dates = trend.dates || trend.months || trend.xAxis || []
  const learners = trend.learners || trend.learningCount || trend.series1 || []
  const completed = trend.completed || trend.completionCount || trend.series2 || []

  const option = {
    tooltip: { trigger: 'axis' },
    legend: { data: ['学习人数', '完成任务'], top: 0 },
    grid: { left: '3%', right: '4%', bottom: '3%', top: '15%', containLabel: true },
    xAxis: {
      type: 'category',
      data: dates,
      boundaryGap: false
    },
    yAxis: { type: 'value' },
    series: [
      {
        name: '学习人数',
        type: 'line',
        smooth: true,
        data: learners,
        itemStyle: { color: '#C8161D' },
        areaStyle: { color: 'rgba(200,22,29,0.1)' }
      },
      {
        name: '完成任务',
        type: 'line',
        smooth: true,
        data: completed,
        itemStyle: { color: '#E6A23C' }
      }
    ]
  }
  chartInstance.setOption(option, true)
}

function refresh() {
  loadData()
}

function handleResize() {
  chartInstance && chartInstance.resize()
}

onMounted(() => {
  loadData()
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  chartInstance && chartInstance.dispose()
})
</script>

<style scoped>
.dashboard { padding: 0; }
.kpi-row { margin-bottom: 18px; }
.kpi-flex {
  display: flex;
  gap: 16px;
}
.kpi-col {
  flex: 1;
  min-width: 0;
}
.equal-card {
  height: 100%;
}
.equal-card :deep(.el-card__body) {
  height: calc(100% - 57px);
  overflow: hidden;
}
.kpi-card {
  border-radius: 6px;
  border: 1px solid #f0f0f0;
}
.kpi-card :deep(.el-card__body) {
  padding: 18px 20px;
}
.kpi-inner {
  display: flex;
  align-items: center;
  gap: 14px;
}
.kpi-icon {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.kpi-info {
  flex: 1;
  text-align: right;
}
.kpi-value {
  font-size: 26px;
  font-weight: 700;
  color: #303133;
  line-height: 1.1;
}
.kpi-value.text-red {
  color: #C8161D;
}
.kpi-unit {
  font-size: 14px;
  color: #909399;
  font-weight: 400;
  margin-left: 2px;
}
.kpi-label {
  color: #909399;
  font-size: 13px;
  margin-top: 6px;
}

.content-row { margin-bottom: 18px; }
.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  font-size: 15px;
}

/* 预警提醒 */
.warn-card :deep(.el-card__body) {
  padding: 0;
}
.warn-list {
  padding: 0;
}
.warn-item {
  display: flex;
  align-items: center;
  padding: 14px 20px;
  border-bottom: 1px solid #f5f5f5;
  gap: 14px;
  cursor: pointer;
  transition: background 0.15s;
}
.warn-item:last-child {
  border-bottom: none;
}
.warn-item:hover {
  background: #fafafa;
}
.warn-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  flex-shrink: 0;
}
.warn-dot.red { background: #f5222d; }
.warn-dot.yellow { background: #faad14; }
.warn-dot.orange { background: #fa8c16; }
.warn-content {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}
.warn-title {
  font-weight: 500;
  color: #303133;
}
.warn-desc {
  color: #606266;
  font-size: 13px;
}
.warn-action {
  color: #C8161D;
  font-size: 13px;
  flex-shrink: 0;
}

/* 快捷操作 */
.quick-actions {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 14px;
}
.action-item {
  padding: 22px 16px;
  text-align: center;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid #f0f0f0;
}
.action-item:hover {
  border-color: #C8161D;
  box-shadow: 0 2px 8px rgba(200,22,29,0.1);
  transform: translateY(-2px);
}
.action-icon {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  margin: 0 auto 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
}
.action-item.b1 .action-icon { background: #f5222d; }
.action-item.b2 .action-icon { background: #fa8c16; }
.action-item.b3 .action-icon { background: #1890ff; }
.action-item.b4 .action-icon { background: #722ed1; }
.action-text {
  font-size: 14px;
  color: #303133;
  font-weight: 500;
}

.chart-container {
  width: 100%;
  height: 300px;
}
</style>
