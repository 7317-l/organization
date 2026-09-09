<template>
  <div class="ai-chat-overlay" @click.self="closeAI">
    <div class="ai-chat">
      <!-- ========================================
           头部
           ======================================== -->
      <header class="ai-header">
        <div class="ai-header-left">
          <div class="ai-logo">
            <span class="ai-logo-core">AI</span>
          </div>
          <div class="ai-header-title">
            <div class="ai-title">党员学习平台 · AI功能中心</div>
            <div class="ai-subtitle">党建问答 · 学习推荐 · 综合报告 · 错题聚类 · 互助匹配 · 语音交互</div>
          </div>
        </div>
        <div class="ai-header-right">
          <div class="ai-status">
            <span class="ai-status-dot"></span>
            AI 在线
          </div>
          <button class="close-button" @click="closeAI">×</button>
        </div>
      </header>

      <!-- ========================================
           主体：左侧分组导航 + 右侧内容
           ======================================== -->
      <div class="ai-body">
        <aside class="ai-sidebar">
          <template v-for="group in navGroups" :key="group.title">
            <div class="nav-group-title">{{ group.title }}</div>
            <div
              v-for="item in group.items"
              :key="item.key"
              class="nav-item"
              :class="{ active: activeNav === item.key }"
              @click="activeNav = item.key"
            >
              <el-icon><component :is="item.icon" /></el-icon>
              <span>{{ item.label }}</span>
            </div>
          </template>
        </aside>

        <main class="ai-content">
          <component :is="currentComponent" />
        </main>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import {
  Close, ChatDotRound, Reading, Document, MapLocation, PieChart, Connection
} from '@element-plus/icons-vue'

import KnowledgeQa from './ai-panel/KnowledgeQa.vue'
import RecommendPanel from './ai-panel/RecommendPanel.vue'
import ReportPanel from './ai-panel/ReportPanel.vue'
import RoadmapPanel from './ai-panel/RoadmapPanel.vue'
import ClusterPanel from './ai-panel/ClusterPanel.vue'
import PairHelpPanel from './ai-panel/PairHelpPanel.vue'
import './ai-panel/panel-style.css'

const emit = defineEmits(['close'])

const activeNav = ref('knowledge')

const navGroups = [
  {
    title: '学习问答',
    items: [
      { key: 'knowledge', label: 'AI党建知识问答', icon: ChatDotRound }
    ]
  },
  {
    title: '个人学习',
    items: [
      { key: 'recommend', label: '个性化学习推荐', icon: Reading },
      { key: 'report', label: 'AI综合学习报告', icon: Document },
      { key: 'roadmap', label: 'AI学习路线图', icon: MapLocation }
    ]
  },
  {
    title: '错题提升',
    items: [
      { key: 'cluster', label: '错题知识点聚类', icon: PieChart }
    ]
  },
  {
    title: '互助交互',
    items: [
      { key: 'pairhelp', label: 'AI辅助互助匹配', icon: Connection }
    ]
  }
]

const componentMap = {
  knowledge: KnowledgeQa,
  recommend: RecommendPanel,
  report: ReportPanel,
  roadmap: RoadmapPanel,
  cluster: ClusterPanel,
  pairhelp: PairHelpPanel
}

const currentComponent = computed(() => componentMap[activeNav.value] || KnowledgeQa)

function closeAI() {
  emit('close')
}
</script>

<style scoped>
.ai-chat-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
}

.ai-chat {
  width: 1080px;
  max-width: 95vw;
  height: 700px;
  max-height: 92vh;
  background: #fff;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

/* 头部 */
.ai-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 20px;
  background: linear-gradient(90deg, #C8161D, #A01016);
  color: #fff;
  flex-shrink: 0;
}

.ai-header-left { display: flex; align-items: center; gap: 12px; }

.ai-logo {
  width: 40px; height: 40px; border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  display: flex; align-items: center; justify-content: center;
}

.ai-logo-core { font-weight: 700; font-size: 15px; }

.ai-title { font-size: 16px; font-weight: 600; }
.ai-subtitle { font-size: 12px; opacity: 0.85; margin-top: 2px; }

.ai-header-right { display: flex; align-items: center; gap: 16px; }

.ai-status {
  display: flex; align-items: center; gap: 6px; font-size: 13px;
}
.ai-status-dot {
  width: 8px; height: 8px; border-radius: 50%;
  background: #4ade80; animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.close-button {
  width: 32px; height: 32px; border-radius: 50%;
  border: none; background: rgba(255, 255, 255, 0.15);
  color: #fff; font-size: 20px; cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: background 0.2s;
}
.close-button:hover { background: rgba(255, 255, 255, 0.3); }

/* 主体 */
.ai-body { flex: 1; display: flex; overflow: hidden; }

/* 左侧导航 */
.ai-sidebar {
  width: 224px;
  background: #f8f9fa;
  border-right: 1px solid #eaecef;
  padding: 8px 0 12px;
  display: flex; flex-direction: column;
  overflow-y: auto; flex-shrink: 0;
}

.nav-group-title {
  font-size: 11px; color: #999; padding: 10px 20px 4px;
  font-weight: 600; letter-spacing: 1px;
}

.nav-item {
  display: flex; align-items: center; gap: 10px;
  padding: 9px 20px; cursor: pointer;
  font-size: 13px; color: #333;
  transition: all 0.15s;
  border-left: 3px solid transparent;
}
.nav-item:hover { background: #f0f0f0; }
.nav-item.active {
  background: #fff; border-left-color: #C8161D;
  color: #C8161D; font-weight: 600;
}

/* 右侧内容 */
.ai-content { flex: 1; overflow-y: auto; padding: 20px; }
</style>
