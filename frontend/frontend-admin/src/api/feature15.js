import request from './request'

// ========== (1) NL2SQL 多轮上下文 ==========
export function nl2SqlQuery(data) {
  return request.post('/nl2sql/query', data)
}
export function nl2SqlHistory(sessionId, limit = 5) {
  return request.get('/nl2sql/history', { params: { sessionId, limit } })
}

// ========== (2) RAG 两级检索 ==========
export function aiKnowledgeQueryV2(data) {
  return request.post('/ai-knowledge/query', data)
}

// ========== (3) AI 内容生成 ==========
export function aiContentGenerateV2(data) {
  return request.post('/ai-content/generate', data)
}

// ========== (4) AI 学习标兵 ==========
export function generateStarMembers(data) {
  return request.post('/ai/star-members', data)
}

// ========== (5) 三会一课简报 ==========
export function generateMeetingBrief(data) {
  return request.post('/meeting-activities/brief', data)
}

// ========== (6) 思想汇报 AI 建议 ==========
export function getReportSuggestion(id, data) {
  return request.post(`/party-development/${id}/report-suggestion`, data)
}

// ========== (7) 发展材料 AI 校验 ==========
export function checkMaterials(id, data) {
  return request.post(`/party-development/${id}/material-check`, data)
}

// ========== (8) 到期提醒 ==========
export function triggerReminders(data) {
  return request.post('/party-development/reminders/trigger', data)
}
export function getRemindersList(params) {
  return request.get('/party-development/reminders/list', { params })
}

// ========== (9) 支部评级 + 整改 ==========
export function getRectifications(params) {
  return request.get('/rectifications', { params })
}
export function createRectification(data) {
  return request.post('/rectifications', data)
}
export function completeRectification(id, data) {
  return request.put(`/rectifications/${id}/complete`, data)
}
export function updateRectificationStatus(id, data) {
  return request.put(`/rectifications/${id}/status`, data)
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

// ========== (12) 红色教育基地 ==========
export function getEducationSites(params) {
  return request.get('/education-sites', { params })
}
export function getEducationSite(id) {
  return request.get(`/education-sites/${id}`)
}
export function createEducationSite(data) {
  return request.post('/education-sites', data)
}
export function updateEducationSite(id, data) {
  return request.put(`/education-sites/${id}`, data)
}
export function deleteEducationSite(id) {
  return request.delete(`/education-sites/${id}`)
}
export function getSiteCheckins(id, params) {
  return request.get(`/education-sites/${id}/checkins`, { params })
}

// ========== (13) 学习路线图 ==========
export function generateRoadmap(data) {
  return request.post('/ai/roadmap', data)
}

// ========== (14) 精准分层推送 ==========
export function targetedSend(data) {
  return request.post('/notifications/targeted-send', data)
}

// ========== (15) 防挂机 ==========
export function getAntiCheatChallengeV2(contentId) {
  return request.get('/anti-cheat/challenge-v2', { params: { contentId } })
}
export function verifyAntiCheatV2(data) {
  return request.post('/anti-cheat/verify-v2', data)
}
export function getAntiCheatStatsOverview(orgId) {
  return request.get('/anti-cheat/stats-overview', { params: { orgId } })
}
