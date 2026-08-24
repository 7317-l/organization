import request from './request'

// 党员分页列表
export function getMembers(params) {
  return request.get('/members', { params })
}

// 新增党员
export function createMember(data) {
  return request.post('/members', data)
}

// 编辑党员
export function updateMember(id, data) {
  return request.put(`/members/${id}`, data)
}

// 删除党员
export function deleteMember(id) {
  return request.delete(`/members/${id}`)
}

// 分配角色
export function assignMemberRole(id, role) {
  return request.put(`/members/${id}/role`, { role })
}

// 批量导入
export function importMembers(file) {
  const formData = new FormData()
  formData.append('file', file)
  return request.post('/members/import', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
}

// 导出
export function exportMembers() {
  return request.get('/members/export', { responseType: 'blob' })
}
