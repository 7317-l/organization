/**
 * 日期格式化工具
 */

export function formatDate(date, fmt = 'YYYY-MM-DD HH:mm:ss') {
  if (!date) return ''
  const d = new Date(date)
  if (isNaN(d.getTime())) return ''
  const o = {
    'YYYY': d.getFullYear(),
    'MM': String(d.getMonth() + 1).padStart(2, '0'),
    'DD': String(d.getDate()).padStart(2, '0'),
    'HH': String(d.getHours()).padStart(2, '0'),
    'mm': String(d.getMinutes()).padStart(2, '0'),
    'ss': String(d.getSeconds()).padStart(2, '0')
  }
  let result = fmt
  for (const k in o) {
    result = result.replace(k, o[k])
  }
  return result
}

export function formatDateShort(date) {
  return formatDate(date, 'YYYY-MM-DD')
}

export function roleText(role) {
  const map = {
    admin: '管理员',
    secretary: '支部书记',
    member: '党员',
    probationary: '预备党员',
    activist: '入党积极分子'
  }
  return map[role] || role || '-'
}

export function contentTypeText(type) {
  const map = {
    article: '文章',
    video: '视频',
    audio: '音频',
    document: '文档'
  }
  return map[type] || type || '-'
}

export function questionTypeText(type) {
  const map = {
    single: '单选题',
    multiple: '多选题',
    judge: '判断题',
    fill: '填空题',
    essay: '简答题'
  }
  return map[type] || type || '-'
}

export function meetingTypeText(type) {
  const map = {
    branch_meeting: '支部党员大会',
    committee_meeting: '支部委员会',
    group_meeting: '党小组会',
    party_class: '党课'
  }
  return map[type] || type || '-'
}
