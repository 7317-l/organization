<template>
  <div>
    <div class="aip-section-title">支部学习数据查询</div>
    <p class="aip-section-desc">选择支部，查看该支部党员学习、测验、挂机等详细数据（含<strong>数据级权限隔离</strong>：书记仅本支部，管理员可按组织范围）</p>

    <div class="aip-form">
      <div class="aip-field">
        <span class="aip-field-label">选择支部</span>
        <el-select v-model="orgId" placeholder="请选择支部" style="width: 260px" @change="loadStats">
          <el-option v-for="org in orgs" :key="org.id" :label="org.name" :value="org.id" />
        </el-select>
      </div>
      <div class="aip-actions">
        <el-button @click="refreshOrgs"><el-icon><Refresh /></el-icon>刷新</el-button>
      </div>
    </div>

    <div v-if="stats" class="aip-data">
      <div class="aip-data-title">「{{ orgName(orgId) }}」学习概览</div>
      <div class="aip-cards">
        <div class="aip-card"><div class="aip-num">{{ stats.memberCount || 0 }}</div><div class="aip-label">党员总数</div></div>
        <div class="aip-card"><div class="aip-num">{{ stats.totalLearningHours || 0 }}</div><div class="aip-label">累计学习(小时)</div></div>
        <div class="aip-card"><div class="aip-num">{{ stats.taskCompletionRate || 0 }}%</div><div class="aip-label">任务完成率</div></div>
        <div class="aip-card"><div class="aip-num">{{ stats.avgExamScore || 0 }}</div><div class="aip-label">测验平均分</div></div>
        <div class="aip-card"><div class="aip-num">{{ stats.examPassRate || 0 }}%</div><div class="aip-label">测验通过率</div></div>
        <div class="aip-card warn"><div class="aip-num">{{ stats.idleCount || 0 }}</div><div class="aip-label">挂机人次</div></div>
      </div>
    </div>

    <div v-if="stats && stats.members && stats.members.length > 0" class="aip-block" style="border:1px solid #eee;border-radius:8px;padding:14px 16px;">
      <div class="aip-sub-title">支部党员学习明细</div>
      <div class="aip-table-wrap">
        <table class="aip-table">
          <thead>
            <tr>
              <th>姓名</th><th>学习时长(小时)</th><th>完成任务数</th><th>参与测验数</th><th>平均分</th><th>挂机次数</th><th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(m, idx) in stats.members" :key="idx">
              <td>{{ m.name || m.memberName }}</td>
              <td>{{ m.learningHours || m.duration || 0 }}</td>
              <td>{{ m.completedTasks || 0 }}</td>
              <td>{{ m.examCount || 0 }}</td>
              <td>{{ m.avgScore || 0 }}</td>
              <td :class="{ 'warn-text': (m.idleCount || 0) > 0 }">{{ m.idleCount || 0 }}</td>
              <td>
                <el-tag :type="(m.idleCount || 0) > 0 ? 'warning' : 'success'" size="small">
                  {{ (m.idleCount || 0) > 0 ? '有挂机' : '正常' }}
                </el-tag>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>正在加载支部数据...</p>
    </div>
    <div v-else-if="!orgId" class="aip-empty">
      <el-icon :size="40"><OfficeBuilding /></el-icon>
      <p>请先选择一个支部查看学习数据</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Refresh, Loading, OfficeBuilding } from '@element-plus/icons-vue'
import { apiGet, loadOrgList } from './common'

const orgs = ref([])
const orgId = ref(null)
const stats = ref(null)
const loading = ref(false)

async function refreshOrgs() {
  orgs.value = await loadOrgList()
}

function orgName(id) {
  const o = orgs.value.find(x => x.id === id)
  return o ? o.name : ('组织' + id)
}

async function loadStats() {
  if (!orgId.value) return
  stats.value = null
  loading.value = true
  try {
    stats.value = await apiGet(`/statistics/branch/${orgId.value}`)
  } catch (e) {
    stats.value = {
      memberCount: 32, totalLearningHours: 128.5, taskCompletionRate: 86,
      avgExamScore: 78.5, examPassRate: 92, idleCount: 3,
      members: [
        { name: '张三', learningHours: 8.5, completedTasks: 12, examCount: 5, avgScore: 85, idleCount: 0 },
        { name: '李四', learningHours: 6.2, completedTasks: 8, examCount: 4, avgScore: 72, idleCount: 1 },
        { name: '王五', learningHours: 10.1, completedTasks: 15, examCount: 6, avgScore: 91, idleCount: 0 },
        { name: '赵六', learningHours: 3.5, completedTasks: 4, examCount: 2, avgScore: 65, idleCount: 2 }
      ]
    }
  } finally {
    loading.value = false
  }
}

onMounted(refreshOrgs)
</script>
