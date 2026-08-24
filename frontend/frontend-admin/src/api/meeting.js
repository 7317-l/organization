import request from './request'

// 活动列表
export function getMeetingActivities(params) {
  return request.get('/meeting-activities', { params })
}

// 创建活动
export function createMeetingActivity(data) {
  return request.post('/meeting-activities', data)
}

// 删除活动
export function deleteMeetingActivity(id) {
  return request.delete(`/meeting-activities/${id}`)
}

// 提交心得
export function submitHeart(data) {
  return request.post('/meeting-activities/hearts', data)
}

// 查看心得
export function getHearts(id) {
  return request.get(`/meeting-activities/${id}/hearts`)
}

// 生成 AI 总结
export function generateAiSummary(id) {
  return request.post(`/meeting-activities/${id}/ai-summary`)
}
