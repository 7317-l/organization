<template>
  <div>
    <div class="aip-section-title">AI党建知识问答</div>
    <p class="aip-section-desc">基于党建知识库检索（文档切片 → 语义检索 → 结果重排 → 大模型回答），回答<strong>显示引用来源</strong>，支持多轮追问</p>

    <div class="aip-chat-messages" ref="chatRef">
      <div class="aip-chat-msg ai">
        <div class="aip-msg-avatar">AI</div>
        <div class="aip-msg-bubble">您好，我是党建知识助手。可询问党史、党章、党的理论、政策精神等，例如"简述入党誓词内容"、"什么是三会一课"。回答会附上知识库引用来源。</div>
      </div>
      <div v-for="(msg, idx) in messages" :key="idx" class="aip-chat-msg" :class="msg.role">
        <div class="aip-msg-avatar">{{ msg.role === 'user' ? '我' : 'AI' }}</div>
        <div class="aip-msg-bubble">
          <div v-if="msg.role === 'user'">{{ msg.question }}</div>
          <template v-else>
            <div style="white-space:pre-wrap;">{{ msg.answer }}</div>
            <div v-if="msg.sources && msg.sources.length" class="aip-source-ref">
              引用来源：
              <span v-for="(s, si) in msg.sources" :key="si">{{ s }}</span>
            </div>
            <div v-if="msg.confidence" class="aip-source-ref">回答置信度：{{ Math.round(msg.confidence * 100) }}%</div>
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
        v-model="question"
        type="textarea"
        :rows="2"
        placeholder="输入党建问题，例如：什么是党员的义务？"
        @keydown.enter.exact.prevent="send"
      />
      <VoiceInput v-model="question" />
      <el-button type="primary" :loading="loading" @click="send">提问</el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import { apiPost } from './common'
import VoiceInput from './VoiceInput.vue'

const question = ref('')
const loading = ref(false)
const messages = ref([])
const chatRef = ref(null)
let sessionId = ''

async function send() {
  const q = question.value.trim()
  if (!q || loading.value) return
  messages.value.push({ role: 'user', question: q })
  question.value = ''
  loading.value = true
  scroll()
  try {
    const data = await apiPost('/ai-knowledge/query', {
      question: q,
      sessionId: sessionId || undefined,
      topK: 5,
      rerank: true
    })
    sessionId = data?.sessionId || sessionId
    const sources = Array.isArray(data?.sourceReferences) ? data.sourceReferences : []
    if (!sources.length && Array.isArray(data?.results)) {
      data.results.forEach(r => {
        const s = r?.title || r?.file || r?.source
        if (s && !sources.includes(s)) sources.push(s)
      })
    }
    messages.value.push({
      role: 'ai',
      answer: data?.answer || '未获取到答案，请换个问法试试',
      sources: sources.slice(0, 8),
      confidence: data?.confidence || 0
    })
  } catch (e) {
    ElMessage.error(e?.message || '查询失败，请确认后端服务已启动（端口5091）')
  } finally {
    loading.value = false
    scroll()
  }
}

function scroll() {
  nextTick(() => { if (chatRef.value) chatRef.value.scrollTop = chatRef.value.scrollHeight })
}
</script>
