<template>
  <div class="quiz-page">
    <div class="back-link" @click="goBack">← 返回</div>

    <!-- 未开始/加载中 -->
    <div v-if="loading" class="quiz-loading">
      <el-icon class="is-loading" :size="40"><Loading /></el-icon>
      <p>正在加载试卷...</p>
    </div>

    <!-- 答题中 -->
    <template v-else-if="!submitted && quizData">
      <div class="quiz-header">
        <div class="page-title">{{ quizData.title || quizData.name || '测验' }}</div>
        <div class="quiz-timer" v-if="remainingTime > 0">
          <el-icon><Clock /></el-icon>
          {{ formatTime(remainingTime) }}
        </div>
      </div>

      <div class="quiz-progress">
        <div class="quiz-progress-label">
          <span>进度：</span>
          <span>{{ currentIndex + 1 }}/{{ questions.length }}题</span>
        </div>
        <el-progress
          :percentage="Math.round(((currentIndex + 1) / questions.length) * 100)"
          :show-text="false"
          :stroke-width="8"
          color="#C8161D"
        />
        <div class="quiz-progress-label" style="justify-content: flex-end">
          <span>{{ Math.round(((currentIndex + 1) / questions.length) * 100) }}%</span>
        </div>
      </div>

      <div class="quiz-card">
        <div class="quiz-type-tag">
          【{{ getQuestionTypeLabel(currentQuestion.type) }}】
          <span v-if="currentQuestion.category || currentQuestion.knowledgePoint" class="quiz-cat">
            · {{ currentQuestion.category || currentQuestion.knowledgePoint }}
          </span>
        </div>
        <div class="quiz-question">{{ currentQuestion.question || currentQuestion.title }}</div>

        <div class="quiz-options">
          <!-- 单选/多选 -->
          <template v-if="currentQuestion.type !== 'judge'">
            <div
              v-for="(opt, idx) in (currentQuestion.options || [])"
              :key="idx"
              class="quiz-option"
              :class="{ selected: isOptionSelected(idx) }"
              @click="selectOption(idx)"
            >
              <span class="option-label">{{ getOptionLabel(idx) }}</span>
              <span class="option-text">{{ opt.text || opt.content || opt }}</span>
              <span class="option-mark">✓</span>
            </div>
          </template>
          <!-- 判断题 -->
          <template v-else>
            <div
              class="quiz-option"
              :class="{ selected: currentAnswer === true || currentAnswer === 'true' || currentAnswer === '正确' }"
              @click="selectJudge(true)"
            >
              <span class="option-label">✓</span>
              <span class="option-text">正确</span>
              <span class="option-mark">✓</span>
            </div>
            <div
              class="quiz-option"
              :class="{ selected: currentAnswer === false || currentAnswer === 'false' || currentAnswer === '错误' }"
              @click="selectJudge(false)"
            >
              <span class="option-label">✗</span>
              <span class="option-text">错误</span>
              <span class="option-mark">✓</span>
            </div>
          </template>
        </div>
      </div>

      <div class="quiz-actions">
        <el-button size="large" :disabled="currentIndex === 0" @click="prevQuestion">
          上一题
        </el-button>
        <el-button
          v-if="currentIndex < questions.length - 1"
          type="primary"
          size="large"
          @click="nextQuestion"
        >
          下一题 →
        </el-button>
        <el-button
          v-else
          type="success"
          size="large"
          :loading="submitting"
          @click="handleSubmit"
        >
          提交试卷
        </el-button>
      </div>

      <!-- 答题卡 -->
      <div class="answer-card">
        <div class="answer-card-title">答题卡</div>
        <div class="answer-card-grid">
          <div
            v-for="(q, idx) in questions"
            :key="idx"
            class="answer-card-item"
            :class="{
              current: idx === currentIndex,
              answered: isAnswered(idx),
              unanswered: !isAnswered(idx)
            }"
            @click="goToQuestion(idx)"
          >
            {{ idx + 1 }}
          </div>
        </div>
        <div class="answer-card-legend">
          <span><i class="legend-dot answered"></i>已答</span>
          <span><i class="legend-dot unanswered"></i>未答</span>
          <span><i class="legend-dot current"></i>当前</span>
        </div>
      </div>
    </template>

    <!-- 提交后结果 -->
    <template v-else-if="submitted && examResult">
      <div class="result-page">
        <div class="result-header">
          <div class="result-icon">
            <el-icon :size="48"><Trophy /></el-icon>
          </div>
          <h2>测验完成</h2>
          <p>{{ quizData.title || quizData.name || '测验' }}</p>
        </div>

        <div class="result-score-card">
          <div class="result-score">
            <span class="score-num">{{ examResult.score }}</span>
            <span class="score-unit">分</span>
          </div>
          <div class="result-level" :style="{ color: getLevelColor(examResult.score) }">
            {{ getLevelText(examResult.score) }}
          </div>
        </div>

        <div class="result-stats">
          <div class="result-stat-item">
            <div class="stat-num">{{ examResult.correctCount || 0 }}</div>
            <div class="stat-label">答对</div>
          </div>
          <div class="result-stat-item">
            <div class="stat-num" style="color: var(--red)">{{ examResult.wrongCount || 0 }}</div>
            <div class="stat-label">答错</div>
          </div>
          <div class="result-stat-item">
            <div class="stat-num">{{ examResult.correctRate || Math.round(((examResult.correctCount || 0) / questions.length) * 100) }}%</div>
            <div class="stat-label">正确率</div>
          </div>
          <div class="result-stat-item">
            <div class="stat-num">{{ examResult.durationUsed || timeUsed || '-' }}</div>
            <div class="stat-label">用时(分钟)</div>
          </div>
        </div>

        <!-- 答题详情 -->
        <div class="result-detail" v-if="examResult.questions && examResult.questions.length > 0">
          <h3>答题详情</h3>
          <div
            v-for="(q, idx) in examResult.questions"
            :key="idx"
            class="result-detail-item"
          >
            <div class="rd-question">
              <span class="rd-num">{{ idx + 1 }}.</span>
              <span class="rd-text">{{ q.question || q.title }}</span>
              <el-tag :type="q.isCorrect ? 'success' : 'danger'" size="small" effect="light">
                {{ q.isCorrect ? '正确' : '错误' }}
              </el-tag>
            </div>
            <div v-if="!q.isCorrect" class="rd-answers">
              <div class="rd-answer wrong">你的答案：{{ formatAnswer(q.userAnswer) }}</div>
              <div class="rd-answer correct">正确答案：{{ formatAnswer(q.correctAnswer) }}</div>
            </div>
            <div v-if="q.analysis || q.explanation" class="rd-analysis">
              <strong>解析：</strong>{{ q.analysis || q.explanation }}
            </div>
          </div>
        </div>

        <div class="result-actions">
          <el-button size="large" @click="goBack">返回列表</el-button>
          <el-button type="primary" size="large" @click="goExamCenter">再考一次</el-button>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Loading, Clock, Trophy } from '@element-plus/icons-vue'
import { startExam, submitExam, getExamResult } from '@/api/exam'
import { formatTime, questionTypeMap } from '@/utils/format'
import { useAiDataStore } from '@/stores/aiData'

const route = useRoute()
const router = useRouter()
const aiDataStore = useAiDataStore()

const testId = computed(() => route.params.testId)

const loading = ref(true)
const submitting = ref(false)
const submitted = ref(false)
const quizData = ref(null)
const questions = ref([])
const answers = ref({})
const currentIndex = ref(0)
const remainingTime = ref(0)
const examResult = ref(null)
const timeUsed = ref(0)
let timer = null
let startTime = null

const currentQuestion = computed(() => questions.value[currentIndex.value] || {})
const currentAnswer = computed(() => answers.value[currentIndex.value])

// 将当前题目同步到 AI 面板
watch(
  [currentQuestion, currentAnswer, currentIndex],
  () => {
    const q = currentQuestion.value
    if (!q || !q.question && !q.title) {
      aiDataStore.setCurrentQuestion(null)
      return
    }
    const ans = currentAnswer.value
    let userAnswerLetter = ''
    if (q.type === 'judge') {
      userAnswerLetter = ans === true || ans === 'true' || ans === '正确' ? '正确' : ans === false || ans === 'false' || ans === '错误' ? '错误' : ''
    } else if (q.type === 'multiple') {
      userAnswerLetter = Array.isArray(ans) ? ans.map(i => String.fromCharCode(65 + i)).join('') : ''
    } else {
      userAnswerLetter = typeof ans === 'number' ? String.fromCharCode(65 + ans) : (ans || '')
    }

    aiDataStore.setCurrentQuestion({
      question: q.question || q.title,
      options: q.options || [],
      userAnswer: userAnswerLetter,
      correctAnswer: q.correctAnswer || q.answer || '',
      knowledgePoint: q.knowledgePoint || q.category || '',
      questionNumber: currentIndex.value + 1,
      submitted: false,
      correct: null
    })
  },
  { immediate: true, deep: true }
)

function getQuestionTypeLabel(type) {
  return questionTypeMap[type] || type || '未知'
}

function getOptionLabel(idx) {
  return String.fromCharCode(65 + idx)
}

function isOptionSelected(idx) {
  const answer = currentAnswer.value
  if (Array.isArray(answer)) {
    return answer.includes(idx)
  }
  return answer === idx
}

function isAnswered(idx) {
  const answer = answers.value[idx]
  if (answer === undefined || answer === null) return false
  if (Array.isArray(answer)) return answer.length > 0
  return answer !== ''
}

function selectOption(idx) {
  const q = currentQuestion.value
  if (q.type === 'multiple') {
    // 多选
    if (!answers.value[currentIndex.value]) {
      answers.value[currentIndex.value] = []
    }
    const arr = answers.value[currentIndex.value]
    const pos = arr.indexOf(idx)
    if (pos > -1) {
      arr.splice(pos, 1)
    } else {
      arr.push(idx)
      arr.sort((a, b) => a - b)
    }
  } else {
    // 单选
    answers.value[currentIndex.value] = idx
  }
}

function selectJudge(val) {
  answers.value[currentIndex.value] = val
}

function prevQuestion() {
  if (currentIndex.value > 0) {
    currentIndex.value--
  }
}

function nextQuestion() {
  if (currentIndex.value < questions.value.length - 1) {
    currentIndex.value++
  }
}

function goToQuestion(idx) {
  currentIndex.value = idx
}

function formatAnswer(answer) {
  if (answer === undefined || answer === null) return '-'
  if (typeof answer === 'boolean') return answer ? '正确' : '错误'
  if (Array.isArray(answer)) {
    return answer.map(a => typeof a === 'number' ? getOptionLabel(a) : a).join('、')
  }
  if (typeof answer === 'number') return getOptionLabel(answer)
  return String(answer)
}

function buildSubmitAnswers() {
  return questions.value.map((q, idx) => {
    const answer = answers.value[idx]
    let formattedAnswer = ''
    if (q.type === 'judge') {
      // 判断题：后端期望 "true" / "false"
      formattedAnswer = (answer === true || answer === 'true') ? 'true' : (answer === false || answer === 'false') ? 'false' : ''
    } else if (q.type === 'multiple') {
      // 多选题：后端期望 JSON 字符串 "[0,2]"
      formattedAnswer = Array.isArray(answer) && answer.length > 0 ? JSON.stringify(answer) : '[]'
    } else {
      // 单选题：后端期望索引字符串 "0" / "1"
      formattedAnswer = typeof answer === 'number' ? String(answer) : (answer !== null && answer !== undefined ? String(answer) : '')
    }
    return {
      questionId: q.id || q.questionId,
      answer: formattedAnswer
    }
  })
}

// 把后端返回的索引答案转成前端显示用的字母/文字
function formatAnswerForDisplay(answer, type) {
  if (answer === null || answer === undefined || answer === '') return '-'
  const str = String(answer).trim()
  if (type === 'judge') {
    if (str === 'true' || str === '正确') return '正确'
    if (str === 'false' || str === '错误') return '错误'
    return str
  }
  if (type === 'multiple') {
    try {
      const arr = JSON.parse(str)
      if (Array.isArray(arr)) {
        return arr.map(i => typeof i === 'number' ? String.fromCharCode(65 + i) : String(i)).join('、')
      }
    } catch { /* 不是JSON，按普通字符串处理 */ }
    // 可能已经是字母格式
    if (/^[A-Z,，、\s]+$/.test(str)) return str.replace(/[,，]/g, '、')
    return str
  }
  // 单选题：索引数字 → 字母
  if (/^\d+$/.test(str)) {
    return String.fromCharCode(65 + parseInt(str, 10))
  }
  return str
}

async function handleSubmit() {
  const unanswered = questions.value.filter((_, idx) => !isAnswered(idx)).length
  if (unanswered > 0) {
    try {
      await ElMessageBox.confirm(
        `还有 ${unanswered} 道题未作答，确定提交吗？`,
        '提示',
        { confirmButtonText: '确定提交', cancelButtonText: '继续答题', type: 'warning' }
      )
    } catch {
      return
    }
  }

  submitting.value = true
  try {
    // 停止计时
    if (timer) {
      clearInterval(timer)
      timer = null
    }
    if (startTime) {
      timeUsed.value = Math.round((Date.now() - startTime) / 60000)
    }

    const submitData = {
      testId: testId.value,
      answers: buildSubmitAnswers()
    }
    await submitExam(submitData)

    // 获取结果并转换后端格式为前端格式
    const rawResult = await getExamResult(testId.value)
    const rawQuestions = rawResult?.questionAnswers || rawResult?.questions || []
    const convertedQuestions = rawQuestions.map(q => {
      const qid = q.questionId || q.id
      const origQ = questions.value.find(item => (item.id || item.questionId) === qid) || {}
      return {
        id: qid,
        question: q.question || q.title || q.stem || origQ.question || '',
        options: origQ.options || [],
        type: origQ.type || 'single',
        userAnswer: formatAnswerForDisplay(q.userAnswer, origQ.type),
        correctAnswer: formatAnswerForDisplay(q.correctAnswer, origQ.type),
        isCorrect: q.isCorrect === true,
        score: q.score || 0,
        knowledgePoint: origQ.knowledgePoint || q.category || ''
      }
    })
    const correctCount = convertedQuestions.filter(q => q.isCorrect).length
    const wrongCount = convertedQuestions.length - correctCount
    examResult.value = {
      ...rawResult,
      questions: convertedQuestions,
      correctCount,
      wrongCount,
      correctRate: convertedQuestions.length > 0 ? Math.round((correctCount / convertedQuestions.length) * 100) : 0
    }
    submitted.value = true

    // 将答题记录同步到 AI 面板
    const history = convertedQuestions.map((q, idx) => ({
      id: q.id || idx,
      question: q.question,
      options: q.options,
      userAnswer: q.userAnswer,
      correctAnswer: q.correctAnswer,
      correct: q.isCorrect,
      knowledgePoint: q.knowledgePoint
    }))
    aiDataStore.setAnswerHistory(history)

    // 更新当前题目为已提交状态
    const curQ = currentQuestion.value
    if (curQ) {
      const curResult = convertedQuestions[currentIndex.value]
      aiDataStore.setCurrentQuestion({
        question: curQ.question || curQ.title,
        options: curQ.options || [],
        userAnswer: curResult?.userAnswer || '',
        correctAnswer: curResult?.correctAnswer || curQ.correctAnswer || '',
        knowledgePoint: curQ.knowledgePoint || curQ.category || '',
        questionNumber: currentIndex.value + 1,
        submitted: true,
        correct: curResult?.isCorrect === true
      })
    }

    ElMessage.success('试卷提交成功')
  } catch {
    // 错误已由拦截器处理
  } finally {
    submitting.value = false
  }
}

function getLevelColor(score) {
  if (score >= 90) return 'var(--green)'
  if (score >= 75) return 'var(--blue)'
  if (score >= 60) return 'var(--orange)'
  return 'var(--red)'
}

function getLevelText(score) {
  if (score >= 90) return '优秀'
  if (score >= 75) return '良好'
  if (score >= 60) return '及格'
  return '不及格'
}

function goBack() {
  router.back()
}

function goExamCenter() {
  router.push('/exam')
}

function startTimer(duration) {
  if (!duration || duration <= 0) return
  remainingTime.value = duration * 60
  timer = setInterval(() => {
    remainingTime.value--
    if (remainingTime.value <= 0) {
      clearInterval(timer)
      timer = null
      ElMessage.warning('考试时间到，自动提交')
      handleSubmit()
    }
  }, 1000)
}

async function loadQuiz() {
  loading.value = true
  try {
    const data = await startExam(testId.value)
    quizData.value = data
    const rawQuestions = data?.questions || data?.items || []
    // 后端字段转换：stem→question, questionType(int)→type(string), 保留 options/id
    const typeMap = { 0: 'single', 1: 'multiple', 2: 'judge' }
    questions.value = rawQuestions.map(q => ({
      id: q.id || q.questionId,
      question: q.question || q.title || q.stem || '',
      type: q.type || typeMap[q.questionType] || 'single',
      options: Array.isArray(q.options) ? q.options : [],
      correctAnswer: q.correctAnswer || q.answer || '',
      knowledgePoint: q.knowledgePoint || q.category || '',
      score: q.score || 5
    }))
    if (questions.value.length === 0) {
      ElMessage.warning('该试卷暂无题目')
    }
    // 初始化答案
    questions.value.forEach((_, idx) => {
      answers.value[idx] = questions.value[idx].type === 'multiple' ? [] : null
    })
    // 启动计时器
    if (data?.duration || data?.timeLimitMinutes) {
      startTimer((data?.duration || data?.timeLimitMinutes) * 60)
    }
    startTime = Date.now()
  } catch {
    // 错误已由拦截器处理
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadQuiz()
})

onBeforeUnmount(() => {
  if (timer) {
    clearInterval(timer)
    timer = null
  }
})
</script>

<style scoped>
.quiz-page {
  padding-bottom: 40px;
  max-width: 900px;
  margin: 0 auto;
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

.quiz-loading {
  text-align: center;
  padding: 80px 0;
  color: var(--t3);
}

.quiz-loading p {
  margin-top: 16px;
  font-size: 14px;
}

.quiz-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.quiz-timer {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: var(--red-10);
  color: var(--red);
  border-radius: 8px;
  font-weight: 600;
  font-size: 15px;
}

.quiz-progress {
  margin-bottom: 20px;
}

.quiz-progress-label {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  font-size: 13px;
  color: var(--t2);
}

.quiz-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 28px;
  box-shadow: var(--sh);
  margin-bottom: 20px;
}

.quiz-type-tag {
  display: inline-block;
  padding: 4px 12px;
  background: var(--red-10);
  color: var(--red);
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 16px;
}

.quiz-cat {
  font-weight: 400;
  color: var(--t2);
}

.quiz-question {
  font-size: 18px;
  font-weight: 500;
  line-height: 1.7;
  margin-bottom: 24px;
}

.quiz-options {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.quiz-option {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 20px;
  border: 2px solid var(--bd);
  border-radius: var(--r10);
  cursor: pointer;
  transition: all 0.15s;
  font-size: 15px;
}

.quiz-option:hover {
  border-color: var(--red-50);
}

.quiz-option.selected {
  border-color: var(--red);
  background: var(--red-10);
}

.option-label {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: var(--bg);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 13px;
  flex-shrink: 0;
}

.quiz-option.selected .option-label {
  background: var(--red);
  color: #fff;
}

.option-text {
  flex: 1;
}

.option-mark {
  margin-left: auto;
  font-size: 20px;
  color: var(--red);
  display: none;
}

.quiz-option.selected .option-mark {
  display: block;
}

.quiz-actions {
  display: flex;
  gap: 16px;
  margin-bottom: 24px;
}

.quiz-actions .el-button {
  flex: 1;
  height: 48px;
  font-size: 15px;
}

/* 答题卡 */
.answer-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 20px;
  box-shadow: var(--sh);
}

.answer-card-title {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 12px;
}

.answer-card-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 12px;
}

.answer-card-item {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.15s;
  border: 1px solid var(--bd);
}

.answer-card-item.answered {
  background: var(--red-10);
  color: var(--red);
  border-color: var(--red);
}

.answer-card-item.unanswered {
  background: #fff;
  color: var(--t3);
}

.answer-card-item.current {
  background: var(--red);
  color: #fff;
  border-color: var(--red);
  font-weight: 600;
}

.answer-card-legend {
  display: flex;
  gap: 20px;
  font-size: 12px;
  color: var(--t3);
}

.legend-dot {
  display: inline-block;
  width: 12px;
  height: 12px;
  border-radius: 3px;
  margin-right: 4px;
  vertical-align: middle;
}

.legend-dot.answered {
  background: var(--red-10);
  border: 1px solid var(--red);
}

.legend-dot.unanswered {
  background: #fff;
  border: 1px solid var(--bd);
}

.legend-dot.current {
  background: var(--red);
}

/* 结果页 */
.result-page {
  text-align: center;
}

.result-header {
  margin-bottom: 24px;
}

.result-icon {
  color: var(--gold);
  margin-bottom: 12px;
}

.result-header h2 {
  font-size: 24px;
  margin-bottom: 8px;
}

.result-header p {
  color: var(--t3);
  font-size: 14px;
}

.result-score-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 32px;
  box-shadow: var(--sh);
  margin-bottom: 20px;
}

.result-score {
  margin-bottom: 8px;
}

.score-num {
  font-size: 64px;
  font-weight: 700;
  color: var(--red);
  line-height: 1;
}

.score-unit {
  font-size: 20px;
  color: var(--t3);
  margin-left: 4px;
}

.result-level {
  font-size: 18px;
  font-weight: 600;
}

.result-stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}

.result-stat-item {
  background: var(--card);
  border-radius: var(--r10);
  padding: 20px;
  box-shadow: var(--sh);
}

.stat-num {
  font-size: 24px;
  font-weight: 700;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 12px;
  color: var(--t3);
}

.result-detail {
  background: var(--card);
  border-radius: var(--r10);
  padding: 24px;
  box-shadow: var(--sh);
  margin-bottom: 24px;
  text-align: left;
}

.result-detail h3 {
  font-size: 16px;
  margin-bottom: 16px;
}

.result-detail-item {
  padding: 16px 0;
  border-bottom: 1px solid var(--bd);
}

.result-detail-item:last-child {
  border-bottom: none;
}

.rd-question {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  margin-bottom: 12px;
}

.rd-num {
  font-weight: 600;
  flex-shrink: 0;
}

.rd-text {
  flex: 1;
  font-size: 14px;
}

.rd-answers {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  margin-bottom: 12px;
  margin-left: 24px;
}

.rd-answer {
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 13px;
}

.rd-answer.wrong {
  background: rgba(200, 22, 29, 0.08);
  color: var(--red);
}

.rd-answer.correct {
  background: rgba(46, 139, 87, 0.08);
  color: var(--green);
}

.rd-analysis {
  font-size: 13px;
  color: var(--t2);
  line-height: 1.6;
  padding: 10px 14px;
  background: var(--bg);
  border-radius: 8px;
  margin-left: 24px;
}

.result-actions {
  display: flex;
  gap: 16px;
  justify-content: center;
}

.result-actions .el-button {
  min-width: 160px;
  height: 48px;
  font-size: 15px;
}
</style>
