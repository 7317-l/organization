import request from './request'

/** 未读通知 */
export function getUnreadNotifications(params = {}) {
  return request.get('/notifications/unread', { params })
}

/** 全部通知 */
export function getAllNotifications(params = {}) {
  return request.get('/notifications/all', { params })
}

/** 标记单条已读 */
export function markNotificationRead(id) {
  return request.put(`/notifications/${id}/read`)
}

/** 标记全部已读 */
export function markAllNotificationsRead() {
  return request.put('/notifications/read-all')
}
