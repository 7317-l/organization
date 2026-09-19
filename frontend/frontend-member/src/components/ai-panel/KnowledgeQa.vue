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
            <div style="white-space:pre-wrap;">
              <template v-if="msg.answer">{{ msg.answer }}<span v-if="msg.streaming" class="stream-cursor">▍</span></template>
              <span v-else-if="msg.connecting" class="thinking-hint">正在连接AI服务…</span>
              <span v-else-if="msg.streaming" class="thinking-hint">正在思考</span>
            </div>
            <div v-if="msg.answer && msg.questionType !== 'chat' && msg.sources && msg.sources.length" class="aip-source-ref">
              引用来源：
              <span v-for="(s, si) in msg.sources" :key="si">{{ s }}</span>
            </div>
            <div v-if="msg.answer && msg.questionType !== 'chat' && msg.knowledgeBased" class="aip-source-ref">回答基于本地党建知识库检索生成</div>
          </template>
        </div>
      </div>
      <div v-if="loading" class="aip-chat-msg ai">
        <div class="aip-msg-avatar">AI</div>
        <div class="aip-msg-bubble typing"><span></span><span></span><span></span></div>
      </div>
    </div>

    <div class="aip-version-tag">v3.3</div>
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
import { ref, reactive, nextTick, onUnmounted } from 'vue'
// KnowledgeQa v3.3（8秒超时 + 请求状态分步显示）
console.log('%c[KnowledgeQa] v3.3 loaded', 'color:#C8161D;font-weight:bold')
import { ElMessage } from 'element-plus'
import { apiPost, apiPostStream } from './common'
import VoiceInput from './VoiceInput.vue'

const question = ref('')
const loading = ref(false)
const messages = ref([])
const chatRef = ref(null)
let sessionId = ''

async function send() {
  const q = question.value.trim()
  if (!q || loading.value) return
  // 终止上一轮未打完的字
  stopTyping()
  messages.value.push({ role: 'user', question: q })
  question.value = ''
  loading.value = true
  // 预创建 AI 消息，由打字机逐字填充 answer
  const aiMsg = reactive({ role: 'ai', answer: '', sources: [], confidence: 0, streaming: true, connecting: true, questionType: 'knowledge', knowledgeBased: false })
  messages.value.push(aiMsg)
  scroll()
  try {
    await apiPostStream('/ai-knowledge/query-stream', {
      question: q,
      sessionId: sessionId || undefined,
      topK: 5,
      rerank: true,
      role: 'member'
    }, {
      onMeta(meta) {
        loading.value = false
        aiMsg.connecting = false
        sessionId = meta?.sessionId || sessionId
        aiMsg.questionType = meta?.questionType || 'knowledge'
        aiMsg.sources = Array.isArray(meta?.sourceReferences) ? meta.sourceReferences.slice(0, 8) : []
        aiMsg.confidence = meta?.confidence || 0
        // 知识类且检索到依据时才显示"基于知识库"标签；寒暄类不显示
        aiMsg.knowledgeBased = aiMsg.questionType !== 'chat' && (aiMsg.sources.length > 0 || aiMsg.confidence > 0)
        scroll()
      },
      onDelta(text) {
        feedTyping(aiMsg, text)
      },
      onDone(d) {
        if (d?.fallback) {
          // 兜底答案直接展示，不再逐字
          stopTyping()
          aiMsg.answer = d.fallback
        } else {
          finishTyping(aiMsg)
        }
        if (!aiMsg.answer) aiMsg.answer = '未获取到答案，请换个问法试试'
        scroll()
      },
      onError(message) {
        stopTyping()
        if (!aiMsg.answer) aiMsg.answer = message || '回答生成失败'
        scroll()
      }
    })
  } catch (e) {
    stopTyping()
    const errMsg = e?.message || '回答生成失败'
    if (!aiMsg.answer) {
      aiMsg.answer = errMsg
    } else {
      // 已有部分内容被中断时，明确追加中断提示
      aiMsg.answer += '\n\n（' + errMsg + '）'
    } '查询失败，请确认后端服务已启动（端口5091）'
  } finally {
    loading.value = false
    scroll()
  }
}


function scroll() {
  nextTick(() => { if (chatRef.value) chatRef.value.scrollTop = chatRef.value.scrollHeight })
}

// ===== 逐字打字机（requestAnimationFrame 驱动 + 自适应批量，抗网络抖动）=====
let rafId = null
const typingState = { msg: null, chars: [], done: false }

function rafTick() {
  const st = typingState
  if (!st.msg) { rafId = null; return }
  if (st.chars.length > 0) {
    // 自适应：积压多时每帧多吐几个（约 20 帧/0.33 秒追平），积压少时每帧 1 个保持逐字感
    const n = Math.max(1, Math.ceil(st.chars.length / 20))
    st.msg.answer += st.chars.splice(0, n).join('')
    scroll()
  }
  if (st.chars.length === 0 && st.done) {
    st.msg.streaming = false
    st.msg = null
    rafId = null
    return
  }
  rafId = requestAnimationFrame(rafTick)
}

function ensureRaf() {
  if (!rafId) rafId = requestAnimationFrame(rafTick)
}

function feedTyping(msg, text) {
  typingState.msg = msg
  typingState.done = false
  // Array.from 按码点切分，emoji 等代理对字符不会被拆坏
  for (const ch of Array.from(text)) typingState.chars.push(ch)
  ensureRaf()
}

function finishTyping(msg) {
  typingState.done = true
  typingState.msg = typingState.msg || msg
  if (typingState.chars.length === 0) {
    if (rafId) { cancelAnimationFrame(rafId); rafId = null }
    msg.streaming = false
    typingState.msg = null
    return
  }
  ensureRaf()
}

function stopTyping() {
  if (rafId) { cancelAnimationFrame(rafId); rafId = null }
  if (typingState.msg) typingState.msg.streaming = false
  typingState.msg = null
  typingState.chars = []
  typingState.done = false
}

onUnmounted(stopTyping)
</script>

<style scoped>
.stream-cursor {
  display: inline-block;
  margin-left: 2px;
  color: #C8161D;
  font-weight: 600;
  animation: stream-blink 0.8s step-start infinite;
}
@keyframes stream-blink {
  50% { opacity: 0; }
}
.thinking-hint {
  color: #999;
  font-size: 13px;
  opacity: 0.85;
}
.aip-version-tag {
  position: absolute;
  right: 8px;
  bottom: 8px;
  font-size: 10px;
  color: #bbb;
  z-index: 1;
}
</style>
