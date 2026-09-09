<template>
  <el-container class="main-layout" :class="{ 'mobile-view': isMobileView }">
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
        <el-dropdown trigger="click" @command="handleUserCommand">
          <div class="user-info">
            <el-avatar :size="36" style="background: rgba(255,255,255,0.25); color: #fff; font-weight: 600">
              {{ avatarChar }}
            </el-avatar>
            <span class="user-name">{{ displayName }}</span>
            <el-icon class="arrow-icon"><ArrowDown /></el-icon>
          </div>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="profile">个人中心</el-dropdown-item>
              <el-dropdown-item command="toggleView">{{ isMobileView ? '电脑端' : '手机端' }}</el-dropdown-item>
              <el-dropdown-item command="logout" divided>退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
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


    <!-- 移动端底部导航 -->
    <div class="mobile-bottom-nav">
      <div class="nav-item" :class="{active: activeMenu === '/home'}" @click="router.push('/home')">
        <el-icon><House /></el-icon>
        <span>首页</span>
      </div>
      <div class="nav-item" :class="{active: activeMenu === '/learning'}" @click="router.push('/learning')">
        <el-icon><Reading /></el-icon>
        <span>学习</span>
      </div>
      <div class="nav-item" :class="{active: activeMenu === '/exam'}" @click="router.push('/exam')">
        <el-icon><Document /></el-icon>
        <span>考试</span>
      </div>
      <div class="nav-item" :class="{active: activeMenu === '/pair-help'}" @click="router.push('/pair-help')">
        <el-icon><Connection /></el-icon>
        <span>互助</span>
      </div>
      <div class="nav-item" :class="{active: activeMenu === '/profile'}" @click="router.push('/profile')">
        <el-icon><User /></el-icon>
        <span>我的</span>
      </div>
    </div>
    <!-- AI 悬浮球 + AI 功能中心面板 -->
    <AIFloatingButton @click="aiVisible = true" />
    <AIChatPanel v-show="aiVisible" ref="aiPanelRef" @close="aiVisible = false" />
  </el-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessageBox, ElMessage } from 'element-plus'
import { House, Reading, Document, User, Trophy, Connection, Guide, ArrowDown } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/user'
import { getAvatarChar } from '@/utils/format'
import AIFloatingButton from '@/components/AIFloatingButton.vue'
import AIChatPanel from '@/components/AIChatPanel.vue'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()
const aiVisible = ref(false)
const aiPanelRef = ref(null)

// 全局提供打开AI面板的方法
provide('openAIPanel', (panelKey) => {
  aiVisible.value = true
  nextTick(() => {
    aiPanelRef.value?.open(panelKey)
  })
})
const isMobileView = ref(localStorage.getItem('memberViewMode') === 'mobile')

function toggleView() {
  isMobileView.value = !isMobileView.value
  localStorage.setItem('memberViewMode', isMobileView.value ? 'mobile' : 'desktop')
  ElMessage.success(isMobileView.value ? '已切换到手机视图' : '已切换到电脑视图')
}

function handleUserCommand(command) {
  if (command === 'profile') {
    router.push('/profile')
  } else if (command === 'toggleView') {
    toggleView()
  } else if (command === 'logout') {
    handleLogout()
  }
}

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

/* ========== 移动端响应式适配 ========== */
@media (max-width: 768px) {
  .top-header {
    padding: 0 16px !important;
    height: 48px !important;
  }
  .logo-text {
    font-size: 15px;
  }
  .user-name, .divider, .logout {
    display: none;
  }
  .header-right {
    gap: 8px;
  }
  .body-container {
    margin-top: 48px;
    height: calc(100vh - 48px);
  }
  /* 隐藏左侧侧边栏 */
  .sidebar {
    display: none;
  }
  /* 内容区全屏 */
  .content-area {
    margin-left: 0 !important;
    padding: 12px 14px 80px 14px !important;
    min-height: calc(100vh - 48px);
  }
  /* 显示底部导航 */
  .mobile-bottom-nav {
    display: flex !important;
  }
}

/* 底部导航栏（默认隐藏，移动端显示） */
.mobile-bottom-nav {
  display: none;
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 60px;
  background: #fff;
  border-top: 1px solid #eee;
  box-shadow: 0 -2px 10px rgba(0,0,0,0.08);
  z-index: 99;
  align-items: center;
  justify-content: space-around;
  padding-bottom: env(safe-area-inset-bottom);
}
.mobile-bottom-nav .nav-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 3px;
  color: #999;
  font-size: 11px;
  cursor: pointer;
  height: 100%;
  transition: color 0.15s;
}
.mobile-bottom-nav .nav-item .el-icon {
  font-size: 22px;
}
.mobile-bottom-nav .nav-item.active {
  color: var(--red);
  font-weight: 600;
}
.mobile-bottom-nav .nav-item:active {
  opacity: 0.7;
}

/* ========== 手动手机视图 ========== */
.mobile-view .body-container {
  max-width: 375px;
  width: 375px;
  margin: 56px auto 0 auto !important;
  height: calc(100vh - 56px) !important;
  min-height: calc(100vh - 56px) !important;
  box-shadow: none;
  border: none;
}
.mobile-view .top-header {
  width: 375px !important;
  max-width: 375px !important;
  left: 50% !important;
  right: auto !important;
  transform: translateX(-50%) !important;
  padding: 0 16px !important;
  box-shadow: none !important;
  border-bottom: none !important;
}
.mobile-view .sidebar {
  display: none;
}
.mobile-view .content-area {
  margin-left: 0 !important;
  padding: 12px 14px 80px 14px !important;
  min-height: calc(100vh - 56px) !important;
  height: 100% !important;
}
.mobile-view .mobile-bottom-nav {
  display: flex !important;
  width: 375px;
  left: 50% !important;
  right: auto !important;
  transform: translateX(-50%) !important;
}
.mobile-view .user-name {
  display: none;
}
/* 手机视图下通用：多列grid/flex变单列，避免溢出 */
.mobile-view :deep(.top-row),
.mobile-view :deep(.stats-row),
.mobile-view :deep(.bottom-row),
.mobile-view :deep(.ai-cards),
.mobile-view :deep(.card-grid),
.mobile-view :deep(.content-grid) {
  grid-template-columns: 1fr !important;
}
.mobile-view :deep(.flex-row) {
  flex-direction: column !important;
}
.mobile-view :deep(.el-col) {
  width: 100% !important;
}
/* AI悬浮按钮：限制在手机框内，缩小 */
.mobile-view :deep(.ai-floating-button) {
  right: calc(50% - 187.5px + 12px) !important;
  bottom: 72px !important;
  width: 52px !important;
  height: 52px !important;
}
.mobile-view :deep(.ai-floating-button .ai-glow) {
  width: 52px !important;
  height: 52px !important;
}
.mobile-view :deep(.ai-floating-button .ai-ring) {
  width: 52px !important;
  height: 52px !important;
}
.mobile-view :deep(.ai-floating-button .ai-button-core) {
  width: 46px !important;
  height: 46px !important;
}
.mobile-view :deep(.ai-floating-button .ai-text) {
  font-size: 14px !important;
}
/* 个人中心卡片：手机视图下纵向布局，避免进度条溢出 */
.mobile-view :deep(.profile-card) {
  flex-direction: column !important;
  align-items: stretch !important;
  gap: 16px !important;
  padding: 16px !important;
}
.mobile-view :deep(.profile-stats) {
  margin-left: 0 !important;
  justify-content: space-around !important;
  gap: 16px !important;
}
.mobile-view :deep(.profile-progress) {
  width: 100% !important;
  max-width: 120px !important;
  margin: 4px auto 0 auto !important;
}
.arrow-icon {
  color: #fff;
  font-size: 14px;
  margin-left: 4px;
}
</style>
