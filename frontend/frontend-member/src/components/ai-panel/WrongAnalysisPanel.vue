<template>
  <div>
    <div class="aip-section-title">错题分析</div>
    <p class="aip-section-desc">AI 会感应到你正在做的题或历史错题，点一下即可对单题进行<strong>深度解析</strong>：考点定位、逐项讲解、易错提醒</p>

    <!-- 当前正在做的题 -->
    <div v-if="currentQuestion" class="aip-data">
      <div class="aip-data-title">
        正在做的题<template v-if="currentQuestion.questionNumber"> · 第 {{ currentQuestion.questionNumber }} 题</template>
        <span v-if="currentQuestion.submitted" style="margin-left:8px;font-size:12px;color:#67c23a;">已提交{{ currentQuestion.correct === true ? ' · 回答正确' : ' · 回答错误' }}</span>
        <span v-else style="margin-left:8px;font-size:12px;color:#e6a23c;">答题中</span>
      </div>
      <div style="font-size:14px;line-height:1.8;color:#333;margin-bottom:10px;">{{ currentQuestion.question }}</div>
      <div v-if="optionTexts.length" style="margin-bottom:10px;">
        <div v-for="(opt, i) in optionTexts" :key="i" class="aip-line">
          <span class="aip-line-tag" :class="letterOf(i) === currentQuestion.userAnswer ? 'warn' : 'good'">{{ letterOf(i) }}</span>
          <span>{{ opt }}</span>
        </div>
      </div>
      <div class="aip-line">
        <span class="aip-line-tag warn">我的答案</span>
        <span>{{ currentQuestion.userAnswer || '未作答' }}</span>
      </div>
      <div v-if="currentQuestion.correctAnswer" class="aip-line">
        <span class="aip-line-tag good">正确答案</span>
        <span>{{ currentQuestion.correctAnswer }}</span>
      </div>
      <div v-if="currentQuestion.knowledgePoint" class="aip-line">
        <span class="aip-line-tag good">知识点</span>
        <span>{{ currentQuestion.knowledgePoint }}</span>
      </div>
      <div style="margin-top:12px;">
        <el-button type="primary" :loading="loading" @click="analyzeCurrent">
          <el-icon v-if="!loading" style="margin-right:4px;"><MagicStick /></el-icon> AI 分析本题
        </el-button>
      </div>
    </div>

    <!-- 没在读题但历史有错题 -->
    <div v-else-if="wrongAnswers.length" class="aip-data" style="border-color:#f0dcdd;background:#fef7f7;">
      <div class="aip-data-title">近期错题（点击即可分析）</div>
      <div v-for="(w, i) in wrongAnswers.slice(0, 8)" :key="i" class="aip-section-card" style="cursor:pointer;" @click="analyzeWrong(w)">
        <div class="aip-sec-head">
          <span class="aip-sec-title">{{ w.question || '（无题干）' }}</span>
          <span class="aip-sec-min">我的：{{ w.userAnswer || '-' }} / 正确：{{ w.correctAnswer || '-' }}</span>
        </div>
        <div v-if="w.knowledgePoint" class="aip-sec-content">知识点：{{ w.knowledgePoint }}</div>
      </div>
    </div>

    <!-- 空态 -->
    <div v-else class="aip-empty">
      <p>暂未检测到题目。请先在「练习」或「考试中心」中做题，AI 会自动感应当前题目；也可以先完成一次测验，历史错题会出现在这里。</p>
    </div>

    <!-- 分析输出 -->
    <div v-if="analysisMsg.answer" class="aip-result" style="margin-top:16px;">
      <div class="aip-toolbar">
        <div class="aip-result-title">AI 错题分析</div>
        <span v-if="analysisMsg.streaming" class="thinking-hint">分析中…</span>
      </div>
      <div class="aip-block" style="border-bottom:none;">
        <div class="aip-text">{{ analysisMsg.answer }}<span v-if="analysisMsg.streaming" class="stream-cursor">▍</span></div>
      </div>
    </div>
    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>AI 正在分析这道题...</p>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onUnmounted } from 'vue'
import { Loading, MagicStick } from '@element-plus/icons-vue'
import { apiPostStream } from './common'
import { useAiDataStore } from '@/stores/aiData'

const aiDataStore = useAiDataStore()
const loading = ref(false)
const analysisMsg = reactive({ answer: '', streaming: false })

const currentQuestion = computed(() => aiDataStore.currentQuestion)
const wrongAnswers = computed(() => aiDataStore.wrongAnswers)

const optionTexts = computed(() => {
  const opts = currentQuestion.value?.options || []
  return opts.map(o => {
    if (typeof o === 'string') return o
    return o?.text || o?.content || JSON.stringify(o)
  }).filter(t => t)
})

function letterOf(i) {
  return String.fromCharCode(65 + i)
}

function optionLetters(options) {
  return options.map((_, i) => String.fromCharCode(65 + i))
}

function extractText(o) {
  if (typeof o === 'string') return o
  return o?.text || o?.content || ''
}

// ===== 逐字打字机（与知识问答一致） =====
let rafId = null
const typingState = { msg: null, chars: [], done: false }

function rafTick() {
  const st = typingState
  if (!st.msg) { rafId = null; return }
  if (st.chars.length > 0) {
    const n = Math.max(1, Math.ceil(st.chars.length / 20))
    st.msg.answer += st.chars.splice(0, n).join('')
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

function runAnalyze(payload) {
  if (loading.value) return
  loading.value = true
  stopTyping()
  analysisMsg.answer = ''
  analysisMsg.streaming = true
  apiPostStream('/ai-knowledge/analyze-stream', payload, {
    onDelta(text) { feedTyping(analysisMsg, text) },
    onDone(d) {
      if (d?.fallback) {
        stopTyping()
        analysisMsg.answer = d.fallback
        analysisMsg.streaming = false
      } else {
        finishTyping(analysisMsg)
      }
    },
    onError(message) {
      stopTyping()
      if (!analysisMsg.answer) analysisMsg.answer = message || '分析生成失败'
    }
  }).catch(e => {
    stopTyping()
    if (!analysisMsg.answer) analysisMsg.answer = e?.message || '分析失败'
  }).finally(() => {
    loading.value = false
  })
}

function analyzeCurrent() {
  const q = currentQuestion.value
  if (!q || !q.question) return
  runAnalyze({
    question: q.question,
    options: (q.options || []).map(extractText).filter(t => t),
    userAnswer: q.userAnswer || '',
    correctAnswer: q.correctAnswer || '',
    knowledgePoint: q.knowledgePoint || '',
    sessionId: undefined
  })
}

function analyzeWrong(w) {
  if (!w || !w.question) return
  runAnalyze({
    question: w.question,
    options: (w.options || []).map(extractText).filter(t => t),
    userAnswer: w.userAnswer || '',
    correctAnswer: w.correctAnswer || '',
    knowledgePoint: w.knowledgePoint || '',
    sessionId: undefined
  })
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
  font-size: 12px;
}
</style>
