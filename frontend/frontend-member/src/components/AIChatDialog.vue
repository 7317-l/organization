<template>
  <div class="ai-chat-dialog">
    <!-- 头部 -->
    <div class="dialog-header">
      <div class="dialog-title">
        <span class="title-dot"></span>
        AI 党建助手
      </div>
      <button class="dialog-close" @click="$emit('close')" aria-label="关闭">✕</button>
    </div>

    <!-- 消息列表 -->
    <div class="dialog-messages" ref="messagesRef">
      <div
        v-for="(msg, idx) in messages"
        :key="idx"
        class="chat-msg"
        :class="msg.role"
      >
        <div class="chat-avatar-sm">
          <span v-if="msg.role === 'ai'">AI</span>
          <span v-else>{{ avatarChar }}</span>
        </div>
        <div class="chat-bubble-wrap">
          <div class="chat-bubble" v-html="formatMessage(msg.content)"></div>
          <div class="chat-time">{{ msg.time }}</div>
        </div>
      </div>

      <!-- 加载中 -->
      <div v-if="loading" class="chat-msg ai">
        <div class="chat-avatar-sm">AI</div>
        <div class="chat-bubble-wrap">
          <div class="chat-bubble typing">
            <span></span><span></span><span></span>
          </div>
        </div>
      </div>

      <div v-if="messages.length === 0 && !loading" class="empty-hint">
        请输入党建相关问题，AI 将结合您的学习数据为您解答
      </div>
    </div>

    <!-- 快捷问题 -->
    <div class="quick-questions" v-if="messages.length === 0">
      <button
        v-for="q in quickQuestions"
        :key="q"
        class="qq-btn"
        @click="sendMessage(q)"
      >
        {{ q }}
      </button>
    </div>

    <!-- 输入框 -->
    <div class="dialog-input">
      <input
        v-model="inputText"
        type="text"
        placeholder="请输入党建问题..."
        @keyup.enter="handleSend"
        :disabled="loading"
      />
      <button class="send-btn" :disabled="loading" @click="handleSend">
        发送
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, nextTick, onMounted, onBeforeUnmount } from 'vue'
import { aiPersonalChat } from '@/api/aiModule'
import { useUserStore } from '@/stores/user'
import { getAvatarChar } from '@/utils/format'

const emit = defineEmits(['close'])

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
  '请根据我的学习情况给出建议'
]

function formatMessage(text) {
  if (!text) return ''
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

  messages.value.push({
    role: 'user',
    content: question,
    time: getCurrentTime()
  })
  inputText.value = ''
  scrollToBottom()

  loading.value = true
  try {
    const answer = await aiPersonalChat(question)
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

function handleKeydown(e) {
  if (e.key === 'Escape') emit('close')
}

onMounted(() => {
  if (!userStore.userInfo) {
    userStore.fetchUserInfo().then(() => {
      avatarChar.value = getAvatarChar(userStore.userName || '用户')
    }).catch(() => {})
  }
  document.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
  document.removeEventListener('keydown', handleKeydown)
})
</script>

<style scoped>
.ai-chat-dialog {
  position: fixed;
  right: 120px;
  bottom: 32px;
  width: 400px;
  height: 580px;
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 16px 48px rgba(0, 0, 0, 0.22);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  z-index: 2600;
  border: 1px solid rgba(200, 22, 29, 0.12);
}

/* 头部 */
.dialog-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 18px;
  background: linear-gradient(135deg, #e53935, #c8161d 55%, #a50f15);
  color: #fff;
  flex-shrink: 0;
}

.dialog-title {
  font-size: 15px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
}

.title-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #fff;
  box-shadow: 0 0 8px rgba(255, 255, 255, 0.9);
}

.dialog-close {
  width: 28px;
  height: 28px;
  border: none;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  color: #fff;
  font-size: 14px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s;
}

.dialog-close:hover {
  background: rgba(255, 255, 255, 0.35);
}

/* 消息区 */
.dialog-messages {
  flex: 1;
  overflow-y: auto;
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  background: #f7f7f9;
}

.empty-hint {
  text-align: center;
  color: #909399;
  font-size: 13px;
  padding: 60px 20px;
  line-height: 1.8;
}

.chat-msg {
  display: flex;
  gap: 10px;
  max-width: 88%;
}

.chat-msg.user {
  align-self: flex-end;
  flex-direction: row-reverse;
}

.chat-avatar-sm {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  font-size: 12px;
  font-weight: 700;
}

.chat-msg.ai .chat-avatar-sm {
  background: #fdeaea;
  color: #c8161d;
}

.chat-msg.user .chat-avatar-sm {
  background: #c8161d;
  color: #fff;
}

.chat-bubble-wrap {
  min-width: 0;
}

.chat-bubble {
  padding: 12px 15px;
  border-radius: 12px;
  font-size: 13px;
  line-height: 1.7;
  word-break: break-word;
}

.chat-msg.ai .chat-bubble {
  background: #fff8e1;
  color: #303133;
  border-top-left-radius: 4px;
}

.chat-msg.user .chat-bubble {
  background: #c8161d;
  color: #fff;
  border-top-right-radius: 4px;
}

.chat-time {
  font-size: 11px;
  color: #b0b3b8;
  margin-top: 4px;
}

.chat-msg.user .chat-time {
  text-align: right;
}

.chat-bubble.typing {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 14px 16px;
}

.chat-bubble.typing span {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #c8161d;
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
  padding: 10px 16px;
  border-top: 1px solid #eee;
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  flex-shrink: 0;
  background: #fff;
}

.qq-btn {
  padding: 6px 12px;
  border: 1px solid #f1c9cb;
  border-radius: 14px;
  background: #fff8f8;
  color: #c8161d;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.15s;
}

.qq-btn:hover {
  background: #c8161d;
  color: #fff;
  border-color: #c8161d;
}

/* 输入框 */
.dialog-input {
  padding: 12px 16px;
  border-top: 1px solid #eee;
  display: flex;
  align-items: center;
  gap: 10px;
  background: #fff;
  flex-shrink: 0;
}

.dialog-input input {
  flex: 1;
  padding: 10px 14px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  font-size: 13px;
  background: #f7f7f9;
  outline: none;
  transition: all 0.15s;
}

.dialog-input input:focus {
  border-color: #c8161d;
  background: #fff;
}

.dialog-input input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.send-btn {
  padding: 10px 18px;
  border: none;
  border-radius: 8px;
  background: linear-gradient(135deg, #e53935, #c8161d);
  color: #fff;
  font-size: 13px;
  cursor: pointer;
  transition: opacity 0.15s;
}

.send-btn:hover {
  opacity: 0.88;
}

.send-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 600px) {
  .ai-chat-dialog {
    right: 12px;
    left: 12px;
    bottom: 12px;
    width: auto;
    height: calc(100vh - 90px);
  }
}
</style>
