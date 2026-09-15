import request from './request'

// 党员发展台账列表
export function getPartyDevelopmentList(params) {
  return request.get('/party-development', { params })
}

// 兼容别名
export const getPartyDevelopments = getPartyDevelopmentList

// 党员发展详情
export function getPartyDevelopment(id) {
  return request.get(`/party-development/${id}`)
}

export const getPartyDevelopmentDetail = getPartyDevelopment

// 创建党员发展记录
export function createPartyDevelopment(data) {
  return request.post('/party-development', data)
}

// 提交审核
export function submitPartyDevelopment(id, data) {
  return request.put(`/party-development/${id}/submit`, data)
}

// 审核
export function reviewPartyDevelopment(id, data) {
  return request.put(`/party-development/${id}/review`, data)
}

// 阶段推进
export function advancePartyDevelopment(id) {
  return request.put(`/party-development/${id}/advance`)
}

// AI材料检查
export function aiCheckPartyDevelopment(id) {
  return request.get(`/party-development/${id}/ai-check`)
}

// 材料检查V2
export function checkMaterials(id, data) {
  return request.post(`/party-development/${id}/material-check`, data)
}

// 思想汇报建议
export function getReportSuggestion(id, data) {
  return request.post(`/party-development/${id}/report-suggestion`, data)
}
