<template>
  <div class="ai-chat-page">
    <div class="back-link" @click="goBack">← 返回</div>
    <div class="page-title" style="margin-bottom: 20px">AI 党建助手</div>

    <div class="chat-container">
      <!-- 聊天头部 -->
      <div class="chat-header">
        <div class="chat-avatar">
          <el-icon :size="20"><ChatDotRound /></el-icon>
        </div>
        <div>
          <div class="chat-title">AI 党建助手</div>
          <div class="chat-status">
            <span class="status-dot"></span>
            在线 · 随时为您服务
          </div>
        </div>
      </div>

      <!-- 消息列表 -->
      <div class="chat-messages" ref="messagesRef">
        <div
          v-for="(msg, idx) in messages"
          :key="idx"
          class="chat-msg"
          :class="msg.role"
        >
          <div class="chat-avatar-sm">
            <el-icon v-if="msg.role === 'ai'" :size="16"><ChatDotRound /></el-icon>
            <span v-else>{{ avatarChar }}</span>
          </div>
          <div class="chat-bubble-wrap">
            <div class="chat-bubble" v-html="formatMessage(msg.content)"></div>
            <div class="chat-time">{{ msg.time }}</div>
          </div>
        </div>

        <!-- 加载中 -->
        <div v-if="loading" class="chat-msg ai">
          <div class="chat-avatar-sm">
            <el-icon :size="16"><ChatDotRound /></el-icon>
          </div>
          <div class="chat-bubble-wrap">
            <div class="chat-bubble typing">
              <span></span><span></span><span></span>
            </div>
          </div>
        </div>

        <el-empty
          v-if="messages.length === 0 && !loading"
          description="请输入党建相关问题，AI 助手将为您解答"
          :image-size="100"
        />
      </div>

      <!-- 快捷问题 -->
      <div class="quick-questions" v-if="messages.length === 0">
        <span class="qq-label">常见问题：</span>
        <el-button
          v-for="q in quickQuestions"
          :key="q"
          size="small"
          type="primary"
          plain
          @click="sendQuickQuestion(q)"
        >
          {{ q }}
        </el-button>
      </div>

      <!-- 输入框 -->
      <div class="chat-input">
        <input
          v-model="inputText"
          type="text"
          placeholder="请输入党建问题..."
          @keyup.enter="handleSend"
          :disabled="loading"
        />
        <el-button type="primary" :loading="loading" @click="handleSend">
          发送
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, nextTick, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ChatDotRound } from '@element-plus/icons-vue'
import { aiQuery } from '@/api/ai'
import { useUserStore } from '@/stores/user'
import { getAvatarChar } from '@/utils/format'

const router = useRouter()
const userStore = useUserStore()

const messages = ref([])
const inputText = ref('')
const loading = ref(false)
const messagesRef = ref(null)

const avatarChar = ref(getAvatarChar(userStore.userName || '用户'))

const quickQuestions = [
  '什么是四个意识？',
  '什么是两个维护？',
  '党的初心和使命是什么？',
  '新时代党的建设总要求是什么？'
]

function formatMessage(text) {
  if (!text) return ''
  // 简单的换行处理
  return text.replace(/\n/g, '<br>')
}

function getCurrentTime() {
  const now = new Date()
  return `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`
}

function scrollToBottom() {
  nextTick(() => {
    if (messagesRef.value) {
      messagesRef.value.scrollTop = messagesRef.value.scrollHeight
    }
  })
}

async function sendMessage(question) {
  if (!question.trim() || loading.value) return

  // 添加用户消息
  messages.value.push({
    role: 'user',
    content: question,
    time: getCurrentTime()
  })
  inputText.value = ''
  scrollToBottom()

  loading.value = true
  try {
    const data = await aiQuery(question)
    const answer = data?.answer || data?.content || data?.message || '抱歉，我暂时无法回答这个问题。'
    messages.value.push({
      role: 'ai',
      content: answer,
      time: getCurrentTime()
    })
  } catch {
    messages.value.push({
      role: 'ai',
      content: '抱歉，服务暂时不可用，请稍后重试。',
      time: getCurrentTime()
    })
  } finally {
    loading.value = false
    scrollToBottom()
  }
}

function handleSend() {
  sendMessage(inputText.value)
}

function sendQuickQuestion(q) {
  sendMessage(q)
}

function goBack() {
  router.back()
}

onMounted(() => {
  if (!userStore.userInfo) {
    userStore.fetchUserInfo().then(() => {
      avatarChar.value = getAvatarChar(userStore.userName || '用户')
    }).catch(() => {})
  }
})
</script>

<style scoped>
.ai-chat-page {
  padding-bottom: 24px;
  height: calc(100vh - 104px);
  display: flex;
  flex-direction: column;
}

.back-link {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  color: var(--t2);
  font-size: 14px;
  cursor: pointer;
  margin-bottom: 12px;
  transition: color 0.15s;
  flex-shrink: 0;
}

.back-link:hover {
  color: var(--red);
}

.page-title {
  font-size: 22px;
  font-weight: 600;
  flex-shrink: 0;
}

.chat-container {
  background: var(--card);
  border-radius: var(--r10);
  box-shadow: var(--sh);
  display: flex;
  flex-direction: column;
  flex: 1;
  overflow: hidden;
}

.chat-header {
  padding: 16px 20px;
  border-bottom: 1px solid var(--bd);
  display: flex;
  align-items: center;
  gap: 12px;
  flex-shrink: 0;
}

.chat-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--red-10);
  color: var(--red);
  display: flex;
  align-items: center;
  justify-content: center;
}

.chat-title {
  font-size: 15px;
  font-weight: 600;
}

.chat-status {
  font-size: 12px;
  color: var(--t3);
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 2px;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--green);
  display: inline-block;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.chat-msg {
  display: flex;
  gap: 12px;
  max-width: 85%;
}

.chat-msg.user {
  align-self: flex-end;
  flex-direction: row-reverse;
}

.chat-avatar-sm {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  font-size: 14px;
  font-weight: 600;
}

.chat-msg.ai .chat-avatar-sm {
  background: var(--red-10);
  color: var(--red);
}

.chat-msg.user .chat-avatar-sm {
  background: var(--blue);
  color: #fff;
}

.chat-bubble-wrap {
  min-width: 0;
}

.chat-bubble {
  padding: 14px 18px;
  border-radius: 12px;
  font-size: 14px;
  line-height: 1.7;
  word-break: break-word;
}

.chat-msg.ai .chat-bubble {
  background: #fff8e1;
  color: var(--t1);
  border-top-left-radius: 4px;
}

.chat-msg.user .chat-bubble {
  background: var(--blue);
  color: #fff;
  border-top-right-radius: 4px;
}

.chat-time {
  font-size: 11px;
  color: var(--t3);
  margin-top: 4px;
}

.chat-msg.user .chat-time {
  text-align: right;
}

/* 打字动画 */
.chat-bubble.typing {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 16px 18px;
}

.chat-bubble.typing span {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--t3);
  animation: typing 1.4s infinite;
}

.chat-bubble.typing span:nth-child(2) {
  animation-delay: 0.2s;
}

.chat-bubble.typing span:nth-child(3) {
  animation-delay: 0.4s;
}

@keyframes typing {
  0%, 60%, 100% {
    transform: translateY(0);
    opacity: 0.4;
  }
  30% {
    transform: translateY(-6px);
    opacity: 1;
  }
}

/* 快捷问题 */
.quick-questions {
  padding: 12px 20px;
  border-top: 1px solid var(--bd);
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  flex-shrink: 0;
}

.qq-label {
  font-size: 13px;
  color: var(--t3);
  flex-shrink: 0;
}

/* 输入框 */
.chat-input {
  padding: 16px 20px;
  border-top: 1px solid var(--bd);
  display: flex;
  align-items: center;
  gap: 12px;
  flex-shrink: 0;
}

.chat-input input {
  flex: 1;
  padding: 12px 16px;
  border: 1px solid var(--bd);
  border-radius: 8px;
  font-size: 14px;
  background: var(--bg);
  outline: none;
  transition: all 0.15s;
}

.chat-input input:focus {
  border-color: var(--red);
  background: #fff;
}

.chat-input input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
