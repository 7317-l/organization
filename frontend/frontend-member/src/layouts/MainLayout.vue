<template>
  <el-container class="main-layout">
    <!-- 顶部深红 Header -->
    <el-header class="top-header">
      <div class="header-left">
        <div class="logo">
          <svg viewBox="0 0 100 100" fill="currentColor" class="logo-svg">
            <circle cx="50" cy="50" r="45" fill="none" stroke="currentColor" stroke-width="2" />
            <path d="M50 20 L54 36 L70 36 L57 46 L62 62 L50 52 L38 62 L43 46 L30 36 L46 36 Z" />
          </svg>
          <span class="logo-text">党建 · 党员学习平台</span>
        </div>
      </div>
      <div class="header-right">
        <div class="user-info" @click="handleProfile">
          <el-avatar :size="36" style="background: rgba(255,255,255,0.25); color: #fff; font-weight: 600">
            {{ avatarChar }}
          </el-avatar>
          <span class="user-name">{{ displayName }}</span>
        </div>
        <span class="divider">|</span>
        <span class="logout" @click="handleLogout">退出登录</span>
      </div>
    </el-header>

    <el-container class="body-container">
      <!-- 左侧白色 Sidebar -->
      <el-aside width="220px" class="sidebar">
        <el-menu
          :default-active="activeMenu"
          class="sidebar-menu"
          @select="handleMenuSelect"
        >
          <el-menu-item index="/home">
            <el-icon><House /></el-icon>
            <span>首页</span>
          </el-menu-item>
          <el-menu-item index="/learning">
            <el-icon><Reading /></el-icon>
            <span>学习中心</span>
          </el-menu-item>
          <el-menu-item index="/exam">
            <el-icon><Document /></el-icon>
            <span>考试中心</span>
          </el-menu-item>
          <el-menu-item index="/pair-help">
            <el-icon><Connection /></el-icon>
            <span>结对互助</span>
          </el-menu-item>
          <el-menu-item index="/profile">
            <el-icon><User /></el-icon>
            <span>我的</span>
          </el-menu-item>
        </el-menu>
      </el-aside>

      <!-- 右侧内容区 -->
      <el-main class="content-area">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </el-main>
    </el-container>

    <!-- AI 悬浮球 + 完整 AI 面板（接入 ai-module） -->
    <AIFloatingButton @click="aiVisible = true" />
    <AIChatPanel
      v-if="aiVisible"
      :question-data="aiDataStore.currentQuestion"
      :answer-history="aiDataStore.answerHistory"
      @close="aiVisible = false"
    />
  </el-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessageBox, ElMessage } from 'element-plus'
import { House, Reading, Document, User, Trophy, Connection, Guide } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/user'
import { getAvatarChar } from '@/utils/format'
import { useAiDataStore } from '@/stores/aiData'
import AIFloatingButton from '@/components/AIFloatingButton.vue'
import AIChatPanel from '@/components/AIChatPanel.vue'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()
const aiDataStore = useAiDataStore()
const aiVisible = ref(false)

const activeMenu = computed(() => {
  const path = route.path
  if (path.startsWith('/content') || path.startsWith('/ai-chat')) return '/learning'
  if (path.startsWith('/quiz') || path.startsWith('/practice')) return '/exam'
  if (path.startsWith('/report')) return '/profile'
  return path
})

const displayName = computed(() => userStore.userName || '用户')
const avatarChar = computed(() => getAvatarChar(displayName.value))

function handleMenuSelect(index) {
  router.push(index)
}

function handleProfile() {
  router.push('/profile')
}

async function handleLogout() {
  try {
    await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    userStore.logout()
    ElMessage.success('已退出登录')
    router.push('/login')
  } catch {
    // 用户取消
  }
}

onMounted(() => {
  if (userStore.token && !userStore.userInfo) {
    userStore.fetchUserInfo().catch(() => {})
  }
})
</script>

<style scoped>
.main-layout {
  height: 100vh;
}

/* 顶部导航栏 */
.top-header {
  height: 56px !important;
  background: var(--red);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 32px !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 100;
}

.header-left {
  display: flex;
  align-items: center;
}

.logo {
  display: flex;
  align-items: center;
  gap: 12px;
  color: #fff;
}

.logo-svg {
  width: 32px;
  height: 32px;
  color: var(--gold-l);
}

.logo-text {
  font-size: 18px;
  font-weight: 600;
  letter-spacing: 1px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
  color: #fff;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.user-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
}

.user-name {
  font-size: 14px;
}

.divider {
  color: rgba(255, 255, 255, 0.4);
}

.logout {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.8);
  cursor: pointer;
  transition: color 0.15s;
}

.logout:hover {
  color: #fff;
}

/* 主体 */
.body-container {
  margin-top: 56px;
  height: calc(100vh - 56px);
}

/* 侧边栏 */
.sidebar {
  background: transparent;
  border-right: none;
  position: fixed;
  top: 56px;
  bottom: 0;
  left: 0;
  overflow-y: auto;
  padding: 16px 12px;
}

.sidebar-menu {
  border-right: none;
  padding: 0;
  background: transparent;
}

.sidebar-menu :deep(.el-menu-item) {
  height: 48px;
  line-height: 48px;
  border-radius: 10px;
  margin-bottom: 10px;
  font-size: 14px;
  font-weight: 500;
  background-color: #ffffff !important;
  color: #333333 !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.10);
}

/* 选中项：白底 + 深红加粗文字 + 左侧红条，保证文字清晰可读 */
.sidebar-menu :deep(.el-menu-item.is-active) {
  background-color: #ffffff !important;
  color: var(--red) !important;
  font-weight: 700;
  border-left: 3px solid var(--red);
  box-shadow: 0 2px 10px rgba(200, 22, 29, 0.25);
}

.sidebar-menu :deep(.el-menu-item:hover) {
  background-color: #ffffff !important;
  color: var(--red);
}

/* 内容区 */
.content-area {
  margin-left: 220px;
  padding: 24px 32px;
  min-height: calc(100vh - 56px);
  overflow-y: auto;
}

/* 过渡动画 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
