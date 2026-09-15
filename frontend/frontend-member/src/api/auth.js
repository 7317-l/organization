import request from './request'

/** 登录 */
export function login(phone, password) {
  return request.post('/auth/login', { phone, password })
}

/** 获取当前用户信息 */
export function getMe() {
  return request.get('/auth/me')
}
