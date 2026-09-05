<template>
  <div class="content-detail" v-loading="loading">
    <div class="back-link" @click="goBack">← 返回课程列表</div>

    <div class="page-header" style="margin-bottom: 20px">
      <div class="page-title" style="font-size: 20px">{{ content.title }}</div>
    </div>

    <!-- 视频播放器 -->
    <div v-if="isVideo && content.videoUrl" class="video-player">
      <video
        ref="videoRef"
        :src="content.videoUrl"
        controls
        class="video-element"
        @timeupdate="handleTimeUpdate"
        @ended="handleVideoEnded"
      >
        您的浏览器不支持视频播放
      </video>
    </div>

    <!-- 视频占位（无视频URL时） -->
    <div v-else-if="isVideo" class="video-player video-poster">
      <h2>{{ content.title }}</h2>
      <p>视频课程</p>
      <div class="video-play-btn" @click="ElMessage.info('视频资源加载中')">
        <el-icon :size="32"><VideoPlay /></el-icon>
      </div>
    </div>

    <!-- 文章内容 -->
    <div v-else class="article-content" v-html="content.content || content.body || content.description"></div>

    <!-- 课程信息 -->
    <div class="course-info">
      <div class="course-meta">
        <el-tag v-if="contentTypeLabel" type="danger" effect="light" size="small">
          {{ contentTypeLabel }}
        </el-tag>
        <el-tag v-for="(tag, idx) in parsedTags" :key="idx" size="small" effect="light">
          {{ tag }}
        </el-tag>
        <span class="meta-date">{{ formatDate(content.publishTime || content.createdAt) }}</span>
      </div>

      <h4 v-if="content.description" class="course-desc-title">课程简介：</h4>
      <p v-if="content.description" class="course-desc">{{ content.description }}</p>

      <div class="progress-section">
        <span class="progress-label">学习进度：</span>
        <div class="progress-bar-wrap">
          <el-progress :percentage="currentProgress" :show-text="false" :stroke-width="8" color="#C8161D" />
        </div>
        <span class="progress-value">{{ currentProgress }}%</span>
        <el-button
          v-if="!reported"
          type="primary"
          size="small"
          :loading="reporting"
          @click="reportLearningProgress"
        >
          上报进度
        </el-button>
        <el-tag v-else type="success" effect="light" size="small">已上报 ✓</el-tag>
        <el-button
          v-if="content.taskId && currentProgress >= 100 && reported"
          type="success"
          size="small"
          :loading="completing"
          @click="completeTask"
        >
          标记任务完成
        </el-button>
      </div>
      <div v-if="!isVideo && !reported && currentProgress < 100" class="progress-hint">
        {{ progressHint }}
      </div>
    </div>

    <!-- AI 解读入口 -->
    <div class="ai-insight" @click="goAiChat">
      <div class="ai-insight-icon">
        <el-icon :size="24"><ChatDotRound /></el-icon>
      </div>
      <div class="ai-insight-content">
        <div class="ai-insight-title">AI 解读本章内容</div>
        <div class="ai-insight-desc">点击与 AI 党建助手对话，深入理解本内容</div>
      </div>
      <div class="ai-insight-arrow">→</div>
    </div>

    <!-- 相关推荐 -->
    <div class="related-list" v-if="relatedList.length > 0">
      <h4>相关推荐：</h4>
      <div
        v-for="item in relatedList"
        :key="item.id"
        class="related-item"
        @click="goRelated(item.id)"
      >
        <div class="related-dot">
          <el-icon><VideoPlay v-if="isVideoContent(item)" /><Document v-else /></el-icon>
        </div>
        <div class="title">{{ item.title }}</div>
        <el-tag size="small" :type="isVideoContent(item) ? 'danger' : 'primary'" effect="light">
          {{ isVideoContent(item) ? '视频' : '文章' }}
        </el-tag>
      </div>
    </div>

    <!-- 防挂机验证弹窗 -->
    <el-dialog
      v-model="antiCheatVisible"
      title="防挂机验证"
      width="500px"
      :close-on-click-modal="false"
      :close-on-press-escape="false"
    >
      <div v-loading="antiCheatLoading">
        <p style="color:#666;margin-bottom:16px">学习时长超过2分钟，请完成验证以继续计时：</p>
        <div v-if="antiCheatQuestion" class="anti-cheat-question">{{ antiCheatQuestion }}</div>
        <el-radio-group v-model="antiCheatSelected" class="anti-cheat-options">
          <el-radio
            v-for="(opt, idx) in antiCheatOptions"
            :key="idx"
            :label="opt.key !== undefined ? opt.key : idx"
            class="anti-cheat-option"
          >
            {{ opt.text || opt }}
          </el-radio>
        </el-radio-group>
        <div v-if="antiCheatCountdown > 0" style="color:#999;font-size:12px;margin-top:12px">
          倒计时：{{ antiCheatCountdown }}秒
        </div>
      </div>
      <template #footer>
        <el-button type="primary" :loading="antiCheatLoading" @click="submitAntiCheatAnswer">提交验证</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { VideoPlay, Document, ChatDotRound } from '@element-plus/icons-vue'
import { getContentDetail, reportProgress } from '@/api/content'
import { completeTask as apiCompleteTask } from '@/api/task'
import { getRecommendations } from '@/api/mobile'
import { formatDate, isVideoContent } from '@/utils/format'
import request from '@/api/request'

const route = useRoute()
const router = useRouter()

const contentId = computed(() => route.params.id)
const loading = ref(false)
const reporting = ref(false)
const completing = ref(false)
const content = ref({})
const currentProgress = ref(0)
const reported = ref(false)
const progressHint = ref('完成本内容学习后，请点击「上报进度」上报学习进度')
const relatedList = ref([])
const videoRef = ref(null)
let progressTimer = null
let articleTimer = null
let lastReportedTime = 0

// 防挂机相关
const antiCheatVisible = ref(false)
const antiCheatQuestion = ref('')
const antiCheatOptions = ref([])
const antiCheatRecordId = ref(null)
const antiCheatSelected = ref(null)
const antiCheatLoading = ref(false)
const antiCheatCountdown = ref(60)
let antiCheatTimer = null
let pendingReportData = null

/** 是否为视频内容（兼容 contentType 数字和 type 字符串） */
const isVideo = computed(() => isVideoContent(content.value))

/** 内容类型中文标签 */
const contentTypeLabel = computed(() => {
  if (content.value.contentType !== undefined) return content.value.contentType === 1 ? '视频' : '文章'
  if (content.value.type) return content.value.type === 'video' ? '视频' : '文章'
  return ''
})

/** 解析 tags：兼容字符串数组和对象数组 */
const parsedTags = computed(() => {
  const tags = content.value.tags || []
  return tags.map(t => {
    if (typeof t === 'string') return t
    if (t && typeof t === 'object') return t.name || t.label || JSON.stringify(t)
    return String(t)
  }).filter(Boolean)
})

// 防挂机API
async function getAntiCheatChallenge(cid) {
  return request.get('/anti-cheat/challenge-v2', { params: { contentId: cid } })
}

async function verifyAntiCheat(data) {
  return request.post('/anti-cheat/verify-v2', data)
}

async function loadContent() {
  loading.value = true
  try {
    const data = await getContentDetail(contentId.value)
    content.value = data || {}
    currentProgress.value = data?.progress || data?.learningProgress || 0
    if (currentProgress.value >= 100) {
      reported.value = true
      progressHint.value = '该内容学习已完成'
    }
  } catch {
    // 错误已由拦截器处理
  } finally {
    loading.value = false
    startArticleAutoProgress()
  }
}

async function loadRelated() {
  try {
    const data = await getRecommendations({ page: 1, size: 5, contentId: contentId.value })
    relatedList.value = (data?.items || data || []).filter(i => i.id !== contentId.value).slice(0, 3)
  } catch {
    // 错误已由拦截器处理
  }
}

/** 文章类内容：停留 5 秒后学习进度自动变为 100% */
function startArticleAutoProgress() {
  if (isVideo.value || reported.value) return
  articleTimer = setTimeout(() => {
    currentProgress.value = 100
    progressHint.value = '学习时长达标，请点击「上报进度」完成上报'
    ElMessage.success('学习进度已达 100%，请点击「上报进度」提交')
  }, 5000)
}

function handleTimeUpdate() {
  if (!videoRef.value) return
  const currentTime = Math.floor(videoRef.value.currentTime)
  const duration = Math.floor(videoRef.value.duration)
  if (duration > 0) {
    currentProgress.value = Math.min(100, Math.round((currentTime / duration) * 100))
  }
  // 每30秒自动上报一次
  if (currentTime - lastReportedTime >= 30) {
    lastReportedTime = currentTime
    autoReportProgress(currentTime, false)
  }
}

function handleVideoEnded() {
  currentProgress.value = 100
  autoReportProgress(Math.floor(videoRef.value?.duration || 0), true)
}

async function autoReportProgress(durationSeconds, isCompleted) {
  try {
    await reportProgress({
      contentId: contentId.value,
      durationSeconds,
      isCompleted
    })
  } catch {
    // 静默处理自动上报错误（可能触发防挂机403）
  }
}

/** 打开防挂机验证弹窗 */
async function openAntiCheatDialog() {
  antiCheatLoading.value = true
  antiCheatSelected.value = null
  antiCheatCountdown.value = 60
  try {
    const res = await getAntiCheatChallenge(contentId.value)
    const data = res.data || res
    antiCheatQuestion.value = data.question || data.stem || ''
    antiCheatOptions.value = data.options || []
    antiCheatRecordId.value = data.recordId || data.id
    antiCheatVisible.value = true
    startAntiCheatCountdown()
  } catch (e) {
    ElMessage.error('获取验证题目失败')
  } finally {
    antiCheatLoading.value = false
  }
}

function startAntiCheatCountdown() {
  if (antiCheatTimer) clearInterval(antiCheatTimer)
  antiCheatTimer = setInterval(() => {
    antiCheatCountdown.value--
    if (antiCheatCountdown.value <= 0) {
      clearInterval(antiCheatTimer)
      ElMessage.warning('验证超时，请重新验证')
      openAntiCheatDialog()
    }
  }, 1000)
}

/** 提交防挂机验证答案 */
async function submitAntiCheatAnswer() {
  if (antiCheatSelected.value === null || antiCheatSelected.value === undefined) {
    ElMessage.warning('请选择答案')
    return
  }
  antiCheatLoading.value = true
  try {
    const res = await verifyAntiCheat({
      recordId: antiCheatRecordId.value,
      answer: String(antiCheatSelected.value)
    })
    const data = res.data || res
    if (data.isPass || data.passed || data.success) {
      ElMessage.success('验证通过')
      antiCheatVisible.value = false
      if (antiCheatTimer) clearInterval(antiCheatTimer)
      antiCheatSelected.value = null
      // 验证通过后继续上报进度
      if (pendingReportData) {
        await doReportProgress(pendingReportData)
        pendingReportData = null
      }
    } else {
      ElMessage.error('验证失败，请重新答题')
      await openAntiCheatDialog()
    }
  } catch (e) {
    ElMessage.error('验证提交失败')
  } finally {
    antiCheatLoading.value = false
  }
}

/** 实际执行进度上报 */
async function doReportProgress(data) {
  reporting.value = true
  try {
    await reportProgress(data)
    reported.value = true
    progressHint.value = data.isCompleted ? '学习进度已上报，任务完成' : '学习进度已上报'
    ElMessage.success(data.isCompleted ? '学习进度已上报，任务完成' : '学习进度已上报')
  } catch (e) {
    // 如果返回403，说明需要防挂机验证
    if (e?.response?.status === 403 || e?.status === 403) {
      pendingReportData = data
      await openAntiCheatDialog()
    }
  } finally {
    reporting.value = false
  }
}

async function reportLearningProgress() {
  const durationSeconds = videoRef.value ? Math.floor(videoRef.value.currentTime) : (currentProgress.value >= 100 ? 600 : 0)
  const isCompleted = currentProgress.value >= 100
  const data = {
    contentId: contentId.value,
    durationSeconds,
    isCompleted
  }
  // 学习时长超过120秒时，先尝试上报，若返回403则弹出防挂机验证
  await doReportProgress(data)
}

async function completeTask() {
  if (!content.value.taskId) return
  completing.value = true
  try {
    await apiCompleteTask({
      taskId: content.value.taskId,
      contentId: contentId.value
    })
    ElMessage.success('任务已完成')
  } catch {
    // 错误已由拦截器处理
  } finally {
    completing.value = false
  }
}

function goBack() {
  router.back()
}

function goAiChat() {
  router.push('/ai-chat')
}

function goRelated(id) {
  router.push(`/content/${id}`)
}

onMounted(() => {
  loadContent()
  loadRelated()
})

onBeforeUnmount(() => {
  if (progressTimer) clearInterval(progressTimer)
  if (articleTimer) clearTimeout(articleTimer)
  if (antiCheatTimer) clearInterval(antiCheatTimer)
})
</script>

<style scoped>
.content-detail {
  padding-bottom: 24px;
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
}

.back-link:hover {
  color: var(--red);
}

/* 视频播放器 */
.video-player {
  background: linear-gradient(135deg, #1a1a2e, #16213e);
  border-radius: var(--r10);
  aspect-ratio: 16 / 9;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  margin-bottom: 20px;
  overflow: hidden;
}

.video-element {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.video-poster {
  flex-direction: column;
  color: #fff;
  background: linear-gradient(135deg, #2c3e50, #1a252f);
}

.video-poster h2 {
  font-size: 28px;
  font-weight: 600;
  margin-bottom: 8px;
  text-align: center;
}

.video-poster p {
  font-size: 16px;
  color: rgba(255, 255, 255, 0.8);
}

.video-play-btn {
  width: 72px;
  height: 72px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.9);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 20px;
  cursor: pointer;
  transition: transform 0.2s;
  color: var(--red);
}

.video-play-btn:hover {
  transform: scale(1.1);
}

/* 文章内容 */
.article-content {
  background: var(--card);
  border-radius: var(--r10);
  padding: 32px;
  box-shadow: var(--sh);
  margin-bottom: 20px;
  font-size: 15px;
  line-height: 2;
  color: var(--t1);
}

.article-content :deep(img) {
  max-width: 100%;
  border-radius: 8px;
}

.article-content :deep(h1),
.article-content :deep(h2),
.article-content :deep(h3) {
  margin: 20px 0 12px;
}

.article-content :deep(p) {
  margin-bottom: 12px;
}

/* 课程信息 */
.course-info {
  background: var(--card);
  border-radius: var(--r10);
  padding: 24px;
  box-shadow: var(--sh);
  margin-bottom: 16px;
}

.course-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}

.meta-date {
  font-size: 12px;
  color: var(--t3);
  margin-left: auto;
}

.course-desc-title {
  font-size: 15px;
  margin-bottom: 12px;
}

.course-desc {
  font-size: 14px;
  color: var(--t2);
  line-height: 1.8;
  margin-bottom: 16px;
}

.progress-section {
  display: flex;
  align-items: center;
  gap: 12px;
  padding-top: 16px;
  border-top: 1px solid var(--bd);
}

.progress-label {
  font-size: 14px;
  color: var(--t2);
  flex-shrink: 0;
}

.progress-bar-wrap {
  flex: 1;
  max-width: 300px;
}

.progress-value {
  font-size: 14px;
  font-weight: 600;
  flex-shrink: 0;
}

.progress-hint {
  font-size: 12px;
  color: var(--t3);
  margin-top: 10px;
  line-height: 1.6;
}

/* 防挂机验证 */
.anti-cheat-question {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 16px;
  line-height: 1.6;
}

.anti-cheat-options {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.anti-cheat-option {
  margin: 0;
  padding: 10px 12px;
  border: 1px solid #e4e7ed;
  border-radius: 8px;
  transition: all 0.2s;
}

.anti-cheat-option:hover {
  border-color: #C8161D;
  background: #fef0f0;
}

/* AI 解读 */
.ai-insight {
  background: var(--card);
  border-radius: var(--r10);
  padding: 20px;
  box-shadow: var(--sh);
  margin-bottom: 16px;
  border-left: 4px solid var(--red);
  display: flex;
  align-items: center;
  gap: 16px;
  cursor: pointer;
  transition: box-shadow 0.2s;
}

.ai-insight:hover {
  box-shadow: var(--sh-hover);
}

.ai-insight-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: var(--red-10);
  color: var(--red);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.ai-insight-content {
  flex: 1;
}

.ai-insight-title {
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 4px;
}

.ai-insight-desc {
  font-size: 12px;
  color: var(--t3);
}

.ai-insight-arrow {
  color: var(--red);
  font-size: 20px;
}

/* 相关推荐 */
.related-list {
  background: var(--card);
  border-radius: var(--r10);
  padding: 20px;
  box-shadow: var(--sh);
}

.related-list h4 {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 12px;
}

.related-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 0;
  border-bottom: 1px solid var(--bd);
  cursor: pointer;
  transition: background 0.15s;
}

.related-item:last-child {
  border-bottom: none;
}

.related-item:hover {
  background: var(--bg);
  border-radius: 6px;
  padding-left: 8px;
  padding-right: 8px;
}

.related-dot {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: var(--bg);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  color: var(--t2);
  flex-shrink: 0;
}

.related-item .title {
  flex: 1;
  font-size: 13px;
}
</style>
