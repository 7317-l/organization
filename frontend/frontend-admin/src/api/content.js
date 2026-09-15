import request from './request'

// 内容列表
export function getContents(params) {
  return request.get('/contents', { params })
}

// 发布内容
export function createContent(data) {
  return request.post('/contents', data)
}

// 编辑内容
export function updateContent(id, data) {
  return request.put(`/contents/${id}`, data)
}

// 删除内容
export function deleteContent(id) {
  return request.delete(`/contents/${id}`)
}

// 分类树
export function getContentCategoriesTree() {
  return request.get('/contents/categories/tree')
}

// 创建分类
export function createContentCategory(data) {
  return request.post('/contents/categories', data)
}

// 标签列表
export function getContentTags() {
  return request.get('/contents/tags')
}

// 创建标签
export function createContentTag(data) {
  return request.post('/contents/tags', data)
}

// 获取内容详情（党员端）
export function getContentDetail(contentId) {
  return request.get(`/mobile/contents/${contentId}`)
}

// 上报学习进度（党员端）
export function reportProgress(data) {
  return request.post('/mobile/progress', data)
}
