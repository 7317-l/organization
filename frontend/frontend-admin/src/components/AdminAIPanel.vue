<template>
  <!-- AI 悬浮按钮 -->
  <div v-if="!visible" class="admin-ai-fab" @click="visible = true" title="AI数据查询助手">
    <el-icon :size="26"><Search /></el-icon>
    <span class="fab-text">AI</span>
  </div>

  <!-- AI 面板 -->
  <div v-if="visible" class="admin-ai-overlay" @click.self="visible = false">
    <div class="admin-ai-panel">
      <!-- 头部 -->
      <div class="ai-panel-header">
        <div class="header-left">
          <div class="ai-logo">AI</div>
          <div>
            <div class="header-title">管理后台 · AI数据查询助手</div>
            <div class="header-sub">自然语言查数据 · 支部学习分析</div>
          </div>
        </div>
        <div class="header-right">
          <span class="ai-status"><span class="status-dot"></span>在线</span>
          <el-icon class="close-btn" @click="visible = false"><Close /></el-icon>
        </div>
      </div>

      <div class="ai-panel-body">
        <!-- 左侧导航 -->
        <div class="ai-sidebar">
          <div
            v-for="item in navItems"
            :key="item.key"
            class="nav-item"
            :class="{ active: activeNav === item.key }"
            @click="activeNav = item.key"
          >
            <el-icon><component :is="item.icon" /></el-icon>
            <span>{{ item.label }}</span>
          </div>

          <!-- 快捷查询示例 -->
          <div class="sidebar-tips" v-if="activeNav === 'nl2sql'">
            <div class="tips-title">试试这样问：</div>
            <div class="tip-item" @click="askExample('各支部党员人数统计')">各支部党员人数</div>
            <div class="tip-item" @click="askExample('学习完成率最低的三个支部')">学习完成率最低的支部</div>
            <div class="tip-item" @click="askExample('本月测验平均分排名')">本月测验平均分排名</div>
            <div class="tip-item" @click="askExample('挂机次数最多的党员')">挂机次数最多的党员</div>
          </div>
        </div>

        <!-- 右侧内容 -->
        <div class="ai-content">
          <!-- ========== NL2SQL 自然语言查询 ========== -->
          <div v-if="activeNav === 'nl2sql'" class="nl2sql-section">
            <div class="section-title">AI自然语言数据查询</div>
            <p class="section-desc">用自然语言提问，AI自动生成SQL并查询数据，支持多轮追问</p>

            <!-- 对话区 -->
            <div class="chat-messages" ref="chatRef">
              <div class="chat-msg ai">
                <div class="msg-avatar">AI</div>
                <div class="msg-bubble">您好，我是数据查询助手。您可以用自然语言提问，例如"各支部学习完成率排名"、"本月测验平均分"等，我会自动查询数据并展示结果。</div>
              </div>

              <div v-for="(msg, idx) in nl2sqlMessages" :key="idx" class="chat-msg" :class="msg.role">
                <div class="msg-avatar">{{ msg.role === 'user' ? '管' : 'AI' }}</div>
                <div class="msg-bubble">
                  <div v-if="msg.role === 'user'">{{ msg.question }}</div>
                  <template v-else>
                    <div v-if="msg.explanation" class="result-explain">{{ msg.explanation }}</div>
                    <div v-if="msg.generatedSql" class="result-sql">
                      <details class="sql-details">
                        <summary class="sql-summary">查看生成SQL</summary>
                        <pre class="sql-code">{{ msg.generatedSql }}</pre>
                      </details>
                    </div>
                    <!-- 图表 -->
                    <div v-if="msg.chartData && msg.chartData.labels && msg.chartData.labels.length > 0" class="result-chart">
                      <div class="chart-title">数据图表</div>
                      <div class="bar-chart">
                        <div v-for="(label, i) in msg.chartData.labels" :key="i" class="bar-row">
                          <span class="bar-label">{{ label }}</span>
                          <div class="bar-track">
                            <div class="bar-fill" :style="{ width: getBarWidth(msg.chartData.values, i) + '%' }"></div>
                          </div>
                          <span class="bar-value">{{ msg.chartData.values[i] }}</span>
                        </div>
                      </div>
                    </div>
                    <!-- 数据表格 -->
                    <div v-if="msg.resultData && msg.resultData.length > 0" class="result-table">
                      <div class="table-title">查询结果（{{ msg.resultData.length }}条）</div>
                      <div class="table-wrapper">
                        <table>
                          <thead>
                            <tr>
                              <th v-for="(key, ki) in Object.keys(msg.resultData[0])" :key="ki">{{ fieldLabel(key) }}</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr v-for="(row, ri) in msg.resultData.slice(0, 20)" :key="ri">
                              <td v-for="(key, ki) in Object.keys(row)" :key="ki">{{ formatCell(row[key]) }}</td>
                            </tr>
                          </tbody>
                        </table>
                      </div>
                      <div v-if="msg.resultData.length > 20" class="table-more">仅显示前20条，共{{ msg.resultData.length }}条</div>
                    </div>
                    <div v-if="msg.corrections && msg.corrections.length > 0" class="result-corrections">
                      <el-tag size="small" type="warning">已自动修正：{{ msg.corrections.join('、') }}</el-tag>
                    </div>
                  </template>
                </div>
              </div>

              <div v-if="nl2sqlLoading" class="chat-msg ai">
                <div class="msg-avatar">AI</div>
                <div class="msg-bubble typing"><span></span><span></span><span></span></div>
              </div>
            </div>

            <!-- 输入区 -->
            <div class="chat-input-area">
              <el-input
                v-model="nl2sqlInput"
                type="textarea"
                :rows="2"
                placeholder="输入您想查询的数据问题，例如：各支部学习完成率排名..."
                @keydown.enter.exact.prevent="sendNl2sqlQuery"
              />
              <el-button type="primary" :loading="nl2sqlLoading" @click="sendNl2sqlQuery">查询</el-button>
            </div>
          </div>

          <!-- ========== 各支部学习数据查询 ========== -->
          <div v-if="activeNav === 'branch'" class="branch-section">
            <div class="section-title">各支部学习数据查询</div>
            <p class="section-desc">选择支部，查看该支部党员学习、测验、挂机等详细数据</p>

            <!-- 支部选择 -->
            <div class="branch-selector">
              <el-select v-model="selectedOrgId" placeholder="请选择支部" style="width: 300px" @change="loadBranchStats">
                <el-option v-for="org in orgList" :key="org.id" :label="org.name" :value="org.id" />
              </el-select>
              <el-button @click="loadOrgList"><el-icon><Refresh /></el-icon>刷新</el-button>
            </div>

            <!-- 支部数据概览卡片 -->
            <div v-if="branchStats" class="branch-stats-cards">
              <div class="stat-card">
                <div class="stat-num">{{ branchStats.memberCount || 0 }}</div>
                <div class="stat-label">党员总数</div>
              </div>
              <div class="stat-card">
                <div class="stat-num">{{ branchStats.totalLearningHours || 0 }}</div>
                <div class="stat-label">累计学习时长(小时)</div>
              </div>
              <div class="stat-card">
                <div class="stat-num">{{ branchStats.taskCompletionRate || 0 }}%</div>
                <div class="stat-label">任务完成率</div>
              </div>
              <div class="stat-card">
                <div class="stat-num">{{ branchStats.avgExamScore || 0 }}</div>
                <div class="stat-label">测验平均分</div>
              </div>
              <div class="stat-card">
                <div class="stat-num">{{ branchStats.examPassRate || 0 }}%</div>
                <div class="stat-label">测验通过率</div>
              </div>
              <div class="stat-card warning">
                <div class="stat-num">{{ branchStats.idleCount || 0 }}</div>
                <div class="stat-label">挂机人次</div>
              </div>
            </div>

            <!-- 详细数据 -->
            <div v-if="branchStats && branchStats.members && branchStats.members.length > 0" class="branch-detail">
              <div class="detail-title">支部党员学习明细</div>
              <div class="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>姓名</th>
                      <th>学习时长(小时)</th>
                      <th>完成任务数</th>
                      <th>参与测验数</th>
                      <th>平均分</th>
                      <th>挂机次数</th>
                      <th>状态</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(m, idx) in branchStats.members" :key="idx">
                      <td>{{ m.name || m.memberName }}</td>
                      <td>{{ m.learningHours || m.duration || 0 }}</td>
                      <td>{{ m.completedTasks || 0 }}</td>
                      <td>{{ m.examCount || 0 }}</td>
                      <td>{{ m.avgScore || 0 }}</td>
                      <td :class="{ 'text-warning': (m.idleCount || 0) > 0 }">{{ m.idleCount || 0 }}</td>
                      <td>
                        <el-tag :type="(m.idleCount || 0) > 0 ? 'warning' : 'success'" size="small">
                          {{ (m.idleCount || 0) > 0 ? '有挂机' : '正常' }}
                        </el-tag>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <div v-else-if="selectedOrgId && !branchStats" class="branch-loading">
              <el-icon class="is-loading" :size="32"><Loading /></el-icon>
              <p>正在加载支部数据...</p>
            </div>
            <div v-else-if="!selectedOrgId" class="branch-placeholder">
              <el-icon :size="40"><OfficeBuilding /></el-icon>
              <p>请先选择一个支部查看学习数据</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, nextTick, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Search, Close, Refresh, Loading, OfficeBuilding, DataLine, Histogram } from '@element-plus/icons-vue'

const API_BASE = 'http://localhost:5091/api/v1'
const visible = ref(false)
const activeNav = ref('nl2sql')
const chatRef = ref(null)

const navItems = [
  { key: 'nl2sql', label: 'AI自然语言查询', icon: DataLine },
  { key: 'branch', label: '支部学习数据', icon: Histogram }
]

function getToken() {
  return localStorage.getItem('accessToken') || ''
}

/* ========== NL2SQL 自然语言查询 ========== */
const nl2sqlInput = ref('')
const nl2sqlLoading = ref(false)
const nl2sqlMessages = ref([])
let sessionId = ''

function askExample(text) {
  nl2sqlInput.value = text
  sendNl2sqlQuery()
}

async function sendNl2sqlQuery() {
  const question = nl2sqlInput.value.trim()
  if (!question || nl2sqlLoading.value) return
  nl2sqlMessages.value.push({ role: 'user', question })
  nl2sqlInput.value = ''
  nl2sqlLoading.value = true
  await nextTick()
  scrollChat()
  try {
    const res = await fetch(`${API_BASE}/nl2sql/query`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${getToken()}` },
      body: JSON.stringify({ naturalLanguage: question, sessionId: sessionId || undefined })
    })
    const data = await res.json()
    const result = data?.data || data
    if (result) {
      sessionId = result.sessionId || sessionId
      nl2sqlMessages.value.push({
        role: 'ai',
        explanation: result.explanation,
        generatedSql: result.generatedSql,
        resultData: result.resultData || [],
        chartData: result.chartData,
        corrections: result.correctionsApplied || []
      })
    } else {
      nl2sqlMessages.value.push({ role: 'ai', explanation: data?.message || '查询失败，请稍后重试' })
    }
  } catch (e) {
    nl2sqlMessages.value.push({ role: 'ai', explanation: '网络错误，请确认后端服务已启动（端口5091）' })
  } finally {
    nl2sqlLoading.value = false
    await nextTick()
    scrollChat()
  }
}

function scrollChat() {
  if (chatRef.value) chatRef.value.scrollTop = chatRef.value.scrollHeight
}

function fieldLabel(key) {
  const map = {
    org_name: '组织', name: '名称', member_name: '姓名', member_count: '党员数',
    completion_rate: '完成率(%)', avg_score: '平均分', score: '成绩', exam_count: '考试次数',
    date: '日期', minutes: '分钟', member_type: '身份', is_enabled: '启用',
    total_tasks: '任务数', completed: '已完成', total: '总数'
  }
  return map[key] || key
}

function formatCell(v) {
  if (v === null || v === undefined) return '-'
  if (typeof v === 'number' && !Number.isInteger(v)) return Number(v.toFixed(2))
  return v
}

function getBarWidth(values, idx) {
  const max = Math.max(...values, 1)
  return Math.round((values[idx] / max) * 100)
}

/* ========== 支部学习数据查询 ========== */
const orgList = ref([])
const selectedOrgId = ref(null)
const branchStats = ref(null)

async function loadOrgList() {
  try {
    const res = await fetch(`${API_BASE}/organizations/tree`, {
      headers: { 'Authorization': `Bearer ${getToken()}` }
    })
    const data = await res.json()
    const tree = data?.data || data || []
    // 扁平化组织树
    const flat = []
    function flatten(nodes) {
      nodes.forEach(n => {
        flat.push({ id: n.id, name: n.name })
        if (n.children && n.children.length > 0) flatten(n.children)
      })
    }
    flatten(Array.isArray(tree) ? tree : [tree])
    orgList.value = flat
  } catch (e) {
    // 加载失败时用默认数据
    orgList.value = [{ id: 1, name: '第一党支部' }, { id: 2, name: '第二党支部' }]
  }
}

async function loadBranchStats() {
  if (!selectedOrgId.value) return
  branchStats.value = null
  try {
    const res = await fetch(`${API_BASE}/statistics/branch/${selectedOrgId.value}`, {
      headers: { 'Authorization': `Bearer ${getToken()}` }
    })
    const data = await res.json()
    branchStats.value = data?.data || data
  } catch (e) {
    // 加载失败时用模拟数据展示
    branchStats.value = {
      memberCount: 32,
      totalLearningHours: 128.5,
      taskCompletionRate: 86,
      avgExamScore: 78.5,
      examPassRate: 92,
      idleCount: 3,
      members: [
        { name: '张三', learningHours: 8.5, completedTasks: 12, examCount: 5, avgScore: 85, idleCount: 0 },
        { name: '李四', learningHours: 6.2, completedTasks: 8, examCount: 4, avgScore: 72, idleCount: 1 },
        { name: '王五', learningHours: 10.1, completedTasks: 15, examCount: 6, avgScore: 91, idleCount: 0 },
        { name: '赵六', learningHours: 3.5, completedTasks: 4, examCount: 2, avgScore: 65, idleCount: 2 }
      ]
    }
    ElMessage.info('当前为示例数据，后端接口返回后自动替换')
  }
}

onMounted(() => {
  loadOrgList()
})
</script>

<style scoped>
.admin-ai-fab {
  position: fixed; right: 24px; bottom: 24px; width: 56px; height: 56px;
  border-radius: 50%; background: linear-gradient(135deg, #C8161D, #A01016);
  color: #fff; display: flex; flex-direction: column; align-items: center;
  justify-content: center; cursor: pointer; box-shadow: 0 4px 16px rgba(200,22,29,0.4);
  z-index: 9998; transition: transform 0.2s;
}
.admin-ai-fab:hover { transform: scale(1.08); }
.fab-text { font-size: 10px; font-weight: 600; margin-top: 2px; }
.admin-ai-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,0.45); z-index: 9999;
  display: flex; align-items: center; justify-content: center;
}
.admin-ai-panel {
  width: 1000px; height: 680px; background: #fff; border-radius: 12px;
  overflow: hidden; display: flex; flex-direction: column;
  box-shadow: 0 20px 60px rgba(0,0,0,0.3);
}
.ai-panel-header {
  background: linear-gradient(90deg, #C8161D, #A01016); color: #fff;
  padding: 14px 20px; display: flex; align-items: center; justify-content: space-between;
}
.header-left { display: flex; align-items: center; gap: 12px; }
.ai-logo {
  width: 40px; height: 40px; border-radius: 50%; background: rgba(255,255,255,0.2);
  display: flex; align-items: center; justify-content: center; font-weight: 700;
}
.header-title { font-size: 16px; font-weight: 600; }
.header-sub { font-size: 12px; opacity: 0.85; margin-top: 2px; }
.header-right { display: flex; align-items: center; gap: 16px; }
.ai-status { font-size: 13px; display: flex; align-items: center; gap: 6px; }
.status-dot { width: 8px; height: 8px; border-radius: 50%; background: #4ade80; }
.close-btn { cursor: pointer; font-size: 20px; }
.ai-panel-body { flex: 1; display: flex; overflow: hidden; }
.ai-sidebar {
  width: 200px; background: #f8f9fa; border-right: 1px solid #eaecef;
  padding: 12px 0; display: flex; flex-direction: column;
}
.nav-item {
  display: flex; align-items: center; gap: 10px; padding: 12px 20px;
  cursor: pointer; font-size: 14px; color: #333; transition: all 0.15s;
  border-left: 3px solid transparent;
}
.nav-item:hover { background: #f0f0f0; }
.nav-item.active { background: #fff; border-left-color: #C8161D; color: #C8161D; font-weight: 600; }
.sidebar-tips { margin-top: auto; padding: 16px; border-top: 1px solid #eaecef; }
.tips-title { font-size: 12px; color: #999; margin-bottom: 10px; font-weight: 600; }
.tip-item {
  font-size: 13px; color: #555; padding: 6px 8px; border-radius: 4px;
  cursor: pointer; margin-bottom: 4px; transition: background 0.15s;
}
.tip-item:hover { background: #e8f4fd; color: #409eff; }
.ai-content { flex: 1; overflow-y: auto; padding: 20px; }
.section-title { font-size: 18px; font-weight: 600; color: #333; margin-bottom: 6px; }
.section-desc { font-size: 13px; color: #888; margin-bottom: 16px; }

/* 对话区 */
.chat-messages { height: 420px; overflow-y: auto; padding-right: 8px; margin-bottom: 12px; }
.chat-msg { display: flex; gap: 10px; margin-bottom: 16px; }
.chat-msg.user { flex-direction: row-reverse; }
.msg-avatar {
  width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center;
  justify-content: center; font-size: 13px; font-weight: 600; flex-shrink: 0;
}
.chat-msg.ai .msg-avatar { background: #C8161D; color: #fff; }
.chat-msg.user .msg-avatar { background: #e8e8e8; color: #333; }
.msg-bubble {
  max-width: 80%; padding: 10px 14px; border-radius: 10px; font-size: 14px;
  line-height: 1.6; background: #f5f5f5; color: #333;
}
.chat-msg.user .msg-bubble { background: #C8161D; color: #fff; }
.msg-bubble.typing { display: flex; gap: 4px; padding: 14px; }
.msg-bubble.typing span {
  width: 6px; height: 6px; border-radius: 50%; background: #999; animation: bounce 1.2s infinite;
}
.msg-bubble.typing span:nth-child(2) { animation-delay: 0.15s; }
.msg-bubble.typing span:nth-child(3) { animation-delay: 0.3s; }
@keyframes bounce { 0%,60%,100% { transform: translateY(0); opacity: 0.4; } 30% { transform: translateY(-5px); opacity: 1; } }
.chat-input-area { display: flex; gap: 10px; align-items: flex-end; }
.chat-input-area .el-textarea { flex: 1; }

/* 查询结果 */
.result-explain { margin-bottom: 10px; }
.result-sql { background: #1e1e1e; border-radius: 6px; padding: 10px 12px; margin-bottom: 12px; }
.sql-label { font-size: 12px; color: #888; margin-bottom: 4px; }
.sql-code { margin: 0; color: #4ec9b0; font-size: 12px; white-space: pre-wrap; word-break: break-all; }
.result-chart { margin-bottom: 12px; }
.chart-title { font-size: 13px; font-weight: 600; color: #555; margin-bottom: 8px; }
.bar-chart { display: flex; flex-direction: column; gap: 6px; }
.bar-row { display: flex; align-items: center; gap: 8px; }
.bar-label { width: 100px; font-size: 12px; color: #666; text-align: right; flex-shrink: 0; }
.bar-track { flex: 1; height: 18px; background: #eee; border-radius: 3px; overflow: hidden; }
.bar-fill { height: 100%; background: linear-gradient(90deg, #C8161D, #e85a5f); border-radius: 3px; transition: width 0.3s; }
.bar-value { width: 50px; font-size: 12px; color: #333; font-weight: 600; }
.result-table { margin-bottom: 10px; }
.table-title { font-size: 13px; font-weight: 600; color: #555; margin-bottom: 8px; }
.table-wrapper { overflow-x: auto; border: 1px solid #eee; border-radius: 6px; }
.table-wrapper table { width: 100%; border-collapse: collapse; font-size: 13px; }
.table-wrapper th { background: #f5f5f5; padding: 8px 10px; text-align: left; font-weight: 600; color: #555; border-bottom: 1px solid #eee; }
.table-wrapper td { padding: 8px 10px; border-bottom: 1px solid #f0f0f0; color: #333; }
.table-wrapper tr:hover { background: #fafafa; }
.table-more { font-size: 12px; color: #999; margin-top: 6px; text-align: center; }
.result-corrections { margin-top: 8px; }

/* 支部查询 */
.branch-selector { display: flex; gap: 12px; align-items: center; margin-bottom: 20px; }
.branch-stats-cards { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-bottom: 20px; }
.stat-card {
  background: linear-gradient(135deg, #f8f9fa, #fff); border: 1px solid #eee;
  border-radius: 8px; padding: 16px; text-align: center;
}
.stat-card.warning { background: linear-gradient(135deg, #fef0f0, #fff); border-color: #fdd; }
.stat-num { font-size: 28px; font-weight: 700; color: #C8161D; }
.stat-card.warning .stat-num { color: #e6a23c; }
.stat-label { font-size: 13px; color: #888; margin-top: 4px; }
.branch-detail { margin-top: 10px; }
.detail-title { font-size: 15px; font-weight: 600; color: #333; margin-bottom: 12px; }
.text-warning { color: #e6a23c; font-weight: 600; }
.branch-loading, .branch-placeholder {
  text-align: center; padding: 60px 20px; color: #aaa;
}
.branch-loading p, .branch-placeholder p { margin-top: 12px; font-size: 14px; }
</style>
