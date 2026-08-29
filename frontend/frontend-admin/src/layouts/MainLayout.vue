<template>
  <el-container class="main-layout">
    <el-header class="app-header">
      <div class="header-left">
        <el-icon :size="28" color="#fff"><Flag /></el-icon>
        <span class="app-title">党建 · 管理后台</span>
      </div>
      <div class="header-right">
        <el-dropdown @command="handleCommand">
          <span class="user-info">
            <el-avatar :size="32" style="background:#fff;color:#C8161D">
              {{ userStore.userInfo?.name?.charAt(0) || '党' }}
            </el-avatar>
            <span class="user-name">{{ userStore.userInfo?.name || '管理员' }}</span>
            <el-icon><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="logout">退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </el-header>

    <el-container>
      <el-aside width="220px" class="app-aside">
        <el-menu
          :default-active="activeMenu"
          router
          class="side-menu"
          text-color="#333333"
          active-text-color="#C8161D"
        >
          <el-menu-item index="/dashboard">
            <el-icon><DataAnalysis /></el-icon>
            <span>工作台</span>
          </el-menu-item>
          <el-menu-item index="/organization">
            <el-icon><User /></el-icon>
            <span>组织人员</span>
          </el-menu-item>
          <el-menu-item index="/learning-content">
            <el-icon><Reading /></el-icon>
            <span>学习内容</span>
          </el-menu-item>
          <el-menu-item index="/exam-management">
            <el-icon><Document /></el-icon>
            <span>题库测验</span>
          </el-menu-item>
          <el-menu-item index="/org-life">
            <el-icon><Calendar /></el-icon>
            <span>组织生活</span>
          </el-menu-item>
          <el-menu-item index="/data-analysis">
            <el-icon><PieChart /></el-icon>
            <span>数据智能分析</span>
          </el-menu-item>
        </el-menu>
      </el-aside>

      <el-main class="app-main">
        <router-view />
      </el-main>
    </el-container>
    </el-container>

    <!-- 管理端 AI 悬浮助手 -->
    <AdminAIPanel />
  </el-container>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import AdminAIPanel from '@/components/AdminAIPanel.vue'
import { ElMessageBox, ElMessage } from 'element-plus'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const activeMenu = computed(() => route.path)

onMounted(async () => {
  if (!userStore.userInfo) {
    try {
      await userStore.fetchUserInfo()
    } catch (e) {
      // 静默处理
    }
  }
})

async function handleCommand(cmd) {
  if (cmd === 'logout') {
    try {
      await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })
      userStore.logout()
      ElMessage.success('已退出登录')
      router.push('/login')
    } catch (e) {
      // 取消
    }
  }
}
</script>

<style scoped>
.main-layout {
  height: 100vh;
  overflow: hidden;
}
/* 内层容器占满剩余高度并允许内容区内部滚动，侧边栏保持固定 */
.main-layout > .el-container {
  flex: 1;
  min-height: 0;
  overflow: hidden;
}
.app-header {
  background: linear-gradient(90deg, #C8161D 0%, #A01016 100%);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  z-index: 10;
}
.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}
.app-title {
  color: #fff;
  font-size: 18px;
  font-weight: 600;
  letter-spacing: 1px;
}
.header-right {
  display: flex;
  align-items: center;
}
.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #fff;
  cursor: pointer;
}
.user-name {
  font-size: 14px;
}
.app-aside {
  background: transparent;
  border-right: none;
  box-shadow: none;
}
.side-menu {
  border-right: none;
  height: calc(100vh - 60px);
  padding: 16px 10px;
  background: transparent;
}
.side-menu :deep(.el-menu-item) {
  border-radius: 10px;
  margin-bottom: 10px;
  background-color: #ffffff !important;
  color: #333333 !important;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.10);
}
/* 选中项：白底 + 深红加粗文字 + 左侧红条，保证文字清晰可读 */
.side-menu :deep(.el-menu-item.is-active) {
  background-color: #ffffff !important;
  color: #C8161D !important;
  font-weight: 700;
  border-right: none;
  border-left: 3px solid #C8161D;
  box-shadow: 0 2px 10px rgba(200, 22, 29, 0.25);
}
.side-menu :deep(.el-menu-item:hover) {
  background-color: #ffffff !important;
  color: #C8161D;
}
.app-main {
  padding: 20px;
  overflow-y: auto;
  height: 100%;
}
</style>
