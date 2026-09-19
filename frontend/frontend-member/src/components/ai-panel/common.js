// 党员端 AI 悬浮面板公共模块：API 封装、token、当前用户、脱敏、语音识别
import { ref } from 'vue'
import { ElMessage } from 'element-plus'

export const API_BASE = 'http://localhost:5091/api/v1'

export function getToken() {
  return localStorage.getItem('accessToken') || ''
}

export async function apiGet(path, params = {}) {
  const qs = new URLSearchParams()
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== '') qs.append(k, v)
  })
  const q = qs.toString()
  const res = await fetch(`${API_BASE}${path}${q ? '?' + q : ''}`, {
    headers: { 'Authorization': `Bearer ${getToken()}` }
  })
  const data = await res.json()
  if (data?.code === 200) return data.data
  throw new Error(data?.message || '请求失败')
}

export async function apiPost(path, body = {}) {
  const res = await fetch(`${API_BASE}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${getToken()}` },
    body: JSON.stringify(body)
  })
  const data = await res.json()
  if (data?.code === 200) return data.data
  throw new Error(data?.message || '请求失败')
}

// ========== SSE 流式请求（用于 AI 打字机式回答） ==========
function parseSseEvent(raw) {
  let event = 'message'
  let data = ''
  raw.split('\n').forEach(line => {
    if (line.startsWith('event:')) event = line.slice(6).trim()
    else if (line.startsWith('data:')) data += line.slice(5).trim() + '\n'
  })
  data = data.trim()
  if (!data) return null
  return { event, data }
}

/**
 * POST 一个流式接口（SSE），边接收边回调。
 * handlers: { onMeta(meta), onDelta(text), onDone(data), onError(message) }
 */
export async function apiPostStream(path, body = {}, handlers = {}) {
  const controller = new AbortController()
  // 整体超时 90 秒，避免网络/服务挂起时前端无限等待
  const overallTimer = setTimeout(() => controller.abort(), 90000)
  let res
  try {
    res = await fetch(`${API_BASE}${path}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${getToken()}` },
      body: JSON.stringify(body),
      signal: controller.signal
    })
  } catch (e) {
    clearTimeout(overallTimer)
    if (e?.name === 'AbortError') throw new Error('回答超时（90秒），请检查网络后重试')
    throw new Error('无法连接后端服务（端口5091），请确认已启动')
  }
  if (res.status === 401) {
    clearTimeout(overallTimer)
    throw new Error('登录已过期，请重新登录后再提问')
  }
  if (!res.ok || !res.body) {
    clearTimeout(overallTimer)
    const text = await res.text()
    let msg = '请求失败'
    try { msg = JSON.parse(text)?.message || msg } catch {}
    throw new Error(msg)
  }
  const reader = res.body.getReader()
  const decoder = new TextDecoder('utf-8')
  let buffer = ''
  // 断流兜底：20 秒未收到任何数据帧视为连接中断
  let idleTimer = setTimeout(() => controller.abort(), 8000)
  function kickIdle() {
    clearTimeout(idleTimer)
    idleTimer = setTimeout(() => controller.abort(), 8000)
  }
  try {
    while (true) {
      const { done, value } = await reader.read()
      if (done) break
      kickIdle()
      buffer += decoder.decode(value, { stream: true })
      let idx
      while ((idx = buffer.indexOf('\n\n')) >= 0) {
        const raw = buffer.slice(0, idx)
        buffer = buffer.slice(idx + 2)
        const evt = parseSseEvent(raw)
        if (!evt) continue
        if (evt.event === 'meta' && handlers.onMeta) {
          try { handlers.onMeta(JSON.parse(evt.data)) } catch {}
        } else if (evt.event === 'delta' && handlers.onDelta) {
          try {
            const text = JSON.parse(evt.data)?.text || ''
            if (text) handlers.onDelta(text)
          } catch {}
        } else if (evt.event === 'done' && handlers.onDone) {
          let d = {}
          try { d = JSON.parse(evt.data) } catch {}
          handlers.onDone(d)
        } else if (evt.event === 'error' && handlers.onError) {
          let d = {}
          try { d = JSON.parse(evt.data) } catch {}
          handlers.onError(d?.message || '回答生成失败')
        }
      }
    }
  } catch (e) {
    clearTimeout(overallTimer)
    clearTimeout(idleTimer)
    if (e?.name === 'AbortError') {
      throw new Error('回答中断（网络不稳定或服务无响应），请重试')
    }
    throw e
  } finally {
    clearTimeout(overallTimer)
    clearTimeout(idleTimer)
    reader.releaseLock()
  }
}

// 手机号等敏感信息脱敏
export function maskPhone(v) {
  if (typeof v !== 'string') return v
  if (/^1\d{10}$/.test(v)) return v.slice(0, 3) + '****' + v.slice(7)
  return v
}

// ========== 当前登录党员（用于"本人"类接口） ==========
let cachedUser = null
export async function getCurrentUser(force = false) {
  if (cachedUser && !force) return cachedUser
  try {
    const user = await apiGet('/auth/me')
    cachedUser = user
    return user
  } catch (e) {
    if (force) throw e
    return null
  }
}

// ========== 语音识别（Web Speech API） ==========
export function speechSupported() {
  return typeof window !== 'undefined' && (window.SpeechRecognition || window.webkitSpeechRecognition)
}

export function createRecognizer(onResult, onEnd, onError) {
  const SR = window.SpeechRecognition || window.webkitSpeechRecognition
  if (!SR) {
    if (onError) onError('当前浏览器不支持语音识别，请使用 Chrome/Edge')
    return null
  }
  const rec = new SR()
  rec.lang = 'zh-CN'
  rec.interimResults = false
  rec.maxAlternatives = 1
  rec.onresult = (e) => {
    if (onResult && e.results && e.results[0]) onResult(e.results[0][0].transcript)
  }
  rec.onerror = (e) => { if (onError) onError('语音识别失败：' + (e.error || '未知错误')) }
  rec.onend = () => { if (onEnd) onEnd() }
  return rec
}

// 通用加载状态钩子
export function useLoading() {
  const loading = ref(false)
  async function withLoading(fn) {
    if (loading.value) return null
    loading.value = true
    try {
      return await fn()
    } catch (e) {
      ElMessage.error(e?.message || '请求失败，请确认后端服务已启动（端口5091）')
      return null
    } finally {
      loading.value = false
    }
  }
  return { loading, withLoading }
}
