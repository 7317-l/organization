import request from './request'

/** 获取待完成任务 */
export function getPendingTasks(params = {}) {
  return request.get('/mobile/tasks/pending', { params })
}

/** 获取已完成任务 */
export function getCompletedTasks(params = {}) {
  return request.get('/mobile/tasks/completed', { params })
}

/** 完成任务 */
export function completeTask(data) {
  return request.post('/mobile/tasks/complete', data)
}
