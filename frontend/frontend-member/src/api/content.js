import request from './request'

/** 获取可学内容列表（公共+支部任务内容） */
export function getContents(params = {}) {
  return request.get('/mobile/contents', { params })
}

/** 获取内容详情 */
export function getContentDetail(contentId) {
  return request.get(`/mobile/contents/${contentId}`)
}

/** 上报学习进度 */
export function reportProgress(data) {
  return request.post('/mobile/progress', data)
}
