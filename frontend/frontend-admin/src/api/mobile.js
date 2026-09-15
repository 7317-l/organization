import request from './request'

/** 个人学习概览（总积分、完成率等） */
export function getOverview() {
  return request.get('/mobile/overview')
}

/** AI 推荐内容 */
export function getRecommendations(params = {}) {
  return request.get('/mobile/recommendations', { params })
}

/** AI 学习报告 */
export function getAiAssessment(data = {}) {
  return request.post('/mobile/report/ai-assessment', data)
}
