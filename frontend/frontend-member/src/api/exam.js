import request from './request'

/** 获取测验列表（待参加+已完成） */
export function getExams(params = {}) {
  return request.get('/mobile/exams', { params })
}

/** 开始测验，获取题目 */
export function startExam(testId) {
  return request.get(`/mobile/exams/${testId}/start`)
}

/** 提交测验答案 */
export function submitExam(data) {
  return request.post('/mobile/exams/submit', data)
}

/** 获取测验结果 */
export function getExamResult(testId) {
  return request.get(`/mobile/exams/${testId}/result`)
}
