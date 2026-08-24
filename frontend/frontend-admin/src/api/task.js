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
