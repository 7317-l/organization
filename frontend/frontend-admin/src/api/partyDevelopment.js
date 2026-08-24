import request from './request'

// 党员发展流程列表（分页）
export function getPartyDevelopments(params) {
  return request.get('/party-development', { params })
}

// 获取发展流程详情（时间线）
export function getPartyDevelopmentDetail(id) {
  return request.get(`/party-development/${id}`)
}

// 创建发展流程记录
export function createPartyDevelopment(data) {
  return request.post('/party-development', data)
}

// 更新发展流程记录
export function updatePartyDevelopment(id, data) {
  return request.put(`/party-development/${id}`, data)
}

// 删除发展流程记录
export function deletePartyDevelopment(id) {
  return request.delete(`/party-development/${id}`)
}
