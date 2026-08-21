import request from './request'

// 树形组织
export function getOrganizationTree() {
  return request.get('/organizations/tree')
}

// 创建组织
export function createOrganization(data) {
  return request.post('/organizations', data)
}

// 编辑组织
export function updateOrganization(id, data) {
  return request.put(`/organizations/${id}`, data)
}

// 删除组织
export function deleteOrganization(id) {
  return request.delete(`/organizations/${id}`)
}
