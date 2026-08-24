import request from './request'

/** 未读通知 */
export function getUnreadNotifications(params = {}) {
  return request.get('/notifications/unread', { params })
}
