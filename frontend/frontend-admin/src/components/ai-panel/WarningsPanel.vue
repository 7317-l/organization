<template>
  <div>
    <div class="aip-section-title">AI学习预警</div>
    <p class="aip-section-desc">识别<strong>连续低正确率、长期未完成任务、学习时长异常</strong>等党员，触发针对性学习提醒（可与通知中心打通）</p>

    <div class="aip-form">
      <div class="aip-field">
        <span class="aip-field-label">所属支部</span>
        <el-select v-model="orgId" placeholder="全部支部" clearable style="width: 220px" @change="loadWarnings">
          <el-option v-for="org in orgs" :key="org.id" :label="org.name" :value="org.id" />
        </el-select>
      </div>
      <div class="aip-actions">
        <el-button type="primary" :loading="loading" @click="loadWarnings">
          <el-icon v-if="!loading"><Search /></el-icon> 查询预警
        </el-button>
        <el-button :loading="triggering" @click="triggerWarnings">触发扫描并推送</el-button>
      </div>
    </div>

    <div v-if="result" class="aip-data">
      <div class="aip-data-title">预警概览 · 共 {{ result.totalWarnings }} 条</div>
      <div class="aip-cards">
        <div v-for="(count, type, i) in result.typeBreakdown" :key="i" class="aip-card" :class="{ warn: count > 0 }">
          <div class="aip-num">{{ count }}</div>
          <div class="aip-label">{{ warningTypeText(type) }}</div>
        </div>
      </div>
    </div>

    <div v-if="result && result.warnings && result.warnings.length" class="aip-block" style="border:1px solid #eee;border-radius:8px;padding:14px 16px;">
      <div class="aip-sub-title">预警明细</div>
      <div class="aip-table-wrap">
        <table class="aip-table">
          <thead>
            <tr>
              <th>党员</th><th>支部</th><th>预警类型</th><th>预警内容</th><th>指标</th><th>建议</th><th>发现时间</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(w, i) in result.warnings" :key="i">
              <td>{{ w.memberName }}</td>
              <td>{{ w.organizationName }}</td>
              <td><el-tag size="small" type="danger">{{ w.warningTypeText || w.warningType }}</el-tag></td>
              <td>{{ w.message }}</td>
              <td>{{ w.metricValue }}{{ w.threshold ? '（阈值' + w.threshold + '）' : '' }}</td>
              <td>{{ w.suggestion }}</td>
              <td>{{ fmtTime(w.detectedAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    <div v-else-if="result" class="aip-empty">
      <el-icon :size="40"><CircleCheck /></el-icon>
      <p>暂无学习预警，全部党员状态正常</p>
    </div>
    <div v-else class="aip-empty">
      <el-icon :size="40"><Warning /></el-icon>
      <p>选择支部后点击「查询预警」查看异常党员</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Loading, Search, Warning, CircleCheck } from '@element-plus/icons-vue'
import { apiGet, apiPost, loadOrgList } from './common'

const orgs = ref([])
const orgId = ref(null)
const loading = ref(false)
const triggering = ref(false)
const result = ref(null)

async function loadWarnings() {
  loading.value = true
  try {
    result.value = await apiGet('/ai/learning-warnings', { organizationId: orgId.value })
  } catch (e) {
    result.value = null
  } finally {
    loading.value = false
  }
}

async function triggerWarnings() {
  if (triggering.value) return
  triggering.value = true
  try {
    const r = await apiPost('/ai/learning-warnings/trigger', { organizationId: orgId.value || undefined })
    if (r && r.warnings) {
      result.value = {
        totalWarnings: r.warningCount,
        warnings: r.warnings,
        typeBreakdown: (r.warnings || []).reduce((acc, w) => {
          const k = w.warningTypeText || w.warningType || 'other'
          acc[k] = (acc[k] || 0) + 1
          return acc
        }, {})
      }
    }
    window.ElMessage && window.ElMessage.success('扫描完成，已推送 ' + (r?.notificationSentCount || 0) + ' 条提醒')
  } catch (e) {
    // 触发失败时仍展示查询结果
  } finally {
    triggering.value = false
  }
}

function warningTypeText(t) {
  const map = {
    low_accuracy: '低正确率', task_overdue: '任务逾期', low_activity: '活跃度低', duration_abnormal: '时长异常'
  }
  return map[t] || t
}

function fmtTime(v) {
  if (!v) return '-'
  return String(v).replace('T', ' ').slice(0, 16)
}

onMounted(async () => { orgs.value = await loadOrgList() })
</script>
