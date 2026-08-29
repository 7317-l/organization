<template>
  <div class="exam-center">
    <div class="page-header">
      <div class="page-title">考试中心</div>
    </div>

    <el-tabs v-model="activeTab" class="exam-tabs" @tab-change="handleTabChange">
      <!-- Tab1 待考测验 -->
      <el-tab-pane label="待考测验" name="pending">
        <div class="material-list" v-loading="pendingLoading">
          <template v-if="pendingExams.length > 0">
            <div v-for="item in pendingExams" :key="item.id || item.testId" class="material-card">
              <div class="material-icon v">
                <el-icon :size="28"><Document /></el-icon>
              </div>
              <div class="material-info">
                <div class="material-title">{{ item.paperName || item.title || item.name }}</div>
                <div class="material-tags">
                  <el-tag v-if="item.timeLimitMinutes" size="small" type="danger" effect="light">
                    限时{{ item.timeLimitMinutes }}分钟
                  </el-tag>
                  <el-tag v-if="item.totalScore" size="small" type="primary" effect="light">
                    满分{{ item.totalScore }}分
                  </el-tag>
                </div>
                <div class="material-date">
                  截止时间：{{ formatDateTime(item.deadline || item.endTime) }}
                </div>
              </div>
              <div class="material-status">
                <div class="status-pending">待考</div>
              </div>
              <div class="material-action">
                <el-button type="primary" size="small" @click="startQuiz(item.id || item.testId)">
                  开始答题
                </el-button>
              </div>
            </div>
          </template>
          <el-empty v-else description="暂无待考测验" :image-size="100" />
        </div>
      </el-tab-pane>

      <!-- Tab2 历史试卷 -->
      <el-tab-pane label="历史试卷" name="history">
        <div class="material-list" v-loading="historyLoading">
          <template v-if="historyExams.length > 0">
            <div v-for="item in historyExams" :key="item.id || item.testId" class="material-card" style="cursor:pointer" @click="viewResult(item.id || item.testId)">
              <div class="material-icon d">
                <el-icon :size="28"><DataAnalysis /></el-icon>
              </div>
              <div class="material-info">
                <div class="material-title">{{ item.paperName || item.title || item.name }}</div>
                <div class="material-tags">
                  <el-tag v-if="item.myScore !== undefined" size="small" type="success" effect="light">
                    得分 {{ item.myScore }}分
                  </el-tag>
                  <el-tag v-if="item.totalScore" size="small" type="primary" effect="light">
                    满分 {{ item.totalScore }}分
                  </el-tag>
                </div>
                <div class="material-date">
                  交卷时间：{{ formatDateTime(item.submittedAt || item.finishTime || item.completedAt) }}
                </div>
              </div>
              <div class="material-action">
                <el-button type="primary" plain size="small" @click="viewResult(item.id || item.testId)">
                  查看详情
                </el-button>
              </div>
            </div>
          </template>
          <el-empty v-else description="暂无历史试卷" :image-size="100" />
        </div>
      </el-tab-pane>

      <!-- Tab3 错题本 -->
      <el-tab-pane label="错题本" name="wrong">
        <!-- 错题统计摘要 -->
        <div class="exam-summary" v-if="wrongQuestions.length > 0">
          <p>
            共 <span class="highlight">{{ wrongQuestions.length }}</span> 道错题
            <span v-if="wrongStats.total > 0"> · 已加载 {{ wrongQuestions.length }}/{{ wrongStats.total }} 道</span>
          </p>
        </div>

        <!-- 筛选器 -->
        <div class="wrong-filters">
          <div class="filter-group">
            <span class="filter-label">题型:</span>
            <el-select v-model="wrongFilter.type" placeholder="全部题型" style="width: 140px" clearable @change="loadWrongQuestions">
              <el-option label="单选题" value="single" />
              <el-option label="多选题" value="multiple" />
              <el-option label="判断题" value="judge" />
            </el-select>
          </div>
          <div class="filter-group">
            <span class="filter-label">知识点:</span>
            <el-select v-model="wrongFilter.category" placeholder="全部知识点" style="width: 140px" clearable @change="loadWrongQuestions">
              <el-option v-for="cat in wrongCategories" :key="cat" :label="cat" :value="cat" />
            </el-select>
          </div>
          <div class="filter-group" style="margin-left: auto">
            <span class="filter-label">排序:</span>
            <el-select v-model="wrongFilter.sort" placeholder="最新" style="width: 120px" @change="loadWrongQuestions">
              <el-option label="最新" value="latest" />
              <el-option label="错误次数" value="count" />
            </el-select>
          </div>
          <el-button size="small" @click="resetWrongFilter">重置</el-button>
        </div>

        <!-- 错题列表 -->
        <div class="wrong-list" v-loading="wrongLoading">
          <template v-if="wrongQuestions.length > 0">
            <div v-for="(w, index) in wrongQuestions" :key="w.id || index" class="wrong-card">
              <div class="wrong-header">
                <div class="wrong-tags">
                  <el-tag size="small" type="danger" effect="light">{{ getQuestionTypeLabel(w.type) }}</el-tag>
                  <el-tag v-if="w.category || w.knowledgePoint" size="small" type="primary" effect="light">
                    {{ w.category || w.knowledgePoint }}
                  </el-tag>
                </div>
                <div class="wrong-meta">
                  <span v-if="w.wrongCount">错误次数 <span class="wrong-count">{{ w.wrongCount }}次</span></span>
                  <span>{{ formatDate(w.wrongTime || w.createdAt || w.date) }}</span>
                </div>
              </div>
              <div class="wrong-body">
                <div class="wrong-question">{{ w.question || w.title }}</div>
                <div class="wrong-answers">
                  <div class="wrong-answer-item wrong">你的答案：{{ formatAnswer(w.yourAnswer || w.userAnswer) }}</div>
                  <div class="wrong-answer-item correct">正确答案：{{ formatAnswer(w.correctAnswer || w.answer) }}</div>
                </div>
                <div v-if="w.analysis || w.explanation" class="wrong-analysis">
                  <strong>解析：</strong>{{ w.analysis || w.explanation }}
                </div>
              </div>
              <div class="wrong-actions">
                <el-button size="small" @click="viewQuestionDetail(w)">查看解析</el-button>
                <el-button type="primary" size="small" @click="addToPractice(w)">加入巩固</el-button>
              </div>
            </div>
          </template>
          <el-empty v-else description="暂无错题记录" :image-size="100" />
        </div>

        <!-- 分页 -->
        <div class="pagination-wrap" v-if="wrongStats.total > 0">
          <el-pagination
            v-model:current-page="wrongPage"
            v-model:page-size="wrongSize"
            :total="wrongStats.total"
            :page-sizes="[10, 20, 50]"
            layout="total, sizes, prev, pager, next, jumper"
            background
            @size-change="loadWrongQuestions"
            @current-change="loadWrongQuestions"
          />
        </div>
      </el-tab-pane>

      <!-- Tab4 AI专项巩固练习 -->
      <el-tab-pane label="AI专项巩固练习" name="practice">
        <div class="ai-analysis-card" v-loading="clusterLoading">
          <div class="ai-analysis-header">
            <div class="ai-analysis-icon">
              <el-icon :size="20"><MagicStick /></el-icon>
            </div>
            <div>
              <div class="ai-analysis-title">AI 助手提示</div>
              <div class="ai-analysis-text">基于你的错题记录，AI 建议优先巩固以下内容：</div>
            </div>
            <el-button type="primary" size="small" style="margin-left: auto" :loading="clusterLoading" @click="loadCluster">
              重新分析
            </el-button>
          </div>
          <div class="ai-top-list" v-if="clusterData.length > 0">
            <div v-for="(item, index) in clusterData" :key="index" class="ai-top-item">
              <div class="ai-top-rank">{{ index + 1 }}</div>
              <div style="flex: 1">
                <div class="ai-top-name">{{ item.clusterName || item.name || item.knowledgePoint }}</div>
                <div class="ai-top-bar">
                  <div class="ai-top-bar-fill" :style="{ width: ((item.severity || 0.5) * 100) + '%' }"></div>
                </div>
              </div>
              <div class="ai-top-stats">
                <div class="ai-top-stat">
                  <div class="ai-top-stat-value">{{ item.errorCount || item.wrongCount || item.count || 0 }}</div>
                  <div class="ai-top-stat-label">错误次数</div>
                </div>
                <div class="ai-top-stat">
                  <div class="ai-top-stat-value">{{ Math.round((item.severity || 0) * 100) }}%</div>
                  <div class="ai-top-stat-label">严重度</div>
                </div>
              </div>
            </div>
          </div>
          <el-empty v-else description="暂无薄弱知识点分析数据" :image-size="80" />
        </div>

        <!-- 练习配置 -->
        <div class="config-card">
          <div class="config-title">练习配置</div>
          <div class="config-row">
            <div class="config-item">
              <div class="config-label">题目数量</div>
              <el-select v-model="practiceConfig.count" style="width: 100%">
                <el-option :label="10 + '题'" :value="10" />
                <el-option :label="20 + '题'" :value="20" />
                <el-option :label="30 + '题'" :value="30" />
              </el-select>
            </div>
            <div class="config-item">
              <div class="config-label">题型</div>
              <el-select v-model="practiceConfig.type" style="width: 100%">
                <el-option label="全部题型" value="all" />
                <el-option label="单选题" value="single" />
                <el-option label="多选题" value="multiple" />
                <el-option label="判断题" value="judge" />
              </el-select>
            </div>
            <div class="config-item">
              <div class="config-label">知识点</div>
              <el-select v-model="practiceConfig.knowledge" style="width: 100%" clearable>
                <el-option v-for="item in clusterData" :key="item.name || item.knowledgePoint" :label="item.name || item.knowledgePoint" :value="item.name || item.knowledgePoint" />
              </el-select>
            </div>
            <div class="config-item">
              <div class="config-label">难度</div>
              <el-select v-model="practiceConfig.difficulty" style="width: 100%">
                <el-option label="智能匹配" value="smart" />
                <el-option label="困难" value="hard" />
                <el-option label="中等" value="medium" />
                <el-option label="简单" value="easy" />
              </el-select>
            </div>
          </div>
        </div>

        <!-- 开始练习 -->
        <div class="practice-footer">
          <div class="practice-info">
            已待巩固：<span class="highlight">{{ practiceConfig.count }}道</span>
            预计用时：<span class="highlight">约{{ Math.round(practiceConfig.count * 0.5) }}分钟</span>
          </div>
          <div class="practice-actions">
            <el-button type="primary" size="large" @click="startPractice">
              开始巩固练习
            </el-button>
          </div>
        </div>
      </el-tab-pane>
    </el-tabs>

    <!-- 试卷结果弹窗 -->
    <el-dialog v-model="resultDialogVisible" title="试卷详情" width="700px">
      <div v-loading="resultLoading" class="result-dialog-content">
        <div v-if="examResult" class="result-summary">
          <div class="result-score">
            <span class="score-value">{{ examResult.score }}</span>
            <span class="score-unit">分</span>
          </div>
          <div class="result-meta">
            <p>正确率：{{ examResult.correctRate || Math.round((examResult.correctCount / examResult.totalCount) * 100) || 0 }}%</p>
            <p>用时：{{ examResult.durationUsed || examResult.timeUsed || '-' }}分钟</p>
          </div>
        </div>
        <el-divider />
        <div class="result-questions" v-if="examResult">
          <div v-for="(q, idx) in (examResult?.questionAnswers || examResult?.questions || [])" :key="idx" class="result-question">
            <div class="rq-title">{{ idx + 1 }}. {{ q.stem || q.question || q.title }}</div>
            <div class="rq-answer">
              <span :class="q.isCorrect ? 'correct' : 'wrong'">
                {{ q.isCorrect ? '✓ 正确' : '✗ 错误' }}
              </span>
              <span v-if="!q.isCorrect">
                你的答案：{{ formatAnswer(q.userAnswer) }} / 正确答案：{{ formatAnswer(q.correctAnswer) }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Document, DataAnalysis, MagicStick } from '@element-plus/icons-vue'
import { getExams, getExamResult } from '@/api/exam'
import { kmeansCluster } from '@/api/ai'
import { useUserStore } from '@/stores/user'
import { formatDate, formatDateTime, questionTypeMap } from '@/utils/format'

const router = useRouter()
const userStore = useUserStore()

const activeTab = ref('pending')

// 待考测验
const pendingExams = ref([])
const pendingLoading = ref(false)

// 历史试卷
const historyExams = ref([])
const historyLoading = ref(false)

// 错题本
const wrongQuestions = ref([])
const wrongLoading = ref(false)
const wrongPage = ref(1)
const wrongSize = ref(10)
const wrongStats = reactive({ total: 0 })
const wrongFilter = reactive({ type: '', category: '', sort: 'latest' })
const wrongCategories = ref([])

// AI 聚类
const clusterData = ref([])
const clusterLoading = ref(false)

// 练习配置
const practiceConfig = reactive({ count: 20, type: 'all', knowledge: '', difficulty: 'smart' })

// 结果弹窗
const resultDialogVisible = ref(false)
const examResult = ref(null)
const resultLoading = ref(false)

function getQuestionTypeLabel(type) {
  return questionTypeMap[type] || type || '未知'
}

function formatAnswer(answer) {
  if (!answer) return '-'
  if (Array.isArray(answer)) return answer.join('、')
  return String(answer)
}

async function loadPendingExams() {
  pendingLoading.value = true
  try {
    const data = await getExams({ page: 1, size: 50, status: 'pending' })
    const items = data?.items || data || []
    pendingExams.value = items.filter(e => e.status === 'pending' || !e.status || e.status === 'not_started')
  } catch {
    // 错误已由拦截器处理
  } finally {
    pendingLoading.value = false
  }
}

async function loadHistoryExams() {
  historyLoading.value = true
  try {
    const data = await getExams({ page: 1, size: 50, status: 'completed' })
    const items = data?.items || data || []
    historyExams.value = items.filter(e => e.status === 'completed' || e.status === 'finished')
    // 如果接口不支持status过滤，全部返回后再过滤
    if (historyExams.value.length === 0 && items.length > 0) {
      historyExams.value = items.filter(e => e.score !== undefined || e.status === 'completed')
    }
  } catch {
    // 错误已由拦截器处理
  } finally {
    historyLoading.value = false
  }
}

/**
 * 错题本数据来源说明：
 * 由于后端没有独立的错题接口，错题数据通过以下方式获取：
 * 1. 调用 GET /mobile/exams 获取已完成的测验列表
 * 2. 对每个已完成测验调用 GET /mobile/exams/{testId}/result 获取详情
 * 3. 从结果中提取答错的题目，聚合为错题本
 * 如果后端后续提供独立错题接口，可直接替换此方法。
 */
async function loadWrongQuestions() {
  wrongLoading.value = true
  try {
    // 获取已完成测验列表
    const examsData = await getExams({ page: 1, size: 20, status: 'completed' })
    const completedExams = (examsData?.items || examsData || []).filter(
      e => e.status === 'completed' || e.status === 'finished' || e.score !== undefined
    )

    // 并行获取每个测验的结果，提取错题
    const resultPromises = completedExams.map(e =>
      getExamResult(e.id || e.testId).catch(() => null)
    )
    const results = await Promise.all(resultPromises)

    let allWrong = []
    const categories = new Set()

    results.forEach((result, idx) => {
      if (!result) return
      const questions = result.questionAnswers || result.questions || result.wrongQuestions || []
      const examInfo = completedExams[idx]

      questions.forEach(q => {
        if (q.isCorrect === false || q.isWrong || q.status === 'wrong') {
          const wrongItem = {
            id: `${examInfo?.id || examInfo?.testId}-${q.questionId || q.id}`,
            type: q.type || q.questionType,
            category: q.category || q.knowledgePoint,
            question: q.stem || q.question || q.title,
            yourAnswer: q.userAnswer || q.yourAnswer,
            correctAnswer: q.correctAnswer || q.answer,
            analysis: q.analysis || q.explanation,
            wrongCount: q.wrongCount || 1,
            wrongTime: q.wrongTime || examInfo?.submittedAt || examInfo?.completedAt,
            testId: examInfo?.id || examInfo?.testId,
            questionId: q.questionId || q.id
          }
          allWrong.push(wrongItem)
          if (wrongItem.category) categories.add(wrongItem.category)
        }
      })
    })

    // 应用筛选
    if (wrongFilter.type) {
      allWrong = allWrong.filter(w => w.type === wrongFilter.type)
    }
    if (wrongFilter.category) {
      allWrong = allWrong.filter(w => w.category === wrongFilter.category)
    }

    // 排序
    if (wrongFilter.sort === 'count') {
      allWrong.sort((a, b) => (b.wrongCount || 0) - (a.wrongCount || 0))
    } else {
      allWrong.sort((a, b) => new Date(b.wrongTime || 0) - new Date(a.wrongTime || 0))
    }

    wrongCategories.value = Array.from(categories)
    wrongStats.total = allWrong.length

    // 分页
    const start = (wrongPage.value - 1) * wrongSize.value
    wrongQuestions.value = allWrong.slice(start, start + wrongSize.value)
  } catch {
    // 错误已由拦截器处理
  } finally {
    wrongLoading.value = false
  }
}

async function loadCluster() {
  clusterLoading.value = true
  try {
    const memberId = userStore.userInfo?.id || userStore.userInfo?.memberId || userStore.userInfo?.userId
    if (!memberId) {
      ElMessage.warning('无法获取用户信息')
      return
    }
    const data = await kmeansCluster(memberId)
    clusterData.value = data?.clusters || data?.items || data || []
  } catch {
    // 错误已由拦截器处理
  } finally {
    clusterLoading.value = false
  }
}

function resetWrongFilter() {
  wrongFilter.type = ''
  wrongFilter.category = ''
  wrongFilter.sort = 'latest'
  wrongPage.value = 1
  loadWrongQuestions()
}

function handleTabChange(name) {
  if (name === 'pending') loadPendingExams()
  else if (name === 'history') loadHistoryExams()
  else if (name === 'wrong') loadWrongQuestions()
  else if (name === 'practice') loadCluster()
}

function startQuiz(testId) {
  router.push(`/quiz/${testId}`)
}

async function viewResult(testId) {
  resultDialogVisible.value = true
  resultLoading.value = true
  try {
    const data = await getExamResult(testId)
    // 计算正确数和总数
    const questions = data?.questionAnswers || data?.questions || []
    const correctCount = questions.filter(q => q.isCorrect === true).length
    examResult.value = {
      ...data,
      correctCount,
      totalCount: questions.length,
      correctRate: questions.length > 0 ? Math.round((correctCount / questions.length) * 100) : 0
    }
  } catch {
    // 错误已由拦截器处理
  } finally {
    resultLoading.value = false
  }
}

function viewQuestionDetail(w) {
  if (w.testId) {
    viewResult(w.testId)
  } else {
    ElMessage.info('暂无详细解析')
  }
}

function addToPractice(w) {
  activeTab.value = 'practice'
  if (w.category) {
    practiceConfig.knowledge = w.category
  }
  ElMessage.success('已加入巩固练习')
}

function startPractice() {
  // 巩固练习使用测验流程，传入配置参数
  ElMessage.info('正在生成巩固练习试卷...')
  // 这里可以调用后端生成练习的接口，如果没有则跳转到一个已有测验
  router.push('/exam')
}

onMounted(() => {
  loadPendingExams()
})
</script>

<style scoped>
.exam-center {
  padding-bottom: 24px;
}

.exam-tabs :deep(.el-tabs__item) {
  font-size: 15px;
  height: 48px;
  line-height: 48px;
}

.material-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.material-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 16px 20px;
  box-shadow: var(--sh);
  display: flex;
  align-items: center;
  gap: 16px;
  transition: box-shadow 0.2s;
}

.material-card:hover {
  box-shadow: var(--sh-hover);
}

.material-icon {
  width: 64px;
  height: 64px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  color: #fff;
}

.material-icon.v {
  background: linear-gradient(135deg, var(--red), var(--red-d));
}

.material-icon.d {
  background: linear-gradient(135deg, #2c3e50, #1a252f);
}

.material-info {
  flex: 1;
  min-width: 0;
}

.material-title {
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 8px;
}

.material-tags {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
  flex-wrap: wrap;
}

.material-date {
  font-size: 12px;
  color: var(--t3);
}

.material-status {
  flex-shrink: 0;
}

.status-pending {
  color: var(--red);
  font-weight: 600;
}

.material-action {
  flex-shrink: 0;
}

/* 错题摘要 */
.exam-summary {
  background: var(--card);
  border-radius: var(--r10);
  padding: 16px 20px;
  box-shadow: var(--sh);
  margin-bottom: 16px;
}

.exam-summary p {
  font-size: 14px;
  color: var(--t2);
}

.exam-summary .highlight {
  color: var(--red);
  font-weight: 600;
}

/* 筛选器 */
.wrong-filters {
  background: var(--card);
  border-radius: var(--r10);
  padding: 16px 20px;
  box-shadow: var(--sh);
  margin-bottom: 16px;
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.filter-label {
  font-size: 13px;
  color: var(--t3);
}

/* 错题列表 */
.wrong-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.wrong-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 20px;
  box-shadow: var(--sh);
}

.wrong-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.wrong-tags {
  display: flex;
  align-items: center;
  gap: 6px;
}

.wrong-meta {
  margin-left: auto;
  display: flex;
  align-items: center;
  gap: 16px;
  font-size: 12px;
  color: var(--t3);
}

.wrong-count {
  color: var(--red);
  font-weight: 600;
}

.wrong-body {
  margin-bottom: 12px;
}

.wrong-question {
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 12px;
  line-height: 1.6;
}

.wrong-answers {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  margin-bottom: 12px;
}

.wrong-answer-item {
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 13px;
}

.wrong-answer-item.wrong {
  background: rgba(200, 22, 29, 0.08);
  color: var(--red);
}

.wrong-answer-item.correct {
  background: rgba(46, 139, 87, 0.08);
  color: var(--green);
}

.wrong-analysis {
  font-size: 13px;
  color: var(--t2);
  line-height: 1.6;
  padding: 10px 14px;
  background: var(--bg);
  border-radius: 8px;
}

.wrong-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
}

.pagination-wrap {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

/* AI分析卡片 */
.ai-analysis-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 24px;
  box-shadow: var(--sh);
  margin-bottom: 20px;
}

.ai-analysis-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.ai-analysis-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  background: var(--red-10);
  color: var(--red);
  display: flex;
  align-items: center;
  justify-content: center;
}

.ai-analysis-title {
  font-size: 14px;
  font-weight: 600;
}

.ai-analysis-text {
  font-size: 13px;
  color: var(--t2);
  margin-top: 2px;
}

.ai-top-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ai-top-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  background: var(--bg);
  border-radius: var(--r10);
}

.ai-top-rank {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: var(--red);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  font-weight: 600;
  flex-shrink: 0;
}

.ai-top-name {
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 6px;
}

.ai-top-bar {
  height: 6px;
  background: var(--bd);
  border-radius: 3px;
  overflow: hidden;
  width: 100%;
  max-width: 300px;
}

.ai-top-bar-fill {
  height: 100%;
  background: var(--red);
  border-radius: 3px;
}

.ai-top-stats {
  display: flex;
  gap: 24px;
  flex-shrink: 0;
}

.ai-top-stat {
  text-align: center;
}

.ai-top-stat-value {
  font-size: 16px;
  font-weight: 600;
  color: var(--red);
}

.ai-top-stat-label {
  font-size: 11px;
  color: var(--t3);
}

/* 配置卡片 */
.config-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 24px;
  box-shadow: var(--sh);
  margin-bottom: 20px;
}

.config-title {
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 16px;
}

.config-row {
  display: flex;
  gap: 16px;
}

.config-item {
  flex: 1;
}

.config-label {
  font-size: 12px;
  color: var(--t3);
  margin-bottom: 6px;
}

/* 练习底部 */
.practice-footer {
  background: var(--card);
  border-radius: var(--r10);
  padding: 24px;
  box-shadow: var(--sh);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.practice-info {
  font-size: 14px;
  color: var(--t2);
}

.practice-info .highlight {
  color: var(--red);
  font-weight: 600;
  margin: 0 4px;
}

/* 结果弹窗 */
.result-dialog-content {
  min-height: 200px;
}

.result-summary {
  display: flex;
  align-items: center;
  gap: 32px;
}

.result-score {
  text-align: center;
}

.score-value {
  font-size: 48px;
  font-weight: 700;
  color: var(--red);
}

.score-unit {
  font-size: 16px;
  color: var(--t3);
}

.result-meta p {
  font-size: 14px;
  color: var(--t2);
  line-height: 2;
}

.result-questions {
  max-height: 400px;
  overflow-y: auto;
}

.result-question {
  padding: 12px 0;
  border-bottom: 1px solid var(--bd);
}

.result-question:last-child {
  border-bottom: none;
}

.rq-title {
  font-size: 14px;
  margin-bottom: 8px;
}

.rq-answer {
  font-size: 13px;
  color: var(--t2);
}

.rq-answer .correct {
  color: var(--green);
  font-weight: 600;
  margin-right: 12px;
}

.rq-answer .wrong {
  color: var(--red);
  font-weight: 600;
  margin-right: 12px;
}
</style>
