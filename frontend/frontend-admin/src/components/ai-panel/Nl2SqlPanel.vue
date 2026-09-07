<template>
  <div>
    <div class="aip-section-title">AI自然语言数据查询</div>
    <p class="aip-section-desc">用自然语言提问，AI 自动生成 SQL 并查询数据，支持多轮追问；含<strong>只读 SELECT 限制、危险关键字拦截、表/字段白名单、查询条数限制</strong>等安全机制</p>

    <div class="aip-chat-messages" ref="chatRef">
      <div class="aip-chat-msg ai">
        <div class="aip-msg-avatar">AI</div>
        <div class="aip-msg-bubble">您好，我是数据查询助手。您可以用自然语言提问，例如"各支部学习完成率排名"、"本月测验平均分"等，我会自动查询数据并展示结果。</div>
      </div>
      <div v-for="(msg, idx) in messages" :key="idx" class="aip-chat-msg" :class="msg.role">
        <div class="aip-msg-avatar">{{ msg.role === 'user' ? '管' : 'AI' }}</div>
        <div class="aip-msg-bubble">
          <div v-if="msg.role === 'user'">{{ msg.question }}</div>
          <template v-else>
            <div v-if="msg.explanation" class="aip-result-explain">{{ msg.explanation }}</div>
            <div v-if="msg.generatedSql" class="aip-result-sql">
              <details>
                <summary style="cursor:pointer;color:#888;font-size:12px;">查看生成SQL</summary>
                <pre class="aip-sql-code">{{ msg.generatedSql }}</pre>
              </details>
            </div>
            <div v-if="msg.chartData && msg.chartData.labels && msg.chartData.labels.length > 0" class="aip-result-chart">
              <div class="aip-chart-title">数据图表</div>
              <div class="aip-bar-chart">
                <div v-for="(label, i) in msg.chartData.labels" :key="i" class="aip-bar-row">
                  <span class="aip-bar-label">{{ label }}</span>
                  <div class="aip-bar-track">
                    <div class="aip-bar-fill" :style="{ width: getBarWidth(msg.chartData.values, i) + '%' }"></div>
                  </div>
                  <span class="aip-bar-value">{{ msg.chartData.values[i] }}</span>
                </div>
              </div>
            </div>
            <div v-if="msg.resultData && msg.resultData.length > 0" class="aip-table-wrap">
              <div class="aip-chart-title">查询结果（{{ msg.resultData.length }}条）</div>
              <table class="aip-table">
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
              <div v-if="msg.resultData.length > 20" class="aip-table-more">仅显示前20条，共{{ msg.resultData.length }}条</div>
            </div>
            <div v-if="msg.corrections && msg.corrections.length > 0" class="aip-result-corrections">
              <el-tag size="small" type="warning">已自动修正：{{ msg.corrections.join('、') }}</el-tag>
            </div>
            <div v-if="msg.masked" class="aip-source-ref">已对敏感字段（手机号等）自动脱敏展示</div>
          </template>
        </div>
      </div>
      <div v-if="loading" class="aip-chat-msg ai">
        <div class="aip-msg-avatar">AI</div>
        <div class="aip-msg-bubble typing"><span></span><span></span><span></span></div>
      </div>
    </div>

    <div class="aip-chat-input">
      <el-input
        v-model="input"
        type="textarea"
        :rows="2"
        placeholder="输入您想查询的数据问题，例如：各支部学习完成率排名..."
        @keydown.enter.exact.prevent="sendQuery"
      />
      <VoiceInput v-model="input" />
      <el-button type="primary" :loading="loading" @click="sendQuery">查询</el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import { apiPost, maskPhone } from './common'
import VoiceInput from './VoiceInput.vue'

const input = ref('')
const loading = ref(false)
const messages = ref([])
const chatRef = ref(null)
let sessionId = ''

async function sendQuery() {
  const q = input.value.trim()
  if (!q || loading.value) return
  messages.value.push({ role: 'user', question: q })
  input.value = ''
  loading.value = true
  scroll()
  try {
    const data = await apiPost('/nl2sql/query', { naturalLanguage: q, sessionId: sessionId || undefined })
    sessionId = data?.sessionId || sessionId
    const resultData = (data?.resultData || []).map(row => {
      const masked = {}
      Object.entries(row).forEach(([k, v]) => {
        masked[k] = /phone|手机/i.test(k) ? maskPhone(String(v)) : v
      })
      return masked
    })
    messages.value.push({
      role: 'ai',
      explanation: data?.explanation,
      generatedSql: data?.generatedSql,
      resultData,
      chartData: data?.chartData,
      corrections: data?.correctionsApplied || [],
      masked: resultData.length > 0
    })
  } catch (e) {
    messages.value.push({ role: 'ai', explanation: e?.message || '查询失败，请稍后重试' })
  } finally {
    loading.value = false
    scroll()
  }
}

function scroll() {
  nextTick(() => { if (chatRef.value) chatRef.value.scrollTop = chatRef.value.scrollHeight })
}

function fieldLabel(key) {
  const map = {
    org_name: '组织', name: '名称', member_name: '姓名', member_count: '党员数',
    completion_rate: '完成率(%)', avg_score: '平均分', score: '成绩', exam_count: '考试次数',
    date: '日期', minutes: '分钟', member_type: '身份', is_enabled: '启用',
    total_tasks: '任务数', completed: '已完成', total: '总数', phone: '手机号(脱敏)'
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
</script>
