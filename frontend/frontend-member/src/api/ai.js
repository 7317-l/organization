import request from './request'

/** AI 党建问答 */
export function aiQuery(question) {
  return request.post('/ai-knowledge/query', { question })
}

/** 错题聚类（获取薄弱知识点） */
export function kmeansCluster(memberId) {
  return request.post('/kmeans/cluster', { partyMemberId: memberId })
}

/** AI 通用聊天（替代3000端口） */
export function aiChat(message, context = []) {
  return request.post('/ai-knowledge/query', { question: message })
}

/** AI 学习分析总结 */
export function aiAnalyzeLearning(data) {
  return request.post('/ai-knowledge/query', {
    question: `请根据以下学习数据生成总结：答题${data.totalQuestions || 0}题，正确${data.correctQuestions || 0}题，薄弱点：${(data.knowledgePoints || []).join('、')}`
  })
}

/** AI 题目帮助 */
export function aiQuestionHelp(question, options = []) {
  return request.post('/ai-knowledge/query', {
    question: `请解答这道题：${question}。选项：${options.join('、')}`
  })
}

/** AI 个性化推荐 */
export function aiRecommendations(limit = 5) {
  return request.get('/mobile/recommendations', { params: { limit } })
}

/** AI 个人学习报告 */
export function aiAssessment() {
  return request.post('/mobile/report/ai-assessment', {})
}

/** AI 学习路线 */
export function aiRoadmap(data) {
  return request.post('/ai/roadmap', data)
}
