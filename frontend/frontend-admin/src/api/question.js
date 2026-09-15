import request from './request'

// 题目列表
export function getQuestions(params) {
  return request.get('/questions', { params })
}

// 新增题目
export function createQuestion(data) {
  return request.post('/questions', data)
}

// 编辑题目
export function updateQuestion(id, data) {
  return request.put(`/questions/${id}`, data)
}

// 删除题目
export function deleteQuestion(id) {
  return request.delete(`/questions/${id}`)
}

// 批量导入
export function importQuestions(file) {
  const formData = new FormData()
  formData.append('file', file)
  return request.post('/questions/import', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
}

// 题目分类
export function getQuestionCategories() {
  return request.get('/questions/categories')
}

// 创建题目分类
export function createQuestionCategory(data) {
  return request.post('/questions/categories', data)
}
