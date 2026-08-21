import request from './request'

// 自然语言查询 (NL2SQL)
export function aiQuery(data) {
  return request.post('/ai/query', data)
}

// AI 素材生成
export function aiContentGenerate(data) {
  return request.post('/ai-content/generate', data)
}

// 党建知识问答
export function aiKnowledgeQuery(data) {
  return request.post('/ai-knowledge/query', data)
}

// 错题聚类
export function kmeansCluster(data) {
  return request.post('/kmeans/cluster', data)
}
