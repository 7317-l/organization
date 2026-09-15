import request from './request'

// 仪表盘
export function getDashboard() {
  return request.get('/statistics/dashboard')
}

// 大屏数据
export function getDashboardLargeScreen() {
  return request.get('/statistics/dashboard-largescreen')
}

// 挂机统计
export function getAntiCheat(params) {
  return request.get('/statistics/anti-cheat', { params })
}
