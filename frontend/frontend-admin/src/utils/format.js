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
    0: '党员',
    1: '支部书记',
    2: '系统管理员',
    PartyMember: '党员',
    BranchSecretary: '支部书记',
    SystemAdmin: '系统管理员',
    admin: '系统管理员',
    secretary: '支部书记',
    member: '党员',
    probationary: '预备党员',
    activist: '入党积极分子'
  }
  if (role === null || role === undefined || role === '') return '-'
  return map[role] || String(role)
}

/** 积分来源类型中文映射 */
export function pointSourceText(sourceType) {
  const map = {
    0: '观看视频',
    1: '完成答题',
    2: '活动打卡',
    3: '党史PK胜利',
    4: '其他',
    WatchVideo: '观看视频',
    CompleteExam: '完成答题',
    ActivityCheckIn: '活动打卡',
    BattleVictory: '党史PK胜利',
    Other: '其他'
  }
  if (sourceType === null || sourceType === undefined || sourceType === '') return '其他'
  return map[sourceType] || String(sourceType)
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
