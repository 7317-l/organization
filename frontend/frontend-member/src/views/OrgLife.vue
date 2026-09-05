<template>
  <div class="org-life-page">
    <div class="page-header">
      <h2>组织生活</h2>
    </div>

    <!-- 活动列表 -->
    <div class="activity-list" v-loading="loading">
      <div v-for="activity in activities" :key="activity.id" class="activity-card" @click="viewDetail(activity)">
        <div class="activity-title">{{ activity.title || activity.activityTitle }}</div>
        <div class="activity-meta">
          <span>📅 {{ formatDate(activity.startTime || activity.activityTime || activity.createdAt) }}</span>
          <span>📍 {{ activity.location || activity.address || '待定' }}</span>
        </div>
        <div class="activity-status">
          <el-tag :type="getStatusType(activity)" size="small">{{ getStatusText(activity) }}</el-tag>
        </div>
        <div class="activity-actions" @click.stop>
          <el-button v-if="!activity.hasSignedUp" type="primary" size="small" @click="handleSignUp(activity)">报名</el-button>
          <el-button v-else-if="!activity.hasCheckedIn" type="success" size="small" @click="handleCheckIn(activity)">签到</el-button>
          <el-button v-if="activity.hasCheckedIn" type="warning" size="small" @click="openHeartDialog(activity)">提交心得</el-button>
        </div>
      </div>
      <el-empty v-if="activities.length === 0 && !loading" description="暂无组织生活活动" />
    </div>

    <!-- 活动详情弹窗 -->
    <el-dialog v-model="detailVisible" title="活动详情" width="500px">
      <div v-if="currentActivity">
        <h3>{{ currentActivity.title || currentActivity.activityTitle }}</h3>
        <p><strong>时间：</strong>{{ formatDate(currentActivity.startTime || currentActivity.activityTime || currentActivity.createdAt) }}</p>
        <p><strong>地点：</strong>{{ currentActivity.location || currentActivity.address || '待定' }}</p>
        <p><strong>简介：</strong>{{ currentActivity.description || currentActivity.content || '暂无简介' }}</p>
      </div>
    </el-dialog>

    <!-- 提交心得弹窗 -->
    <el-dialog v-model="heartVisible" title="提交学习心得" width="500px">
      <el-input v-model="heartContent" type="textarea" :rows="6" placeholder="请输入您的学习心得..." />
      <template #footer>
        <el-button @click="heartVisible = false">取消</el-button>
        <el-button type="primary" :loading="heartSubmitting" @click="submitHeart">提交</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import request from '@/api/request'

const loading = ref(false)
const activities = ref([])
const detailVisible = ref(false)
const heartVisible = ref(false)
const heartSubmitting = ref(false)
const currentActivity = ref(null)
const heartContent = ref('')

function formatDate(date) {
  if (!date) return '待定'
  return new Date(date).toLocaleString('zh-CN')
}

function getStatusType(activity) {
  if (activity.hasCheckedIn) return 'success'
  if (activity.hasSignedUp) return 'warning'
  return 'info'
}

function getStatusText(activity) {
  if (activity.hasCheckedIn) return '已签到'
  if (activity.hasSignedUp) return '已报名'
  return '未报名'
}

async function loadActivities() {
  loading.value = true
  try {
    const res = await request.get('/meeting-activities', { params: { page: 1, size: 50 } })
    const list = Array.isArray(res) ? res : (res.items || res.data || [])
    activities.value = list.map(item => ({
      ...item,
      hasSignedUp: false,
      hasCheckedIn: false
    }))
  } catch (e) {
    ElMessage.error('加载活动列表失败')
  } finally {
    loading.value = false
  }
}

function viewDetail(activity) {
  currentActivity.value = activity
  detailVisible.value = true
}

async function handleSignUp(activity) {
  try {
    await request.post(`/meeting-activities/${activity.id}/signup`)
    ElMessage.success('报名成功')
    activity.hasSignedUp = true
  } catch (e) {
    ElMessage.error('报名失败')
  }
}

async function handleCheckIn(activity) {
  try {
    await request.post(`/meeting-activities/${activity.id}/checkin`)
    ElMessage.success('签到成功')
    activity.hasCheckedIn = true
  } catch (e) {
    ElMessage.error('签到失败')
  }
}

function openHeartDialog(activity) {
  currentActivity.value = activity
  heartContent.value = ''
  heartVisible.value = true
}

async function submitHeart() {
  if (!heartContent.value.trim()) return ElMessage.warning('请输入心得内容')
  heartSubmitting.value = true
  try {
    await request.post('/meeting-activities/hearts', {
      meetingActivityId: currentActivity.value.id,
      content: heartContent.value
    })
    ElMessage.success('心得提交成功')
    heartVisible.value = false
  } catch (e) {
    ElMessage.error('提交失败')
  } finally {
    heartSubmitting.value = false
  }
}

onMounted(() => {
  loadActivities()
})
</script>

<style scoped>
.org-life-page { padding: 16px; }
.page-header { margin-bottom: 16px; }
.page-header h2 { margin: 0; font-size: 20px; }
.activity-list { display: flex; flex-direction: column; gap: 12px; }
.activity-card { background: #fff; border-radius: 8px; padding: 16px; box-shadow: 0 2px 8px rgba(0,0,0,0.08); cursor: pointer; }
.activity-title { font-size: 16px; font-weight: bold; margin-bottom: 8px; }
.activity-meta { display: flex; gap: 16px; color: #666; font-size: 14px; margin-bottom: 8px; }
.activity-status { margin-bottom: 8px; }
.activity-actions { display: flex; gap: 8px; }
</style>
