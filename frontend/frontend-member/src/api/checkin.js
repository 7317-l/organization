import request from './request'

/** 我的打卡记录 */
export function getMyCheckIns(params = {}) {
  return request.get('/check-in/my', { params })
}
