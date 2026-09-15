import request from './request'

// ========== (11) 党史 PK 对战 ==========
export function createBattle(data) {
  return request.post('/battle/create', data)
}
export function getPendingBattles() {
  return request.get('/battle/pending')
}
export function acceptBattle(gameId) {
  return request.post(`/battle/${gameId}/accept`)
}
export function cancelBattle(gameId) {
  return request.post(`/battle/${gameId}/cancel`)
}
export function getBattleQuestion(gameId) {
  return request.get(`/battle/${gameId}/question`)
}
export function submitBattleAnswer(gameId, data) {
  return request.post(`/battle/${gameId}/answer`, data)
}
export function finishBattle(gameId) {
  return request.post(`/battle/${gameId}/finish`)
}
export function getBattleResult(gameId) {
  return request.get(`/battle/${gameId}/result`)
}
export function forfeitBattle(gameId) {
  return request.post(`/battle/${gameId}/forfeit`)
}

// ========== (10) 薄弱点互助 ==========
export function recommendPairHelp(data) {
  return request.post('/pair-help/recommend', data)
}
export function requestPairHelp(data) {
  return request.post('/pair-help/request', data)
}
export function acceptPairHelp(requestId) {
  return request.post(`/pair-help/requests/${requestId}/accept`)
}
export function rejectPairHelp(requestId) {
  return request.post(`/pair-help/requests/${requestId}/reject`)
}
export function getMyPairHelp() {
  return request.get('/pair-help/my')
}
export function completePairHelp(recordId, data) {
  return request.post(`/pair-help/records/${recordId}/complete`, data)
}
export function logPairHelp(recordId, data) {
  return request.post(`/pair-help/records/${recordId}/log`, data)
}

// ========== (13) 学习路线图 ==========
export function generateRoadmap(data) {
  return request.post('/ai/roadmap', data)
}

// ========== (12) 红色教育基地 ==========
export function getEducationSites(params) {
  return request.get('/education-sites', { params })
}
export function getEducationSite(id) {
  return request.get(`/education-sites/${id}`)
}

// ========== (15) 防挂机 ==========
export function getAntiCheatChallengeV2(contentId) {
  return request.get('/anti-cheat/challenge-v2', { params: { contentId } })
}
export function verifyAntiCheatV2(data) {
  return request.post('/anti-cheat/verify-v2', data)
}
