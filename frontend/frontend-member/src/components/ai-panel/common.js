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
