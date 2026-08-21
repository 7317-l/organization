import request from './request'

// 登录
export function login(data) {
  return request.post('/auth/login', data)
}

// 获取当前用户
export function getCurrentUser() {
  return request.get('/auth/me')
}

// 登出（前端清除 token 即可，如需后端可扩展）
export function logout() {
  return Promise.resolve()
}
