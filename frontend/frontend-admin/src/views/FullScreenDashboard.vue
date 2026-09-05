<template>
  <div class="fullscreen-dashboard">
    <!-- 顶部提示 -->
    <div class="fs-tip" v-if="!isFullscreen">
      <span>💡 按 F11 进入全屏模式，获得更佳体验</span>
      <el-button type="primary" size="small" @click="enterFullscreen">进入全屏</el-button>
    </div>

    <!-- 顶部统计卡片 -->
    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-icon red">👥</div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.totalMembers || 0 }}</div>
          <div class="stat-label">党员总数</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon blue">🏢</div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.totalOrgs || 0 }}</div>
          <div class="stat-label">支部数</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon green">⏱️</div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.monthlyLearningHours || 0 }}</div>
          <div class="stat-label">本月学习时长(小时)</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon orange">📊</div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.examPassRate || 0 }}%</div>
          <div class="stat-label">考试通过率</div>
        </div>
      </div>
    </div>

    <!-- 图表区域 -->
    <div class="charts-row">
      <div class="chart-card large">
        <div class="chart-title">支部学习完成率</div>
        <div ref="barChartRef" class="chart-container"></div>
      </div>
      <div class="chart-card">
        <div class="chart-title">薄弱知识分布</div>
        <div ref="pieChartRef" class="chart-container"></div>
      </div>
    </div>

    <div class="charts-row">
      <div class="chart-card large">
        <div class="chart-title">近6个月考试平均成绩趋势</div>
        <div ref="lineChartRef" class="chart-container"></div>
      </div>
      <div class="chart-card">
        <div class="chart-title">学习预警名单</div>
        <div class="warning-list">
          <div v-for="(item, idx) in warningList" :key="idx" class="warning-item">
            <span class="warning-name">{{ item.name || item.memberName }}</span>
            <span class="warning-reason">{{ item.reason || '学习时长不足' }}</span>
          </div>
          <el-empty v-if="warningList.length === 0" description="暂无预警" :image-size="60" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue'
import * as echarts from 'echarts'
import request from '@/api/request'

const isFullscreen = ref(false)
const stats = ref({})
const warningList = ref([])
const barChartRef = ref(null)
const pieChartRef = ref(null)
const lineChartRef = ref(null)
let barChart = null
let pieChart = null
let lineChart = null

async function loadData() {
  try {
    const res = await request.get('/statistics/dashboard-largescreen')
    const data = res.data || res
    stats.value = data.stats || data
    warningList.value = data.warnings || data.warningList || []
    await nextTick()
    initCharts(data)
  } catch (e) {
    // 加载失败时用模拟数据
    stats.value = { totalMembers: 128, totalOrgs: 12, monthlyLearningHours: 3560, examPassRate: 87.5 }
    warningList.value = [
      { name: '张三', reason: '连续7天未学习' },
      { name: '李四', reason: '学习时长不足' },
      { name: '王五', reason: '考试未通过' }
    ]
    await nextTick()
    initCharts(null)
  }
}

function initCharts(data) {
  // 柱状图 - 支部完成率
  if (barChartRef.value) {
    barChart = echarts.init(barChartRef.value)
    const branches = data?.branchCompletion?.map(b => b.name) || ['第一支部', '第二支部', '第三支部', '第四支部', '第五支部']
    const rates = data?.branchCompletion?.map(b => b.rate) || [85, 72, 90, 68, 78]
    barChart.setOption({
      backgroundColor: 'transparent',
      tooltip: { trigger: 'axis' },
      grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
      xAxis: { type: 'category', data: branches, axisLabel: { color: '#aaa' }, axisLine: { lineStyle: { color: '#333' } } },
      yAxis: { type: 'value', max: 100, axisLabel: { color: '#aaa', formatter: '{value}%' }, splitLine: { lineStyle: { color: '#222' } } },
      series: [{
        type: 'bar',
        data: rates,
        itemStyle: { color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{ offset: 0, color: '#e74c3c' }, { offset: 1, color: '#c0392b' }]), borderRadius: [4, 4, 0, 0] },
        barWidth: '40%'
      }]
    })
  }

  // 饼图 - 薄弱知识分布
  if (pieChartRef.value) {
    pieChart = echarts.init(pieChartRef.value)
    const weakData = data?.weakKnowledge || [
      { name: '党史', value: 35 }, { name: '党章', value: 25 },
      { name: '党规党纪', value: 20 }, { name: '党的理论', value: 20 }
    ]
    pieChart.setOption({
      backgroundColor: 'transparent',
      tooltip: { trigger: 'item' },
      legend: { bottom: '5%', left: 'center', textStyle: { color: '#aaa' } },
      series: [{
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: false,
        itemStyle: { borderRadius: 6, borderColor: '#0a1929', borderWidth: 2 },
        label: { show: false },
        data: weakData,
        color: ['#e74c3c', '#f39c12', '#3498db', '#2ecc71', '#9b59b6']
      }]
    })
  }

  // 折线图 - 考试趋势
  if (lineChartRef.value) {
    lineChart = echarts.init(lineChartRef.value)
    const months = data?.examTrend?.map(t => t.month) || ['4月', '5月', '6月', '7月', '8月', '9月']
    const scores = data?.examTrend?.map(t => t.score) || [72, 75, 78, 80, 82, 85]
    lineChart.setOption({
      backgroundColor: 'transparent',
      tooltip: { trigger: 'axis' },
      grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
      xAxis: { type: 'category', data: months, axisLabel: { color: '#aaa' }, axisLine: { lineStyle: { color: '#333' } } },
      yAxis: { type: 'value', min: 60, max: 100, axisLabel: { color: '#aaa' }, splitLine: { lineStyle: { color: '#222' } } },
      series: [{
        type: 'line',
        data: scores,
        smooth: true,
        lineStyle: { color: '#e74c3c', width: 3 },
        itemStyle: { color: '#e74c3c' },
        areaStyle: { color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{ offset: 0, color: 'rgba(231,76,60,0.3)' }, { offset: 1, color: 'rgba(231,76,60,0)' }]) }
      }]
    })
  }
}

function enterFullscreen() {
  document.documentElement.requestFullscreen?.()
  isFullscreen.value = true
}

function handleResize() {
  barChart?.resize()
  pieChart?.resize()
  lineChart?.resize()
}

onMounted(() => {
  loadData()
  window.addEventListener('resize', handleResize)
  document.addEventListener('fullscreenchange', () => {
    isFullscreen.value = !!document.fullscreenElement
  })
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  barChart?.dispose()
  pieChart?.dispose()
  lineChart?.dispose()
})
</script>

<style scoped>
.fullscreen-dashboard {
  min-height: 100vh;
  background: linear-gradient(135deg, #0a1929 0%, #0d1117 100%);
  padding: 20px;
  color: #fff;
}
.fs-tip {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: rgba(231, 76, 60, 0.15);
  border: 1px solid rgba(231, 76, 60, 0.3);
  border-radius: 8px;
  padding: 12px 20px;
  margin-bottom: 20px;
  color: #f39c12;
}
.stats-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 20px;
}
.stat-card {
  background: #16213e;
  border-radius: 12px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  border: 1px solid rgba(255,255,255,0.05);
}
.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
}
.stat-icon.red { background: rgba(231, 76, 60, 0.2); }
.stat-icon.blue { background: rgba(52, 152, 219, 0.2); }
.stat-icon.green { background: rgba(46, 204, 113, 0.2); }
.stat-icon.orange { background: rgba(243, 156, 18, 0.2); }
.stat-value { font-size: 28px; font-weight: bold; color: #fff; }
.stat-label { font-size: 13px; color: #888; margin-top: 4px; }
.charts-row {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 16px;
  margin-bottom: 20px;
}
.chart-card {
  background: #16213e;
  border-radius: 12px;
  padding: 20px;
  border: 1px solid rgba(255,255,255,0.05);
}
.chart-card.large { grid-column: span 1; }
.chart-title { font-size: 16px; font-weight: bold; margin-bottom: 16px; color: #ddd; }
.chart-container { width: 100%; height: 280px; }
.warning-list { max-height: 280px; overflow-y: auto; }
.warning-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  background: rgba(231, 76, 60, 0.1);
  border-radius: 8px;
  margin-bottom: 8px;
  border-left: 3px solid #e74c3c;
}
.warning-name { color: #fff; font-weight: 500; }
.warning-reason { color: #f39c12; font-size: 12px; }
</style>
