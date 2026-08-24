import request from './request'

/** AI 党建问答 */
export function aiQuery(question) {
  return request.post('/ai-knowledge/query', { question })
}

/** 错题聚类（获取薄弱知识点） */
export function kmeansCluster(memberId) {
  return request.post('/kmeans/cluster', { memberId })
}
