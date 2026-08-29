/**
 * 格式化工具函数
 */

/** 格式化日期 YYYY-MM-DD */
export function formatDate(date) {
  if (!date) return ''
  const d = new Date(date)
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

/** 格式化日期时间 YYYY-MM-DD HH:mm */
export function formatDateTime(date) {
  if (!date) return ''
  const d = new Date(date)
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  const h = String(d.getHours()).padStart(2, '0')
  const min = String(d.getMinutes()).padStart(2, '0')
  return `${y}-${m}-${day} ${h}:${min}`
}

/** 格式化时间 mm:ss */
export function formatTime(seconds) {
  if (!seconds && seconds !== 0) return ''
  const m = Math.floor(seconds / 60)
  const s = seconds % 60
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
}

/** 获取用户姓名首字（用于头像） */
export function getAvatarChar(name) {
  if (!name) return '?'
  return name.charAt(0)
}

/** 内容类型映射 */
export const contentTypeMap = {
  video: { label: '视频', tagClass: 'tag-r', icon: '▶' },
  doc: { label: '文章', tagClass: 'tag-b', icon: '📄' },
  article: { label: '文章', tagClass: 'tag-b', icon: '📄' },
  audio: { label: '音频', tagClass: 'tag-o', icon: '🎵' }
}

/** 题型映射 */
export const questionTypeMap = {
  single: '单选题',
  multiple: '多选题',
  judge: '判断题',
  0: '单选题',
  1: '多选题',
  2: '判断题'
}

/** 任务状态映射 */
export const taskStatusMap = {
  pending: { label: '待完成', tagClass: 'tag-r' },
  in_progress: { label: '进行中', tagClass: 'tag-o' },
  completed: { label: '已完成', tagClass: 'tag-g' }
}

/** 考试状态映射 */
export const examStatusMap = {
  pending: { label: '待考', tagClass: 'tag-r' },
  in_progress: { label: '进行中', tagClass: 'tag-o' },
  completed: { label: '已完成', tagClass: 'tag-g' }
}

/** 评级 */
export function getLevel(score) {
  if (score >= 90) return { text: '优秀', color: 'var(--green)' }
  if (score >= 75) return { text: '良好', color: 'var(--blue)' }
  if (score >= 60) return { text: '一般', color: 'var(--orange)' }
  return { text: '待提升', color: 'var(--red)' }
}

/** 角色中文映射（兼容数字枚举 0/1/2 和字符串枚举 PartyMember/BranchSecretary/SystemAdmin） */
export function roleText(role) {
  const map = {
    0: '党员',
    1: '支部书记',
    2: '系统管理员',
    PartyMember: '党员',
    BranchSecretary: '支部书记',
    SystemAdmin: '系统管理员',
    member: '党员',
    secretary: '支部书记',
    admin: '系统管理员'
  }
  if (role === null || role === undefined || role === '') return '-'
  return map[role] || String(role)
}

/** 积分来源类型中文映射（兼容数字枚举和英文枚举名） */
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

/** 内容类型判断（兼容数字 contentType 和字符串 type） */
export function isVideoContent(content) {
  if (!content) return false
  if (content.contentType !== undefined) return content.contentType === 1
  if (content.type) return content.type === 'video' || content.type === 1
  return false
}
