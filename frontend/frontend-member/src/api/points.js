import request from './request'

/** 积分记录 */
export function getPointsRecords(params = {}) {
  return request.get('/points/records', { params })
}

/** 积分排行榜 */
export function getPointsRanking(params = {}) {
  return request.get('/points/ranking', { params })
}
