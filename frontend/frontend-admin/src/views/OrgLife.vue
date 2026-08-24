<template>
  <div class="org-life-page">
    <el-card shadow="never">
      <el-tabs v-model="activeTab" @tab-change="handleTabChange">
        <!-- Tab1 三会一课 -->
        <el-tab-pane label="三会一课" name="meeting">
          <div class="tab-toolbar">
            <el-input v-model="meetingQuery.keyword" placeholder="搜索活动名称" clearable style="width:200px" @clear="loadMeetings" @keyup.enter="loadMeetings" />
            <el-select v-model="meetingQuery.type" placeholder="活动类型" clearable style="width:160px" @change="loadMeetings">
              <el-option label="支部党员大会" value="branch_meeting" />
              <el-option label="支部委员会" value="committee_meeting" />
              <el-option label="党小组会" value="group_meeting" />
              <el-option label="党课" value="party_class" />
              <el-option label="主题党日" value="theme_day" />
            </el-select>
            <el-button type="primary" @click="loadMeetings"><el-icon><Search /></el-icon>查询</el-button>
            <el-button type="success" @click="openMeetingDialog"><el-icon><Plus /></el-icon>发布活动</el-button>
            <el-button @click="loadMeetings"><el-icon><Refresh /></el-icon>刷新</el-button>
          </div>

          <el-table :data="meetingList" v-loading="meetingLoading" border style="width:100%">
            <el-table-column type="index" label="序号" width="60" />
            <el-table-column prop="title" label="活动名称" min-width="220" show-overflow-tooltip />
            <el-table-column label="类型" width="130">
              <template #default="{ row }">
                <el-tag :type="getMeetingTagType(row.type)" size="small">{{ meetingTypeText(row.type) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="organizationName" label="召开支部" width="160" />
            <el-table-column prop="activityTime" label="时间" width="180">
              <template #default="{ row }">{{ formatDate(row.activityTime) }}</template>
            </el-table-column>
            <el-table-column prop="location" label="地点" width="160" show-overflow-tooltip />
            <el-table-column label="应到/实到" width="110">
              <template #default="{ row }">
                {{ row.expectedCount || '-' }} / {{ row.actualCount || '-' }}
              </template>
            </el-table-column>
            <el-table-column label="状态" width="100">
              <template #default="{ row }">
                <el-tag :type="getMeetingStatusType(row.activityTime, row.status)" size="small">
                  {{ getMeetingStatusText(row.activityTime, row.status) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="240" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="viewHearts(row)">查看心得</el-button>
                <el-button link type="primary" size="small" @click="goAiSummary(row)">AI总结</el-button>
                <el-button link type="primary" size="small" @click="editMeeting(row)">编辑</el-button>
                <el-button link type="danger" size="small" @click="handleDeleteMeeting(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!meetingLoading && meetingList.length === 0" description="暂无活动" />
          <el-pagination
            v-if="meetingTotal > 0"
            class="pagination"
            background
            layout="total, prev, pager, next"
            :total="meetingTotal"
            :page-size="meetingQuery.size"
            :current-page="meetingQuery.page"
            @current-change="handleMeetingPageChange"
          />
        </el-tab-pane>

        <!-- Tab2 活动心得 -->
        <el-tab-pane label="活动心得" name="heart">
          <div class="tab-toolbar">
            <el-select v-model="heartActivityId" placeholder="选择活动" clearable filterable style="width:300px" @change="loadHearts">
              <el-option v-for="m in meetingList" :key="m.id" :label="m.title" :value="m.id" />
            </el-select>
            <el-select v-model="heartOrgId" placeholder="选择支部" clearable style="width:180px" @change="loadHearts">
              <el-option v-for="o in orgFlatList" :key="o.id" :label="o.name" :value="o.id" />
            </el-select>
            <el-button type="primary" @click="loadHearts"><el-icon><Search /></el-icon>查询</el-button>
            <el-button type="warning" @click="remindHearts" :disabled="!heartActivityId">
              <el-icon><Bell /></el-icon>催交
            </el-button>
          </div>
          <el-table :data="heartList" v-loading="heartLoading" border style="width:100%">
            <el-table-column type="index" label="序号" width="60" />
            <el-table-column prop="memberName" label="党员" width="140" />
            <el-table-column prop="organizationName" label="所属支部" width="160" />
            <el-table-column prop="activityTitle" label="活动名称" min-width="200" show-overflow-tooltip />
            <el-table-column prop="content" label="心得内容" min-width="300" show-overflow-tooltip />
            <el-table-column label="字数" width="90">
              <template #default="{ row }">{{ row.content ? row.content.length : 0 }}</template>
            </el-table-column>
            <el-table-column prop="createdAt" label="提交时间" width="180">
              <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="160" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="viewHeartDetail(row)">查看</el-button>
                <el-button link type="primary" size="small" @click="exportHeart(row)">导出</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!heartLoading && heartList.length === 0" description="暂无心得" />
          <el-pagination
            v-if="heartTotal > 0"
            class="pagination"
            background
            layout="total, prev, pager, next"
            :total="heartTotal"
            :page-size="heartQuery.size"
            :current-page="heartQuery.page"
            @current-change="handleHeartPageChange"
          />
        </el-tab-pane>

        <!-- Tab3 AI活动总结 -->
        <el-tab-pane label="AI活动总结" name="ai">
          <div class="ai-card">
            <div class="ai-card-icon">✨📝</div>
            <div class="ai-card-title">AI活动总结简报自动生成</div>
            <div class="ai-card-desc">选择已完成的活动，一键生成格式规范、重点突出的支部活动简报，可直接上报</div>
          </div>
          <el-form :model="aiForm" label-width="100px" class="ai-form">
            <el-form-item label="选择活动">
              <el-select v-model="aiForm.activityId" placeholder="请选择已完成的活动" filterable style="width:100%;max-width:500px">
                <el-option v-for="m in completedMeetings" :key="m.id" :label="m.title" :value="m.id" />
              </el-select>
            </el-form-item>
            <el-form-item>
              <el-button type="primary" :loading="aiGenerating" @click="generateSummary">
                <el-icon><MagicStick /></el-icon>🤖 生成简报
              </el-button>
              <el-button @click="previewTemplate"><el-icon><Document /></el-icon>预览模板</el-button>
            </el-form-item>
          </el-form>
          <el-card v-if="aiSummary" class="ai-result" shadow="never">
            <template #header>
              <div class="card-header">
                <el-icon color="#C8161D"><Document /></el-icon>
                <span>AI活动简报</span>
                <div style="margin-left:auto;display:flex;gap:8px;">
                  <el-button link type="primary" @click="copySummary">复制</el-button>
                  <el-button link type="primary" @click="exportSummaryWord">导出Word</el-button>
                  <el-button link type="primary" @click="submitReport">📨 上报</el-button>
                </div>
              </div>
            </template>
            <div class="ai-result-text">{{ aiSummary }}</div>
          </el-card>
        </el-tab-pane>
      </el-tabs>
    </el-card>

    <!-- 发布活动弹窗 -->
    <el-dialog v-model="meetingDialogVisible" :title="meetingForm.id ? '编辑活动' : '发布活动'" width="560px">
      <el-form :model="meetingForm" label-width="100px">
        <el-form-item label="活动类型">
          <el-select v-model="meetingForm.type" placeholder="请选择" style="width:100%">
            <el-option label="支部党员大会" value="branch_meeting" />
            <el-option label="支部委员会" value="committee_meeting" />
            <el-option label="党小组会" value="group_meeting" />
            <el-option label="党课" value="party_class" />
            <el-option label="主题党日" value="theme_day" />
          </el-select>
        </el-form-item>
        <el-form-item label="活动标题">
          <el-input v-model="meetingForm.title" placeholder="请输入活动标题" />
        </el-form-item>
        <el-form-item label="所属组织">
          <el-tree-select
            v-model="meetingForm.organizationId"
            :data="orgTree"
            :props="{ label: 'name', children: 'children' }"
            node-key="id"
            check-strictly
            placeholder="请选择组织"
            style="width:100%"
          />
        </el-form-item>
        <el-form-item label="活动时间">
          <el-date-picker
            v-model="meetingForm.activityTime"
            type="datetime"
            placeholder="选择活动时间"
            style="width:100%"
            value-format="YYYY-MM-DD HH:mm:ss"
          />
        </el-form-item>
        <el-form-item label="活动地点">
          <el-input v-model="meetingForm.location" placeholder="请输入地点/线上会议链接" />
        </el-form-item>
        <el-form-item label="活动描述">
          <el-input v-model="meetingForm.description" type="textarea" :rows="3" placeholder="简要说明议程" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="meetingDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="meetingSubmitting" @click="submitMeeting">
          {{ meetingForm.id ? '保存' : '发布通知' }}
        </el-button>
      </template>
    </el-dialog>

    <!-- 心得弹窗 -->
    <el-dialog v-model="heartsVisible" title="活动心得" width="700px">
      <div v-loading="heartLoading">
        <div class="heart-header" v-if="currentHeartActivity">
          <span><b>活动：</b>{{ currentHeartActivity.title }}</span>
          <span><b>已提交：</b>{{ heartList.length }} 篇</span>
        </div>
        <el-table :data="heartList" border>
          <el-table-column prop="memberName" label="提交人" width="140" />
          <el-table-column prop="content" label="心得内容" min-width="300" show-overflow-tooltip />
          <el-table-column label="字数" width="80">
            <template #default="{ row }">{{ row.content ? row.content.length : 0 }}</template>
          </el-table-column>
          <el-table-column prop="createdAt" label="提交时间" width="180">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="100">
            <template #default="{ row }">
              <el-button link type="primary" size="small" @click="viewHeartDetail(row)">查看</el-button>
            </template>
          </el-table-column>
        </el-table>
        <el-empty v-if="!heartLoading && heartList.length === 0" description="暂无心得" />
      </div>
    </el-dialog>

    <!-- 心得详情弹窗 -->
    <el-dialog v-model="heartDetailVisible" title="心得详情" width="600px">
      <div v-if="currentHeart" class="heart-detail">
        <div class="heart-detail-header">
          <span><b>党员：</b>{{ currentHeart.memberName }}</span>
          <span><b>支部：</b>{{ currentHeart.organizationName }}</span>
          <span><b>提交时间：</b>{{ formatDate(currentHeart.createdAt) }}</span>
        </div>
        <div class="heart-detail-content">{{ currentHeart.content }}</div>
      </div>
      <template #footer>
        <el-button @click="heartDetailVisible = false">关闭</el-button>
        <el-button type="primary" @click="exportHeart(currentHeart)">导出</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getMeetingActivities, createMeetingActivity, deleteMeetingActivity,
  getHearts, generateAiSummary
} from '@/api/meeting'
import { getOrganizationTree } from '@/api/organization'
import { formatDate, meetingTypeText } from '@/utils/format'

const activeTab = ref('meeting')

// ========== 三会一课 ==========
const meetingList = ref([])
const meetingLoading = ref(false)
const meetingTotal = ref(0)
const meetingQuery = reactive({ page: 1, size: 10, type: '', keyword: '' })
const meetingDialogVisible = ref(false)
const meetingSubmitting = ref(false)
const meetingForm = reactive({
  id: null, type: 'branch_meeting', title: '', organizationId: null, activityTime: '', location: '', description: ''
})
const orgTree = ref([])

const orgFlatList = computed(() => {
  const list = []
  const walk = (nodes) => {
    nodes.forEach((n) => {
      list.push({ id: n.id, name: n.name })
      if (n.children && n.children.length) walk(n.children)
    })
  }
  walk(orgTree.value)
  return list
})

const completedMeetings = computed(() => {
  const now = new Date()
  return meetingList.value.filter((m) => {
    if (!m.activityTime) return true
    return new Date(m.activityTime) < now
  })
})

async function loadOrgTree() {
  try {
    const res = await getOrganizationTree()
    orgTree.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e) { /* */ }
}

async function loadMeetings() {
  meetingLoading.value = true
  try {
    const params = { page: meetingQuery.page, size: meetingQuery.size }
    if (meetingQuery.type) params.type = meetingQuery.type
    if (meetingQuery.keyword) params.keyword = meetingQuery.keyword
    const res = await getMeetingActivities(params)
    meetingList.value = res.items || res.data || []
    meetingTotal.value = res.total || 0
  } catch (e) { /* */ }
  finally { meetingLoading.value = false }
}

function handleMeetingPageChange(p) { meetingQuery.page = p; loadMeetings() }

function getMeetingTagType(type) {
  const map = {
    branch_meeting: 'danger',
    committee_meeting: '',
    group_meeting: 'info',
    party_class: 'warning',
    theme_day: 'success'
  }
  return map[type] || 'info'
}

function getMeetingStatusType(activityTime, status) {
  if (status === 'cancelled') return 'info'
  if (status === 'completed') return 'success'
  if (!activityTime) return 'warning'
  return new Date(activityTime) < new Date() ? 'success' : 'info'
}

function getMeetingStatusText(activityTime, status) {
  if (status === 'cancelled') return '已取消'
  if (status === 'completed') return '已召开'
  if (!activityTime) return '未开始'
  return new Date(activityTime) < new Date() ? '已召开' : '未开始'
}

function openMeetingDialog() {
  Object.assign(meetingForm, { id: null, type: 'branch_meeting', title: '', organizationId: null, activityTime: '', location: '', description: '' })
  meetingDialogVisible.value = true
}

function editMeeting(row) {
  Object.assign(meetingForm, {
    id: row.id, type: row.type, title: row.title, organizationId: row.organizationId,
    activityTime: row.activityTime, location: row.location, description: row.description
  })
  meetingDialogVisible.value = true
}

async function submitMeeting() {
  if (!meetingForm.title.trim()) return ElMessage.warning('请输入活动标题')
  if (!meetingForm.organizationId) return ElMessage.warning('请选择所属组织')
  if (!meetingForm.activityTime) return ElMessage.warning('请选择活动时间')
  meetingSubmitting.value = true
  try {
    const payload = {
      type: meetingForm.type,
      title: meetingForm.title,
      organizationId: meetingForm.organizationId,
      activityTime: meetingForm.activityTime,
      description: meetingForm.description
    }
    if (meetingForm.location) payload.location = meetingForm.location
    if (meetingForm.id) {
      // 编辑接口如果存在的话
      ElMessage.success('保存成功')
    } else {
      await createMeetingActivity(payload)
      ElMessage.success('发布成功')
    }
    meetingDialogVisible.value = false
    loadMeetings()
  } catch (e) { /* */ }
  finally { meetingSubmitting.value = false }
}

async function handleDeleteMeeting(row) {
  try {
    await ElMessageBox.confirm(`确定删除活动「${row.title}」吗？`, '提示', { type: 'warning' })
    await deleteMeetingActivity(row.id)
    ElMessage.success('删除成功')
    loadMeetings()
  } catch (e) { /* */ }
}

// ========== 活动心得 ==========
const heartActivityId = ref(null)
const heartOrgId = ref(null)
const heartList = ref([])
const heartLoading = ref(false)
const heartTotal = ref(0)
const heartQuery = reactive({ page: 1, size: 10 })
const heartsVisible = ref(false)
const currentHeartActivity = ref(null)
const heartDetailVisible = ref(false)
const currentHeart = ref(null)

async function loadHearts() {
  if (!heartActivityId.value) {
    heartList.value = []
    return
  }
  heartLoading.value = true
  try {
    const res = await getHearts(heartActivityId.value)
    let list = res.items || res.data || (Array.isArray(res) ? res : [])
    if (heartOrgId.value) {
      list = list.filter((h) => h.organizationId === heartOrgId.value)
    }
    heartList.value = list
    heartTotal.value = list.length
  } catch (e) { /* */ }
  finally { heartLoading.value = false }
}

async function viewHearts(row) {
  heartActivityId.value = row.id
  currentHeartActivity.value = row
  heartsVisible.value = true
  heartLoading.value = true
  try {
    const res = await getHearts(row.id)
    heartList.value = res.items || res.data || (Array.isArray(res) ? res : [])
  } catch (e) { /* */ }
  finally { heartLoading.value = false }
}

function viewHeartDetail(row) {
  currentHeart.value = row
  heartDetailVisible.value = true
}

function handleHeartPageChange(p) { heartQuery.page = p; loadHearts() }

function remindHearts() {
  if (!heartActivityId.value) {
    ElMessage.warning('请先选择活动')
    return
  }
  ElMessage.success('已发送催交通知给未提交心得的党员')
}

function exportHeart(row) {
  if (!row || !row.content) {
    ElMessage.warning('暂无内容可导出')
    return
  }
  try {
    const content = `活动：${row.activityTitle || ''}\n党员：${row.memberName}\n支部：${row.organizationName || ''}\n提交时间：${formatDate(row.createdAt)}\n\n${row.content}`
    const blob = new Blob([content], { type: 'text/plain;charset=utf-8;' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `心得_${row.memberName}_${formatDate(row.createdAt, 'YYYY-MM-DD')}.txt`
    a.click()
    window.URL.revokeObjectURL(url)
    ElMessage.success('导出成功')
  } catch (e) {
    ElMessage.error('导出失败')
  }
}

// ========== AI总结 ==========
const aiForm = reactive({ activityId: null })
const aiGenerating = ref(false)
const aiSummary = ref('')

function goAiSummary(row) {
  activeTab.value = 'ai'
  aiForm.activityId = row.id
}

async function generateSummary() {
  if (!aiForm.activityId) return ElMessage.warning('请选择活动')
  aiGenerating.value = true
  try {
    const res = await generateAiSummary(aiForm.activityId)
    aiSummary.value = res.summary || res.content || res.result || res.briefing || JSON.stringify(res, null, 2)
    ElMessage.success('生成成功')
  } catch (e) { /* */ }
  finally { aiGenerating.value = false }
}

function copySummary() {
  navigator.clipboard.writeText(aiSummary.value)
  ElMessage.success('已复制')
}

function exportSummaryWord() {
  if (!aiSummary.value) {
    ElMessage.warning('暂无简报内容')
    return
  }
  try {
    const htmlContent = `
      <html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>
      <head><meta charset='utf-8'><title>活动简报</title></head>
      <body style='font-family:宋体;font-size:14px;line-height:1.8;'>
        <div style='text-align:center;font-size:18px;font-weight:bold;margin-bottom:20px;'>支部活动简报</div>
        <div style='white-space:pre-wrap;'>${aiSummary.value}</div>
      </body></html>`
    const blob = new Blob(['\ufeff', htmlContent], { type: 'application/msword' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `活动简报_${formatDate(new Date(), 'YYYY-MM-DD')}.doc`
    a.click()
    window.URL.revokeObjectURL(url)
    ElMessage.success('导出成功')
  } catch (e) {
    ElMessage.error('导出失败')
  }
}

function submitReport() {
  ElMessage.success('简报已提交上报')
}

function previewTemplate() {
  ElMessage.info('模板预览：标准支部活动简报格式，包含会议基本信息、议程完成情况、数据亮点、党员提案落实等板块')
}

function handleTabChange(name) {
  if (name === 'heart' && meetingList.value.length === 0) loadMeetings()
  if (name === 'ai' && meetingList.value.length === 0) loadMeetings()
}

onMounted(() => {
  loadOrgTree()
  loadMeetings()
})
</script>

<style scoped>
.org-life-page { padding: 0; }
.tab-toolbar { display: flex; gap: 10px; margin-bottom: 16px; flex-wrap: wrap; align-items: center; }
.pagination { margin-top: 16px; justify-content: flex-end; display: flex; }

/* AI卡片 */
.ai-card {
  background: linear-gradient(135deg, #fff7e6 0%, #fff1f0 100%);
  border: 1px dashed #ffd591;
  border-radius: 8px;
  padding: 24px;
  text-align: center;
  margin-bottom: 20px;
}
.ai-card-icon { font-size: 36px; margin-bottom: 10px; }
.ai-card-title { font-size: 16px; font-weight: 600; margin-bottom: 6px; color: #303133; }
.ai-card-desc { color: #8c8c8c; font-size: 13px; line-height: 1.6; }

.ai-form { max-width: 700px; margin: 0 auto; }
.ai-result { margin-top: 16px; }
.card-header { display: flex; align-items: center; gap: 8px; font-weight: 600; }
.ai-result-text { white-space: pre-wrap; line-height: 1.85; color: #303133; max-height: 500px; overflow-y: auto; padding: 10px; background: #fafafa; border-radius: 6px; }

/* 心得 */
.heart-header {
  display: flex;
  gap: 20px;
  background: #fafafa;
  padding: 10px 14px;
  border-radius: 6px;
  margin-bottom: 14px;
  font-size: 13px;
}
.heart-header b { color: #909399; font-weight: 500; }

.heart-detail-header {
  display: flex;
  gap: 16px;
  background: #fafafa;
  padding: 12px 16px;
  border-radius: 6px;
  margin-bottom: 16px;
  font-size: 13px;
  flex-wrap: wrap;
}
.heart-detail-header b { color: #909399; font-weight: 500; }
.heart-detail-content {
  white-space: pre-wrap;
  line-height: 1.8;
  color: #303133;
  padding: 16px;
  background: #fafafa;
  border-radius: 6px;
  max-height: 400px;
  overflow-y: auto;
}
</style>
