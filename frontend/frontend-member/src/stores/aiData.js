import { defineStore } from 'pinia'
import request from '@/api/request'

export const useAiDataStore = defineStore('aiData', {
  state: () => ({
    currentQuestion: null,
    answerHistory: [],
    aiPanelVisible: false,
    historyLoaded: false
  }),

  getters: {
    hasQuestion: state => !!(state.currentQuestion && state.currentQuestion.question),
    wrongAnswers: state => state.answerHistory.filter(r => r && r.correct === false)
  },

  actions: {
    setCurrentQuestion(question) {
      this.currentQuestion = question
    },

    addAnswerRecord(record) {
      this.answerHistory.push(record)
    },

    setAnswerHistory(history) {
      this.answerHistory = history || []
    },

    clearAll() {
      this.currentQuestion = null
      this.answerHistory = []
    },

    togglePanel(visible) {
      this.aiPanelVisible = visible !== undefined ? visible : !this.aiPanelVisible
    },

    // 从后端加载历史答题记录（刷新页面后恢复数据）
    async loadHistoryFromBackend() {
      if (this.historyLoaded) return
      try {
        // 获取已完成的测验列表
        const examsData = await request.get('/mobile/exams', { params: { page: 1, size: 20, status: 'completed' } })
        const exams = examsData?.items || examsData?.data || examsData || []
        const completedExams = exams.filter(e => e.status === 'completed' || e.isSubmitted === true || e.score !== undefined)

        // 并行获取每个测验的详情
        const allRecords = []
        for (const exam of completedExams) {
          try {
            const testId = exam.id || exam.testId
            const result = await request.get(`/mobile/exams/${testId}/result`)
            const questions = result?.questionAnswers || result?.questions || []
            questions.forEach((q, idx) => {
              allRecords.push({
                id: q.questionId || q.id || `${testId}-${idx}`,
                question: q.stem || q.question || q.title || '',
                options: q.options || [],
                userAnswer: q.userAnswer || '',
                correctAnswer: q.correctAnswer || '',
                correct: q.isCorrect === true,
                knowledgePoint: q.knowledgePoint || q.category || '',
                testId: testId,
                testName: exam.paperName || exam.title || '',
                submittedAt: result.submittedAt || exam.submittedAt
              })
            })
          } catch (e) {
            // 单个测验加载失败不影响其他
          }
        }

        if (allRecords.length > 0) {
          this.answerHistory = allRecords
        }
        this.historyLoaded = true
      } catch (e) {
        // 加载失败，静默处理，用户答题后会有新数据
        this.historyLoaded = true
      }
    }
  }
})
