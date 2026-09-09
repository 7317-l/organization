<template>
  <!-- AI 悬浮按钮 -->
  <div v-if="!visible" class="admin-ai-fab" @click="visible = true" title="AI功能中心">
    <el-icon :size="26"><Search /></el-icon>
    <span class="fab-text">AI</span>
  </div>

  <!-- AI 面板 -->
  <div v-if="visible" class="admin-ai-overlay" @click.self="visible = false">
    <div class="admin-ai-panel">
      <!-- 头部 -->
      <div class="ai-panel-header">
        <div class="header-left">
          <div class="ai-logo">AI</div>
          <div>
            <div class="header-title">管理后台 · AI功能中心</div>
            <div class="header-sub">党建问答 · 数据查询 · 学习分析 · 宣讲素材 · 语音交互</div>
          </div>
        </div>
        <div class="header-right">
          <span class="ai-status"><span class="status-dot"></span>在线</span>
          <el-icon class="close-btn" @click="visible = false"><Close /></el-icon>
        </div>
      </div>

      <div class="ai-panel-body">
        <!-- 左侧分组导航 -->
        <div class="ai-sidebar">
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
        </div>

        <!-- 右侧内容 -->
        <div class="ai-content">
          <component :is="currentComponent" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import {
  Search, Close, ChatDotRound, DataLine, Histogram, Document, PieChart,
  MapLocation, Warning, Notebook, Files, Calendar, TrendCharts, User, Trophy, Connection
} from '@element-plus/icons-vue'

import KnowledgeQa from './ai-panel/KnowledgeQa.vue'
import Nl2SqlPanel from './ai-panel/Nl2SqlPanel.vue'
import BranchStatsPanel from './ai-panel/BranchStatsPanel.vue'
import ReportPanel from './ai-panel/ReportPanel.vue'
import ClusterPanel from './ai-panel/ClusterPanel.vue'
import RoadmapPanel from './ai-panel/RoadmapPanel.vue'
import WarningsPanel from './ai-panel/WarningsPanel.vue'
import SpeechPanel from './ai-panel/SpeechPanel.vue'
import MaterialGenPanel from './ai-panel/MaterialGenPanel.vue'
import MeetingBriefPanel from './ai-panel/MeetingBriefPanel.vue'
import OrgAssessmentPanel from './ai-panel/OrgAssessmentPanel.vue'
import DevelopPanel from './ai-panel/DevelopPanel.vue'
import StarMembersPanel from './ai-panel/StarMembersPanel.vue'
import PairHelpPanel from './ai-panel/PairHelpPanel.vue'
import './ai-panel/panel-style.css'

const API_BASE = '/api/v1'
const visible = ref(false)
const activeNav = ref('knowledge')

const navGroups = [
  {
    title: '问答与查询',
    items: [
      { key: 'knowledge', label: 'AI党建问答', icon: ChatDotRound },
      { key: 'nl2sql', label: 'AI自然语言查询', icon: DataLine },
      { key: 'branch', label: '支部学习数据', icon: Histogram }
    ]
  },
  {
    title: '学习分析',
    items: [
      { key: 'report', label: '个人学习报告', icon: Document },
      { key: 'cluster', label: '错题智能聚类', icon: PieChart },
      { key: 'roadmap', label: '学习路线图', icon: MapLocation },
      { key: 'warnings', label: '学习预警', icon: Warning }
    ]
  },
  {
    title: '党建应用',
    items: [
      { key: 'speech', label: 'AI宣讲稿生成', icon: Notebook },
      { key: 'material', label: 'AI素材生成', icon: Files },
      { key: 'meeting', label: '组织生活总结', icon: Calendar },
      { key: 'assessment', label: '支部季度考核', icon: TrendCharts },
      { key: 'develop', label: '党员发展AI辅助', icon: User },
      { key: 'stars', label: '学习标兵评选', icon: Trophy },
      { key: 'pairhelp', label: '互助智能匹配', icon: Connection }
    ]
  }
]

const componentMap = {
  knowledge: KnowledgeQa,
  nl2sql: Nl2SqlPanel,
  branch: BranchStatsPanel,
  report: ReportPanel,
  cluster: ClusterPanel,
  roadmap: RoadmapPanel,
  warnings: WarningsPanel,
  speech: SpeechPanel,
  material: MaterialGenPanel,
  meeting: MeetingBriefPanel,
  assessment: OrgAssessmentPanel,
  develop: DevelopPanel,
  stars: StarMembersPanel,
  pairhelp: PairHelpPanel
}

const currentComponent = computed(() => componentMap[activeNav.value] || KnowledgeQa)
</script>

<style scoped>
.admin-ai-fab {
  position: fixed; right: 24px; bottom: 24px; width: 56px; height: 56px;
  border-radius: 50%; background: linear-gradient(135deg, #C8161D, #A01016);
  color: #fff; display: flex; flex-direction: column; align-items: center;
  justify-content: center; cursor: pointer; box-shadow: 0 4px 16px rgba(200,22,29,0.4);
  z-index: 9998; transition: transform 0.2s;
}
.admin-ai-fab:hover { transform: scale(1.08); }
.fab-text { font-size: 10px; font-weight: 600; margin-top: 2px; }
.admin-ai-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,0.45); z-index: 9999;
  display: flex; align-items: center; justify-content: center;
}
.admin-ai-panel {
  width: 1080px; height: 700px; background: #fff; border-radius: 12px;
  overflow: hidden; display: flex; flex-direction: column;
  box-shadow: 0 20px 60px rgba(0,0,0,0.3);
}
.ai-panel-header {
  background: linear-gradient(90deg, #C8161D, #A01016); color: #fff;
  padding: 14px 20px; display: flex; align-items: center; justify-content: space-between;
  flex-shrink: 0;
}
.header-left { display: flex; align-items: center; gap: 12px; }
.ai-logo {
  width: 40px; height: 40px; border-radius: 50%; background: rgba(255,255,255,0.2);
  display: flex; align-items: center; justify-content: center; font-weight: 700;
}
.header-title { font-size: 16px; font-weight: 600; }
.header-sub { font-size: 12px; opacity: 0.85; margin-top: 2px; }
.header-right { display: flex; align-items: center; gap: 16px; }
.ai-status { font-size: 13px; display: flex; align-items: center; gap: 6px; }
.status-dot { width: 8px; height: 8px; border-radius: 50%; background: #4ade80; }
.close-btn { cursor: pointer; font-size: 20px; }
.ai-panel-body { flex: 1; display: flex; overflow: hidden; }
.ai-sidebar {
  width: 224px; background: #f8f9fa; border-right: 1px solid #eaecef;
  padding: 8px 0 12px; display: flex; flex-direction: column;
  overflow-y: auto; flex-shrink: 0;
}
.nav-group-title {
  font-size: 11px; color: #999; padding: 10px 20px 4px; font-weight: 600;
  letter-spacing: 1px;
}
.nav-item {
  display: flex; align-items: center; gap: 10px; padding: 9px 20px;
  cursor: pointer; font-size: 13px; color: #333; transition: all 0.15s;
  border-left: 3px solid transparent;
}
.nav-item:hover { background: #f0f0f0; }
.nav-item.active { background: #fff; border-left-color: #C8161D; color: #C8161D; font-weight: 600; }
.ai-content { flex: 1; overflow-y: auto; padding: 20px; }
</style>
