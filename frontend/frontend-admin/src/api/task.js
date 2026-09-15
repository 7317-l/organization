import request from './request'

// 任务列表
export function getTasks(params) {
  return request.get('/tasks', { params })
}

// 创建任务
export function createTask(data) {
  return request.post('/tasks', data)
}

// 编辑任务
export function updateTask(id, data) {
  return request.put(`/tasks/${id}`, data)
}

// 删除任务
export function deleteTask(id) {
  return request.delete(`/tasks/${id}`)
}

// 任务完成详情
export function getTaskCompletion(id) {
  return request.get(`/tasks/${id}/completion`)
}

// 催办任务
export function urgeTask(id) {
  return request.post(`/tasks/${id}/urge`)
}

// 获取待完成任务（党员端）
export function getPendingTasks(params = {}) {
  return request.get('/mobile/tasks/pending', { params })
}

// 获取已完成任务（党员端）
export function getCompletedTasks(params = {}) {
  return request.get('/mobile/tasks/completed', { params })
}

// 完成任务（党员端）
export function completeTask(data) {
  return request.post('/mobile/tasks/complete', data)
}
