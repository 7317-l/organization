// AI 悬浮面板公共模块：API 封装、token、组织/党员加载、脱敏、语音识别
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

// 组织树 → 扁平列表（供选择支部）
export async function loadOrgList() {
  try {
    const tree = await apiGet('/organizations/tree')
    const flat = []
    const flatten = (nodes) => {
      (nodes || []).forEach(n => {
        flat.push({ id: n.id, name: n.name })
        if (n.children && n.children.length) flatten(n.children)
      })
    }
    flatten(Array.isArray(tree) ? tree : [tree])
    if (flat.length === 0) flat.push({ id: 1, name: '第一党支部' })
    return flat
  } catch (e) {
    ElMessage.warning('组织列表加载失败，使用示例支部')
    return [{ id: 1, name: '第一党支部' }, { id: 2, name: '第二党支部' }]
  }
}

// 按组织加载党员
export async function loadMemberList(orgId, keyword = '') {
  if (!orgId) return []
  try {
    const data = await apiGet('/members', { orgId, page: 1, size: 100, name: keyword })
    const items = (data?.items || data?.data?.items || []) || []
    return items.map(m => ({ id: m.id, name: m.name, phone: m.phone, orgName: m.organizationName }))
  } catch (e) {
    return []
  }
}

// 手机号等敏感信息脱敏
export function maskPhone(v) {
  if (typeof v !== 'string') return v
  if (/^1\d{10}$/.test(v)) return v.slice(0, 3) + '****' + v.slice(7)
  return v
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
    if (loading.value) return
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

// 组织 → 党员 联动选择钩子
export function useOrgMember() {
  const orgs = ref([])
  const orgId = ref(null)
  const members = ref([])
  const memberId = ref(null)
  const memberLoading = ref(false)

  async function loadOrgs() {
    orgs.value = await loadOrgList()
  }

  async function onOrgChange() {
    memberId.value = null
    members.value = []
    if (!orgId.value) return
    memberLoading.value = true
    try {
      members.value = await loadMemberList(orgId.value)
    } finally {
      memberLoading.value = false
    }
  }

  function memberName(id) {
    const m = members.value.find(x => x.id === id)
    return m ? m.name : ('党员' + id)
  }

  return { orgs, orgId, members, memberId, memberLoading, loadOrgs, onOrgChange, memberName }
}
