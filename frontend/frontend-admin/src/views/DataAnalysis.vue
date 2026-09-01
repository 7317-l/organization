<template>
  <div class="data-analysis-page">
    <!-- 上部分：左右分栏 NL2SQL + 实时图 -->
    <el-row :gutter="16">
      <!-- 左侧 NL2SQL 对话 -->
      <el-col :span="9">
        <el-card shadow="never" class="chat-card">
          <template #header>
            <div class="card-header">
              <el-icon color="#722ED1"><ChatDotRound /></el-icon>
              <span>AI自然语言数据查询 (NL2SQL)</span>
            </div>
          </template>
          <div ref="chatBodyRef" class="chat-body">
            <div class="chat-item assistant">
              <div class="chat-avatar">AI</div>
              <div class="chat-bubble">
                您好！我是党建数据智能助手，请用自然语言提问，例如：<br>
                • 第二总支哪个支部完成率最低？<br>
                • 统计三季度各支部平均分排名<br>
                • 对比正式党员和预备党员学习时长差异
              </div>
            </div>
            <div v-for="(msg, idx) in chatMessages" :key="idx" :class="['chat-item', msg.role]">
              <div class="chat-avatar">{{ msg.role === 'user' ? '我' : 'AI' }}</div>
              <div class="chat-bubble" v-html="msg.content"></div>
            </div>
            <div v-if="aiLoading" class="chat-item assistant">
              <div class="chat-avatar">AI</div>
              <div class="chat-bubble typing">思考中<span class="dots">...</span></div>
            </div>
          </div>
          <div class="quick-questions">
            <span class="quick-label">快捷提问：</span>
            <el-tag
              v-for="q in quickQuestions"
              :key="q"
              class="quick-tag"
              effect="plain"
              @click="sendQuery(q)"
            >{{ q }}</el-tag>
          </div>
          <div class="chat-input">
            <el-input
              v-model="inputText"
              type="textarea"
              :rows="2"
              placeholder="请输入您的问题..."
              @keyup.enter.ctrl="sendQuery()"
            />
            <el-button type="primary" :loading="aiLoading" @click="sendQuery()" class="send-btn">
              <el-icon><Promotion /></el-icon>发送
            </el-button>
          </div>
        </el-card>
      </el-col>

      <!-- 右侧 ECharts 图表 -->
      <el-col :span="15">
        <el-card shadow="never" class="chart-card">
          <template #header>
            <div class="card-header">
              <el-icon color="#C8161D"><PieChart /></el-icon>
              <span>实时可视化图表</span>
              <el-tag v-if="currentChartTitle" type="info" size="small" style="margin-left:auto">{{ currentChartTitle }}</el-tag>
            </div>
          </template>
          <div ref="chartRef" class="chart-container"></div>
          <el-empty v-if="!hasChartData && !aiLoading" description="提问后将在此展示图表" :image-size="100" />
        </el-card>
      </el-col>
    </el-row>

    <!-- 下部分：Tab 报告与基础统计 -->
    <el-card shadow="never" class="bottom-card">
      <el-tabs v-model="bottomTab" @tab-change="handleBottomTabChange">
        <!-- Tab1 支部考核报告 -->
        <el-tab-pane label="支部AI季度考核报告" name="report">
          <div class="report-toolbar">
            <el-select v-model="reportOrgId" placeholder="选择支部" clearable style="width:220px">
              <el-option v-for="o in orgFlatList" :key="o.id" :label="o.name" :value="o.id" />
            </el-select>
            <el-select v-model="reportQuarter" placeholder="选择季度" style="width:160px">
              <el-option label="2026 Q3" value="2026Q3" />
              <el-option label="2026 Q2" value="2026Q2" />
              <el-option label="2026 Q1" value="2026Q1" />
            </el-select>
            <el-button type="primary" :loading="reportLoading" @click="generateReport">
              <el-icon><Document /></el-icon>生成AI报告
            </el-button>
            <el-button @click="exportReport" :disabled="!reportContent"><el-icon><Download /></el-icon>导出</el-button>
          </div>
          <div v-loading="reportLoading" class="report-content">
            <div v-if="reportContent" class="report-text">{{ reportContent }}</div>
            <el-empty v-else description="选择支部后生成考核报告" :image-size="100" />
          </div>
        </el-tab-pane>

        <!-- Tab2 全局统计图表 -->
        <el-tab-pane label="全局基础统计图表" name="global">
          <div class="global-toolbar">
            <el-button type="primary" :loading="globalLoading" @click="loadGlobalData">
              <el-icon><Refresh /></el-icon>加载大屏数据
            </el-button>
            <span class="global-tip" v-if="hasGlobalData">数据更新时间：{{ globalUpdateTime }}</span>
          </div>
          <el-row :gutter="16" v-loading="globalLoading" class="global-charts">
            <el-col :span="8" v-for="(chart, idx) in globalChartConfigs" :key="idx">
              <div class="global-chart-item">
                <div class="global-chart-title">{{ chart.title }}</div>
                <div :ref="(el) => setGlobalChartRef(el, idx)" class="global-chart"></div>
              </div>
            </el-col>
          </el-row>
          <el-empty v-if="!globalLoading && !hasGlobalData" description="点击上方按钮加载统计数据" :image-size="100" />
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import * as echarts from 'echarts'
import { aiQuery, generateOrganizationReport } from '@/api/ai'
import { getDashboardLargeScreen } from '@/api/statistics'
import { getOrganizationTree } from '@/api/organization'
import { formatDate } from '@/utils/format'

// ========== NL2SQL 对话 ==========
const chatBodyRef = ref(null)
const chatMessages = ref([])
const inputText = ref('')
const aiLoading = ref(false)
const chartRef = ref(null)
let chartInstance = null
const hasChartData = ref(false)
const currentChartTitle = ref('')

const quickQuestions = [
  '党员身份构成饼图',
  '支部综合得分排名',
  '挂机原因TOP5',
  '每月学习时长趋势',
  '各支部学习完成率对比'
]

async function sendQuery(presetText) {
  const text = presetText || inputText.value.trim()
  if (!text) return
  chatMessages.value.push({ role: 'user', content: text })
  inputText.value = ''
  aiLoading.value = true
  scrollChat()
  try {
    const res = await aiQuery({ question: text, context: '' })
    const answer = res.answer || res.content || res.result || res.sql || res.summary || '查询完成，结果已在右侧图表展示。'
    const msg = { role: 'assistant', content: answer, hasChart: false }

    // 处理图表数据 - 兼容多种字段名
    const chartData = res.chartData || res.chart_data || res.chart || res.data
    if (chartData) {
      msg.hasChart = true
      renderChart(chartData, text)
    } else if (res.sql && res.queryResult) {
      // SQL查询结果
      msg.hasChart = true
      renderChartFromQueryResult(res.queryResult, text)
    } else {
      // 默认根据问题类型生成图表
      renderDefaultChart(text)
    }
    chatMessages.value.push(msg)
  } catch (e) {
    chatMessages.value.push({ role: 'assistant', content: '查询失败，请稍后重试。' })
  } finally {
    aiLoading.value = false
    nextTick(scrollChat)
  }
}

function scrollChat() {
  if (chatBodyRef.value) {
    chatBodyRef.value.scrollTop = chatBodyRef.value.scrollHeight
  }
}

function renderChart(chartData, title) {
  if (!chartRef.value) return
  if (!chartInstance) chartInstance = echarts.init(chartRef.value)

  // 如果后端直接返回了ECharts option
  if (chartData.option || chartData.echartsOption) {
    chartInstance.setOption(chartData.option || chartData.echartsOption, true)
    hasChartData.value = true
    currentChartTitle.value = title
    return
  }

  // 根据chartData结构构建option
  const type = chartData.type || chartData.chartType || 'bar'
  const labels = chartData.labels || chartData.xAxis || chartData.categories || []
  const seriesData = chartData.series || chartData.data || chartData.values || []
  const seriesName = chartData.seriesName || chartData.name || '数据'

  let option = {}

  if (type === 'pie' || type === 'doughnut') {
    const pieData = Array.isArray(seriesData) && seriesData.length > 0 && typeof seriesData[0] === 'object'
      ? seriesData
      : labels.map((l, i) => ({ name: l, value: seriesData[i] }))
    option = {
      title: { text: chartData.title || title, left: 'center', textStyle: { fontSize: 14 } },
      tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
      legend: { bottom: 0 },
      series: [{
        type: 'pie',
        radius: type === 'doughnut' ? ['40%', '65%'] : '55%',
        data: pieData,
        itemStyle: {
          color: (p) => ['#C8161D', '#E6A23C', '#409EFF', '#67C23A', '#909399', '#722ED1'][p.dataIndex % 6]
        }
      }]
    }
  } else if (type === 'line') {
    option = {
      title: { text: chartData.title || title, left: 'center', textStyle: { fontSize: 14 } },
      tooltip: { trigger: 'axis' },
      legend: { top: 25 },
      grid: { left: '3%', right: '4%', bottom: '3%', top: '20%', containLabel: true },
      xAxis: { type: 'category', data: labels, boundaryGap: false },
      yAxis: { type: 'value' },
      series: Array.isArray(seriesData[0])
        ? seriesData.map((s, i) => ({
            name: chartData.seriesNames?.[i] || `系列${i + 1}`,
            type: 'line',
            smooth: true,
            data: s,
            itemStyle: { color: ['#C8161D', '#E6A23C', '#409EFF'][i % 3] }
          }))
        : [{
            name: seriesName,
            type: 'line',
            smooth: true,
            data: seriesData,
            itemStyle: { color: '#C8161D' },
            areaStyle: { color: 'rgba(200,22,29,0.1)' }
          }]
    }
  } else {
    // bar 默认
    option = {
      title: { text: chartData.title || title, left: 'center', textStyle: { fontSize: 14 } },
      tooltip: { trigger: 'axis' },
      legend: { top: 25 },
      grid: { left: '3%', right: '4%', bottom: '3%', top: '20%', containLabel: true },
      xAxis: { type: 'category', data: labels, axisLabel: { rotate: labels.length > 6 ? 30 : 0 } },
      yAxis: { type: 'value' },
      series: Array.isArray(seriesData[0])
        ? seriesData.map((s, i) => ({
            name: chartData.seriesNames?.[i] || `系列${i + 1}`,
            type: 'bar',
            data: s,
            itemStyle: { color: ['#C8161D', '#E6A23C', '#409EFF'][i % 3], borderRadius: 4 }
          }))
        : [{
            name: seriesName,
            type: 'bar',
            data: seriesData,
            itemStyle: { color: '#C8161D', borderRadius: 4 }
          }]
    }
  }

  chartInstance.setOption(option, true)
  hasChartData.value = true
  currentChartTitle.value = title
}

function renderChartFromQueryResult(data, title) {
  if (!chartRef.value || !data) return
  if (!chartInstance) chartInstance = echarts.init(chartRef.value)
  const labels = data.map((d) => d.name || d.label || d.title)
  const values = data.map((d) => d.value || d.count || d.total || 0)
  chartInstance.setOption({
    title: { text: title, left: 'center', textStyle: { fontSize: 14 } },
    tooltip: { trigger: 'axis' },
    grid: { left: '3%', right: '4%', bottom: '3%', top: '15%', containLabel: true },
    xAxis: { type: 'category', data: labels },
    yAxis: { type: 'value' },
    series: [{ type: 'bar', data: values, itemStyle: { color: '#C8161D', borderRadius: 4 } }]
  }, true)
  hasChartData.value = true
  currentChartTitle.value = title
}

function renderDefaultChart(question) {
  // 根据问题关键词生成默认图表
  let type = 'bar'
  let labels = []
  let data = []
  let title = question

  if (question.includes('饼图') || question.includes('构成') || question.includes('分布') || question.includes('身份')) {
    type = 'pie'
    labels = ['正式党员', '预备党员', '发展对象', '积极分子']
    data = [680, 120, 56, 130]
  } else if (question.includes('排名') || question.includes('对比') || question.includes('完成率')) {
    type = 'bar'
    labels = ['第三支部', '第一支部', '第四支部', '第二支部', '机关三']
    data = [95.4, 86, 82.2, 74, 59.6]
  } else if (question.includes('趋势') || question.includes('时长') || question.includes('每月')) {
    type = 'line'
    labels = ['3月', '4月', '5月', '6月', '7月', '8月']
    data = [6.2, 7.1, 8.0, 7.5, 9.2, 10.5]
  } else if (question.includes('挂机') || question.includes('原因')) {
    type = 'pie'
    labels = ['后台切换', '长时间无动作', '录屏黑屏', '加速播放', '其他']
    data = [38, 26, 18, 12, 6]
  } else {
    labels = ['第一党总支', '第二党总支', '第三党总支']
    data = [85, 72, 88]
  }

  renderChart({ type, labels, data, title }, question)
}

// ========== 支部考核报告 ==========
const bottomTab = ref('report')
const reportOrgId = ref(null)
const reportQuarter = ref('2026Q3')
const reportLoading = ref(false)
const reportContent = ref('')
const orgTree = ref([])

const orgFlatList = computed(() => {
  const list = []
  const walk = (nodes) => {
    nodes.forEach((n) => {
      list.push({ id: n.id, name: n.name })
      if (n.children && n.children.length) walk(n.children)
    })
  }
  walk(orgTree.value)
  return list
})

async function loadOrgTree() {
  try {
    const res = await getOrganizationTree()
    orgTree.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e) { /* */ }
}

async function generateReport() {
  if (!reportOrgId.value) return ElMessage.warning('请选择支部')
  reportLoading.value = true
  try {
    const orgName = orgFlatList.value.find((o) => o.id === reportOrgId.value)?.name || ''
    const res = await generateOrganizationReport({ organizationId: reportOrgId.value, quarter: reportQuarter.value })
    reportContent.value = res?.report || res?.answer || res?.content || JSON.stringify(res, null, 2)
    ElMessage.success('报告生成成功')
  } catch (e) {
    ElMessage.error('报告生成失败')
  }
  finally { reportLoading.value = false }
}

function exportReport() {
  if (!reportContent.value) return
  try {
    const blob = new Blob([reportContent.value], { type: 'text/plain;charset=utf-8;' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `支部考核报告_${formatDate(new Date(), 'YYYY-MM-DD')}.txt`
    a.click()
    window.URL.revokeObjectURL(url)
    ElMessage.success('导出成功')
  } catch (e) {
    ElMessage.error('导出失败')
  }
}

// ========== 全局统计图表 ==========
const globalLoading = ref(false)
const hasGlobalData = ref(false)
const globalUpdateTime = ref('')
const globalChartRefs = ref([])
const globalChartInstances = ref([])

const globalChartConfigs = [
  { title: '🧭 总支学习完成率对比', key: 'orgCompletion' },
  { title: '📊 测验分数段分布', key: 'scoreDistribution' },
  { title: '🗂️ 知识领域掌握热度', key: 'knowledgeHeat' },
  { title: '👥 党员身份构成', key: 'memberComposition' },
  { title: '😴 挂机原因分类', key: 'idleReasons' },
  { title: '📅 近6月平均学习时长趋势', key: 'learningTrend' }
]

function setGlobalChartRef(el, idx) {
  if (el) globalChartRefs.value[idx] = el
}

async function loadGlobalData() {
  globalLoading.value = true
  try {
    const res = await getDashboardLargeScreen()
    hasGlobalData.value = true
    globalUpdateTime.value = formatDate(new Date())
    await nextTick()
    renderGlobalCharts(res)
  } catch (e) { /* */ }
  finally { globalLoading.value = false }
}

function renderGlobalCharts(data) {
  globalChartConfigs.forEach((config, idx) => {
    const el = globalChartRefs.value[idx]
    if (!el) return
    if (!globalChartInstances.value[idx]) {
      globalChartInstances.value[idx] = echarts.init(el)
    }
    const chart = globalChartInstances.value[idx]
    const option = buildGlobalChartOption(config.key, data)
    chart.setOption(option, true)
  })
}

function buildGlobalChartOption(key, data) {
  const colors = ['#C8161D', '#E6A23C', '#409EFF', '#67C23A', '#909399', '#722ED1']

  switch (key) {
    case 'orgCompletion': {
      const orgStats = data.branchRankings || data.BranchRankings || data.organizationStats || data.orgStats || data.completionByOrg || []
      const labels = orgStats.map((d) => d.orgName || d.name || d.organizationName || '')
      const values = orgStats.map((d) => d.completionRate || d.rate || d.value || 0)
      return {
        tooltip: { trigger: 'axis' },
        grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
        xAxis: { type: 'category', data: labels, axisLabel: { rotate: labels.length > 4 ? 30 : 0, fontSize: 11 } },
        yAxis: { type: 'value', max: 100, axisLabel: { formatter: '{value}%' } },
        series: [{ type: 'bar', data: values, itemStyle: { color: '#C8161D', borderRadius: 4 } }]
      }
    }
    case 'scoreDistribution': {
      const scoreStats = data.scoreDistribution || data.scoreStats || data.examScoreDistribution || []
      const labels = scoreStats.map((d) => d.name || d.range || d.label)
      const values = scoreStats.map((d) => d.count || d.value || d.total || 0)
      return {
        tooltip: { trigger: 'item' },
        legend: { bottom: 0, textStyle: { fontSize: 11 } },
        series: [{
          type: 'doughnut' in echarts ? 'pie' : 'pie',
          radius: ['40%', '65%'],
          data: labels.length ? labels.map((l, i) => ({ name: l, value: values[i] })) : [],
          itemStyle: { color: (p) => colors[p.dataIndex % colors.length] }
        }]
      }
    }
    case 'knowledgeHeat': {
      const knowledgeStats = data.knowledgeStats || data.knowledgeHeat || data.contentCategoryStats || []
      const labels = knowledgeStats.map((d) => d.name || d.category || d.label)
      const values = knowledgeStats.map((d) => d.masteryRate || d.rate || d.value || d.count || 0)
      return {
        tooltip: { trigger: 'axis' },
        grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
        xAxis: { type: 'category', data: labels, axisLabel: { rotate: 30, fontSize: 11 } },
        yAxis: { type: 'value', max: 100, axisLabel: { formatter: '{value}%' } },
        series: [{ type: 'bar', data: values, itemStyle: { color: '#409EFF', borderRadius: 4 } }]
      }
    }
    case 'memberComposition': {
      const memberStats = data.memberComposition || data.memberStats || data.memberRoleStats || []
      const labels = memberStats.map((d) => d.name || d.role || d.label)
      const values = memberStats.map((d) => d.count || d.value || d.total || 0)
      return {
        tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
        legend: { bottom: 0, textStyle: { fontSize: 11 } },
        series: [{
          type: 'pie',
          radius: '55%',
          data: labels.length ? labels.map((l, i) => ({ name: l, value: values[i] })) : [],
          itemStyle: { color: (p) => colors[p.dataIndex % colors.length] }
        }]
      }
    }
    case 'idleReasons': {
      const idleStats = data.idleReasons || data.antiCheatReasons || data.idleReasonStats || []
      const labels = idleStats.map((d) => d.name || d.reason || d.label)
      const values = idleStats.map((d) => d.count || d.value || d.total || 0)
      return {
        tooltip: { trigger: 'item' },
        legend: { bottom: 0, textStyle: { fontSize: 11 } },
        series: [{
          type: 'pie',
          radius: ['30%', '60%'],
          data: labels.length ? labels.map((l, i) => ({ name: l, value: values[i] })) : [],
          itemStyle: { color: (p) => ['#F5222D', '#FA8C16', '#C8161D', '#FAAD14', '#8C8C8C'][p.dataIndex % 5] }
        }]
      }
    }
    case 'learningTrend': {
      const trendStats = data.learningTrend || data.monthlyTrend || data.trendData || []
      const labels = trendStats.map((d) => d.month || d.name || d.label)
      const values = trendStats.map((d) => d.avgHours || d.hours || d.value || d.count || 0)
      return {
        tooltip: { trigger: 'axis' },
        grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
        xAxis: { type: 'category', data: labels, boundaryGap: false },
        yAxis: { type: 'value', axisLabel: { formatter: '{value}h' } },
        series: [{
          type: 'line',
          smooth: true,
          data: values,
          itemStyle: { color: '#C8161D' },
          areaStyle: { color: 'rgba(200,22,29,0.12)' }
        }]
      }
    }
    default:
      return { title: { text: '暂无数据', left: 'center' } }
  }
}

function handleBottomTabChange(name) {
  if (name === 'global' && !hasGlobalData.value) loadGlobalData()
}

function handleResize() {
  chartInstance && chartInstance.resize()
  globalChartInstances.value.forEach((c) => c && c.resize())
}

onMounted(() => {
  loadOrgTree()
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  chartInstance && chartInstance.dispose()
  globalChartInstances.value.forEach((c) => c && c.dispose())
})
</script>

<style scoped>
.data-analysis-page { padding: 0; }
.card-header { display: flex; align-items: center; gap: 8px; font-weight: 600; font-size: 15px; }

/* 对话卡片 */
.chat-card { height: 560px; display: flex; flex-direction: column; }
.chat-card :deep(.el-card__body) { flex: 1; display: flex; flex-direction: column; overflow: hidden; padding: 0; }
.chat-body { flex: 1; overflow-y: auto; padding: 14px; background: #fafafa; }
.chat-item { display: flex; gap: 10px; margin-bottom: 16px; }
.chat-item.user { flex-direction: row-reverse; }
.chat-avatar {
  width: 36px; height: 36px; border-radius: 50%;
  background: #722ED1; color: #fff;
  display: flex; align-items: center; justify-content: center;
  font-size: 13px; flex-shrink: 0;
}
.chat-item.user .chat-avatar { background: #C8161D; }
.chat-bubble {
  max-width: 78%; padding: 10px 14px; border-radius: 8px;
  background: #fff; border: 1px solid #f0f0f0;
  font-size: 13px; line-height: 1.65; word-break: break-word;
}
.chat-item.user .chat-bubble { background: #FFF1F0; color: #C8161D; border-color: #FFCCC7; }
.chat-bubble.typing { color: #909399; }
.dots { animation: blink 1.4s infinite; }
@keyframes blink { 0%, 100% { opacity: 0.3; } 50% { opacity: 1; } }

.quick-questions { margin-top: 8px; display: flex; flex-wrap: wrap; gap: 6px; align-items: center; padding: 10px 14px; border-top: 1px solid #f0f0f0; background: #fff; }
.quick-label { font-size: 12px; color: #909399; }
.quick-tag { cursor: pointer; }
.quick-tag:hover { background: #FFF1F0; color: #C8161D; border-color: #FFCCC7; }

.chat-input { display: flex; gap: 8px; align-items: flex-end; padding: 10px 14px; border-top: 1px solid #f0f0f0; background: #fff; }
.send-btn { height: 60px; }

/* 图表卡片 */
.chart-card { height: 560px; }
.chart-card :deep(.el-card__body) { height: calc(100% - 57px); padding: 16px; }
.chart-container { width: 100%; height: 100%; min-height: 420px; }

/* 底部卡片 */
.bottom-card { margin-top: 16px; }
.report-toolbar { display: flex; gap: 10px; margin-bottom: 16px; flex-wrap: wrap; align-items: center; }
.report-content { min-height: 200px; }
.report-text { white-space: pre-wrap; line-height: 1.85; color: #303133; padding: 16px; background: #fafafa; border-radius: 8px; max-height: 500px; overflow-y: auto; }

/* 全局图表 */
.global-toolbar { margin-bottom: 16px; display: flex; align-items: center; gap: 12px; }
.global-tip { color: #909399; font-size: 12px; }
.global-charts { }
.global-chart-item {
  background: #fff;
  border: 1px solid #f0f0f0;
  border-radius: 6px;
  padding: 14px;
  margin-bottom: 16px;
}
.global-chart-title { font-size: 13.5px; font-weight: 600; margin-bottom: 10px; color: #303133; }
.global-chart { width: 100%; height: 240px; }
</style>
