<template>
  <div>
    <div class="aip-section-title">AI支部季度考核</div>
    <p class="aip-section-desc">综合<strong>任务完成率、考试成绩、学习活跃度、活动参与情况</strong>等形成支部季度量化评级，并输出问题与整改建议</p>

    <div class="aip-form">
      <div class="aip-field">
        <span class="aip-field-label">考核支部</span>
        <el-select v-model="orgId" placeholder="请选择支部" style="width: 220px">
          <el-option v-for="org in orgs" :key="org.id" :label="org.name" :value="org.id" />
        </el-select>
      </div>
      <div class="aip-field">
        <span class="aip-field-label">季度</span>
        <el-select v-model="quarter" style="width: 140px">
          <el-option v-for="q in quarterOptions" :key="q" :label="q" :value="q" />
        </el-select>
      </div>
      <div class="aip-actions">
        <el-button type="primary" :loading="loading" :disabled="!orgId" @click="generate">
          <el-icon v-if="!loading"><MagicStick /></el-icon> 生成考核报告
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>AI 正在对支部进行季度量化考核...</p>
    </div>

    <div v-if="result" class="aip-result">
      <div class="aip-toolbar">
        <div class="aip-result-title">{{ result.organizationName }} · {{ result.quarter }} 季度考核</div>
      </div>
      <div class="aip-meta">
        <el-tag size="small" type="danger">评级：{{ result.rating }}</el-tag>
        <el-tag size="small" type="warning">量化得分：{{ result.ratingScore }}</el-tag>
      </div>

      <div v-if="result.metrics && Object.keys(result.metrics).length" class="aip-block">
        <div class="aip-sub-title">核心指标</div>
        <div class="aip-cards">
          <div v-for="(v, k, i) in result.metrics" :key="i" class="aip-card">
            <div class="aip-num">{{ fmtNum(v) }}</div>
            <div class="aip-label">{{ metricLabel(k) }}</div>
          </div>
        </div>
      </div>

      <div v-if="result.ratings && result.ratings.length" class="aip-block">
        <div class="aip-sub-title">量化评级维度</div>
        <div v-for="(r, i) in result.ratings" :key="i" class="aip-section-card">
          <div class="aip-sec-head">
            <span class="aip-sec-title">{{ r.dimension }}</span>
            <span class="aip-sec-min">{{ r.score }}分 · {{ r.grade }}</span>
          </div>
          <div class="aip-sec-content">{{ r.comment }}</div>
        </div>
      </div>

      <div v-if="result.suggestions && result.suggestions.length" class="aip-block">
        <div class="aip-sub-title">问题与整改建议</div>
        <div v-for="(s, i) in result.suggestions" :key="i" class="aip-section-card" style="border-left:3px solid #e6a23c;">
          <div class="aip-sec-content">
            <strong>问题：</strong>{{ s.issue }}
          </div>
          <div class="aip-sec-content" style="margin-top:4px;">
            <strong>建议：</strong>{{ s.suggestion }}
          </div>
          <div class="aip-sec-content" style="margin-top:4px;color:#C8161D;">优先级：{{ priorityText(s.priority) }}</div>
        </div>
      </div>

      <div v-if="result.report" class="aip-block" style="border-bottom:none;">
        <div class="aip-sub-title">考核报告全文</div>
        <div class="aip-text">{{ result.report }}</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Loading, MagicStick } from '@element-plus/icons-vue'
import { apiPost, loadOrgList } from './common'

const orgs = ref([])
const orgId = ref(null)
const loading = ref(false)
const result = ref(null)

function currentQuarter() {
  const now = new Date()
  const q = Math.floor(now.getMonth() / 3) + 1
  return `${now.getFullYear()}Q${q}`
}
const quarterOptions = ref([])
for (let i = 0; i < 4; i++) {
  const d = new Date()
  d.setMonth(d.getMonth() - i * 3)
  quarterOptions.value.push(`${d.getFullYear()}Q${Math.floor(d.getMonth() / 3) + 1}`)
}
const quarter = ref(currentQuarter())

async function generate() {
  if (!orgId.value || loading.value) return
  loading.value = true
  result.value = null
  try {
    result.value = await apiPost('/ai/organization-report', {
      organizationId: orgId.value,
      quarter: quarter.value
    })
  } catch (e) {
    ElMessage.error(e?.message || '生成失败')
  } finally {
    loading.value = false
  }
}

function fmtNum(v) {
  if (v === null || v === undefined) return '-'
  return Number.isInteger(v) ? v : Number(v.toFixed(1))
}

function metricLabel(k) {
  const map = {
    taskCompletionRate: '任务完成率(%)', avgExamScore: '测验平均分', examPassRate: '测验通过率(%)',
    learningActivity: '学习活跃度', activityParticipation: '活动参与率(%)', memberCount: '党员数',
    totalLearningHours: '学习时长(小时)'
  }
  return map[k] || k
}

function priorityText(p) {
  const map = { high: '高', medium: '中', low: '低' }
  return map[p] || p
}

onMounted(async () => { orgs.value = await loadOrgList() })
</script>
