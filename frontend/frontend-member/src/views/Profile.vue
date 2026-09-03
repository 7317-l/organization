<template>
  <div class="profile-page">
    <div class="page-header">
      <div class="page-title">我的</div>
    </div>

    <!-- 个人信息卡片 -->
    <div class="profile-card" v-loading="userLoading">
      <div class="profile-left">
        <div class="profile-avatar">{{ avatarChar }}</div>
        <div class="profile-info">
          <h3>{{ userName }}</h3>
          <el-tag type="danger" effect="light" size="small">
            {{ userBranch }} · {{ userRole || '党员' }}
          </el-tag>
          <div class="profile-meta" v-if="userInfo">
            <span v-if="userInfo.phone">手机号：{{ userInfo.phone }}</span>
            <span v-if="userInfo.email"> · {{ userInfo.email }}</span>
          </div>
        </div>
      </div>
      <div class="profile-stats">
        <div class="profile-stat">
          <div class="profile-stat-value gold">{{ overview.totalPoints || 0 }}</div>
          <div class="profile-stat-label">总积分（分）</div>
        </div>
        <div class="profile-stat clickable" @click="goReport" title="查看完整 AI 学习报告">
          <div class="profile-stat-value red">{{ overview.learningProgress || 0 }}%</div>
          <div class="profile-stat-label">学习进度</div>
          <div class="profile-progress">
            <el-progress :percentage="overview.learningProgress || 0" :show-text="false" :stroke-width="6" color="#C8161D" />
          </div>
        </div>
      </div>
    </div>

    <!-- 功能列表 -->
    <div class="me-list">
      <div class="me-item" @click="goReport">
        <div class="me-icon r">
          <el-icon :size="18"><DataAnalysis /></el-icon>
        </div>
        <div class="me-label">完整 AI 学习报告</div>
        <span class="me-arrow">›</span>
      </div>

      <div class="me-item" @click="activePanel = 'checkin'">
        <div class="me-icon b">
          <el-icon :size="18"><Calendar /></el-icon>
        </div>
        <div class="me-label">我的打卡记录</div>
        <span class="me-arrow">›</span>
      </div>

      <div class="me-item" @click="activePanel = 'points'">
        <div class="me-icon g">
          <el-icon :size="18"><Star /></el-icon>
        </div>
        <div class="me-label">积分明细</div>
        <span class="me-arrow">›</span>
      </div>

      <div class="me-item" @click="activePanel = 'notifications'">
        <div class="me-icon o">
          <el-icon :size="18"><Bell /></el-icon>
        </div>
        <div class="me-label">
          <el-badge v-if="unreadCount > 0" :value="unreadCount" class="me-badge" />
          消息通知
        </div>
        <span class="me-arrow">›</span>
      </div>

      <div class="me-item" @click="activePanel = 'settings'">
        <div class="me-icon c">
          <el-icon :size="18"><Setting /></el-icon>
        </div>
        <div class="me-label">账号设置</div>
        <span class="me-arrow">›</span>
      </div>
    </div>

    <!-- 打卡记录弹窗 -->
    <el-dialog v-model="checkinDialogVisible" title="我的打卡记录" width="700px">
      <div v-loading="checkinLoading">
        <el-table :data="checkinRecords" stripe style="width: 100%">
          <el-table-column prop="date" label="打卡日期" width="180">
            <template #default="{ row }">{{ formatDateTime(row.checkinTime || row.date || row.createdAt) }}</template>
          </el-table-column>
          <el-table-column prop="type" label="类型" width="120">
            <template #default="{ row }">
              <el-tag size="small" type="success" effect="light">{{ row.type || '日常打卡' }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="points" label="获得积分" width="120">
            <template #default="{ row }">+{{ row.points || row.score || 0 }}</template>
          </el-table-column>
          <el-table-column prop="remark" label="备注">
            <template #default="{ row }">{{ row.remark || row.note || '-' }}</template>
          </el-table-column>
        </el-table>
        <el-empty v-if="checkinRecords.length === 0" description="暂无打卡记录" :image-size="80" />
        <div class="dialog-pagination" v-if="checkinTotal > 0">
          <el-pagination
            v-model:current-page="checkinPage"
            v-model:page-size="checkinSize"
            :total="checkinTotal"
            layout="prev, pager, next"
            background
            @current-change="loadCheckins"
          />
        </div>
      </div>
    </el-dialog>

    <!-- 积分明细弹窗 -->
    <el-dialog v-model="pointsDialogVisible" title="积分明细" width="700px">
      <div v-loading="pointsLoading">
        <el-table :data="pointsRecords" stripe style="width: 100%">
          <el-table-column prop="time" label="时间" width="180">
            <template #default="{ row }">{{ formatDateTime(row.earnedAt || row.createdAt || row.time || row.date) }}</template>
          </el-table-column>
          <el-table-column prop="type" label="来源" width="140">
            <template #default="{ row }">
              <el-tag size="small" :type="row.points >= 0 ? 'success' : 'danger'" effect="light">
                {{ pointSourceText(row.sourceType ?? row.sourceTypeName) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="points" label="积分变动" width="120">
            <template #default="{ row }">
              <span :style="{ color: row.points >= 0 ? 'var(--green)' : 'var(--red)' }">
                {{ row.points >= 0 ? '+' : '' }}{{ row.points ?? row.score ?? 0 }}
              </span>
            </template>
          </el-table-column>
          <el-table-column prop="description" label="说明">
            <template #default="{ row }">{{ row.description || row.remark || pointSourceText(row.sourceType ?? row.sourceTypeName) }}</template>
          </el-table-column>
        </el-table>
        <el-empty v-if="pointsRecords.length === 0" description="暂无积分记录" :image-size="80" />
        <div class="dialog-pagination" v-if="pointsTotal > 0">
          <el-pagination
            v-model:current-page="pointsPage"
            v-model:page-size="pointsSize"
            :total="pointsTotal"
            layout="prev, pager, next"
            background
            @current-change="loadPoints"
          />
        </div>
      </div>
    </el-dialog>

    <!-- 消息通知弹窗 -->
    <el-dialog v-model="notificationsDialogVisible" title="消息通知" width="700px">
      <div v-loading="notificationsLoading">
        <div class="notification-list" v-if="notifications.length > 0">
          <div
            v-for="item in notifications"
            :key="item.id"
            class="notification-item"
            :class="{ unread: !item.isRead }"
          >
            <div class="notification-icon">
              <el-icon :size="16"><Bell /></el-icon>
            </div>
            <div class="notification-content">
              <div class="notification-title">{{ item.title }}</div>
              <div class="notification-body">{{ item.content || item.message }}</div>
              <div class="notification-time">{{ formatDateTime(item.createdAt || item.time) }}</div>
            </div>
            <span v-if="!item.isRead" class="unread-dot"></span>
          </div>
        </div>
        <el-empty v-else description="暂无未读通知" :image-size="80" />
      </div>
    </el-dialog>

    <!-- 账号设置弹窗 -->
    <el-dialog v-model="settingsDialogVisible" title="账号设置" width="500px">
      <el-form label-width="80px" class="settings-form">
        <el-form-item label="姓名">
          <el-input :model-value="userName" disabled />
        </el-form-item>
        <el-form-item label="手机号">
          <el-input :model-value="userInfo?.phone" disabled />
        </el-form-item>
        <el-form-item label="所属支部">
          <el-input :model-value="userBranch" disabled />
        </el-form-item>
        <el-form-item label="角色">
          <el-input :model-value="userRole || '党员'" disabled />
        </el-form-item>
        <el-form-item label="修改密码">
          <el-button type="primary" plain @click="ElMessage.info('密码修改功能开发中')">修改密码</el-button>
        </el-form-item>
      </el-form>
      <div class="settings-tip">
        <el-icon><InfoFilled /></el-icon>
        个人信息如需修改，请联系支部管理员
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { DataAnalysis, Calendar, Star, Bell, Setting, InfoFilled } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/user'
import { getOverview } from '@/api/mobile'
import { getMyCheckIns } from '@/api/checkin'
import { getPointsRecords } from '@/api/points'
import { getUnreadNotifications, markNotificationRead, markAllNotificationsRead } from '@/api/notification'
import { getAvatarChar, formatDateTime, pointSourceText } from '@/utils/format'

const router = useRouter()
const userStore = useUserStore()

const userLoading = ref(false)
const overview = ref({ totalPoints: 0, learningProgress: 0 })

const userInfo = computed(() => userStore.userInfo)
const userName = computed(() => userStore.userName || '用户')
const userBranch = computed(() => userStore.userBranch || '')
const userRole = computed(() => userStore.userRole || '')
const avatarChar = computed(() => getAvatarChar(userName.value))

// 面板控制
const activePanel = ref('')
const checkinDialogVisible = ref(false)
const pointsDialogVisible = ref(false)
const notificationsDialogVisible = ref(false)
const settingsDialogVisible = ref(false)

// 打卡记录
const checkinRecords = ref([])
const checkinLoading = ref(false)
const checkinPage = ref(1)
const checkinSize = ref(10)
const checkinTotal = ref(0)

// 积分记录
const pointsRecords = ref([])
const pointsLoading = ref(false)
const pointsPage = ref(1)
const pointsSize = ref(10)
const pointsTotal = ref(0)

// 通知
const notifications = ref([])
const notificationsLoading = ref(false)
const unreadCount = ref(0)

watch(activePanel, (val) => {
  if (val === 'checkin') {
    checkinDialogVisible.value = true
    loadCheckins()
  } else if (val === 'points') {
    pointsDialogVisible.value = true
    loadPoints()
  } else if (val === 'notifications') {
    notificationsDialogVisible.value = true
    loadNotifications()
  } else if (val === 'settings') {
    settingsDialogVisible.value = true
  }
  activePanel.value = ''
})

async function loadUserInfo() {
  userLoading.value = true
  try {
    if (!userStore.userInfo) {
      await userStore.fetchUserInfo()
    }
  } catch {
    // 错误已由拦截器处理
  } finally {
    userLoading.value = false
  }
}

async function loadOverviewData() {
  try {
    const data = await getOverview()
    overview.value = data || {}
  } catch {
    // 错误已由拦截器处理
  }
}

async function loadCheckins() {
  checkinLoading.value = true
  try {
    const data = await getMyCheckIns({ page: checkinPage.value, size: checkinSize.value })
    checkinRecords.value = data?.items || data || []
    checkinTotal.value = data?.total || checkinRecords.value.length
  } catch {
    // 错误已由拦截器处理
  } finally {
    checkinLoading.value = false
  }
}

async function loadPoints() {
  pointsLoading.value = true
  try {
    const data = await getPointsRecords({ page: pointsPage.value, size: pointsSize.value })
    pointsRecords.value = data?.items || data || []
    pointsTotal.value = data?.total || pointsRecords.value.length
  } catch {
    // 错误已由拦截器处理
  } finally {
    pointsLoading.value = false
  }
}

async function loadNotifications() {
  notificationsLoading.value = true
  try {
    const data = await getUnreadNotifications({ page: 1, size: 50 })
    notifications.value = data?.items || data || []
    unreadCount.value = notifications.value.length
  } catch {
    // 错误已由拦截器处理
  } finally {
    notificationsLoading.value = false
  }
}

async function loadUnreadCount() {
  try {
    const data = await getUnreadNotifications({ page: 1, size: 1 })
    unreadCount.value = data?.total || (data?.items || data || []).length
  } catch {
    // 静默处理
  }
}

function goReport() {
  router.push('/report')
}

onMounted(() => {
  loadUserInfo()
  loadOverviewData()
  loadUnreadCount()
})
</script>

<style scoped>
.profile-page {
  padding-bottom: 24px;
}

/* 个人信息卡片 */
.profile-card {
  background: var(--card);
  border-radius: var(--r10);
  padding: 24px;
  box-shadow: var(--sh);
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  gap: 24px;
}

.profile-left {
  display: flex;
  align-items: center;
  gap: 20px;
}

.profile-avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--red), var(--red-d));
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
  font-weight: 600;
  color: #fff;
  flex-shrink: 0;
}

.profile-info h3 {
  font-size: 20px;
  font-weight: 600;
  margin-bottom: 6px;
}

.profile-meta {
  font-size: 12px;
  color: var(--t3);
  margin-top: 8px;
}

.profile-stats {
  display: flex;
  gap: 40px;
  margin-left: auto;
}

.profile-stat.clickable { cursor: pointer; }
.profile-stat {
  text-align: center;
}

.profile-stat-value {
  font-size: 24px;
  font-weight: 700;
}

.profile-stat-value.gold {
  color: var(--gold);
}

.profile-stat-value.red {
  color: var(--red);
}

.profile-stat-label {
  font-size: 12px;
  color: var(--t3);
  margin-top: 4px;
}

.profile-progress {
  width: 160px;
  margin-top: 4px;
}

/* 功能列表 */
.me-list {
  background: var(--card);
  border-radius: var(--r10);
  box-shadow: var(--sh);
  overflow: hidden;
}

.me-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 18px 24px;
  cursor: pointer;
  transition: background 0.15s;
  border-bottom: 1px solid var(--bd);
}

.me-item:last-child {
  border-bottom: none;
}

.me-item:hover {
  background: var(--bg);
}

.me-icon {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.me-icon.r {
  background: var(--red-10);
  color: var(--red);
}

.me-icon.b {
  background: rgba(30, 100, 200, 0.1);
  color: var(--blue);
}

.me-icon.g {
  background: var(--gold-50);
  color: #b07e0a;
}

.me-icon.o {
  background: rgba(230, 126, 34, 0.1);
  color: var(--orange);
}

.me-icon.c {
  background: rgba(46, 139, 87, 0.1);
  color: var(--green);
}

.me-label {
  flex: 1;
  font-size: 14px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.me-badge {
  margin-right: 0;
}

.me-arrow {
  color: var(--t3);
  font-size: 20px;
}

/* 弹窗 */
.dialog-pagination {
  display: flex;
  justify-content: center;
  margin-top: 16px;
}

/* 通知列表 */
.notification-list {
  max-height: 400px;
  overflow-y: auto;
}

.notification-item {
  display: flex;
  gap: 12px;
  padding: 16px;
  border-bottom: 1px solid var(--bd);
  position: relative;
}

.notification-item:last-child {
  border-bottom: none;
}

.notification-item.unread {
  background: rgba(200, 22, 29, 0.03);
}

.notification-icon {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--red-10);
  color: var(--red);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.notification-content {
  flex: 1;
  min-width: 0;
}

.notification-title {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 4px;
}

.notification-body {
  font-size: 13px;
  color: var(--t2);
  line-height: 1.5;
  margin-bottom: 4px;
}

.notification-time {
  font-size: 11px;
  color: var(--t3);
}

.unread-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--red);
  flex-shrink: 0;
  margin-top: 6px;
}

/* 设置 */
.settings-form {
  margin-bottom: 16px;
}

.settings-tip {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--t3);
  padding: 12px;
  background: var(--bg);
  border-radius: 8px;
}
</style>
