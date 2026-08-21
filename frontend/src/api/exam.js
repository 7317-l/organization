import request from './request'

// 试卷列表
export function getExamPapers() {
  return request.get('/exam-papers')
}

// 创建试卷
export function createExamPaper(data) {
  return request.post('/exam-papers', data)
}

// 编辑试卷
export function updateExamPaper(id, data) {
  return request.put(`/exam-papers/${id}`, data)
}

// 删除试卷
export function deleteExamPaper(id) {
  return request.delete(`/exam-papers/${id}`)
}

// 测验列表
export function getExamTests(params) {
  return request.get('/exam-tests', { params })
}

// 发布测验
export function createExamTest(data) {
  return request.post('/exam-tests', data)
}

// 测验成绩
export function getExamTestResults(id) {
  return request.get(`/exam-tests/${id}/results`)
}
