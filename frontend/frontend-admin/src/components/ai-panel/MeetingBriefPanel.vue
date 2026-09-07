<template>
  <div>
    <div class="aip-section-title">AI组织生活总结</div>
    <p class="aip-section-desc">对三会一课/主题党日的<strong>活动内容、签到、党员心得</strong>进行汇总，AI 自动生成活动简报</p>

    <div class="aip-form">
      <div class="aip-field">
        <span class="aip-field-label">所属支部</span>
        <el-select v-model="orgId" placeholder="全部支部" clearable style="width: 200px">
          <el-option v-for="org in orgs" :key="org.id" :label="org.name" :value="org.id" />
        </el-select>
      </div>
      <div class="aip-field">
        <span class="aip-field-label">开始日期</span>
        <el-date-picker v-model="startDate" type="date" value-format="YYYY-MM-DD" style="width: 150px" />
      </div>
      <div class="aip-field">
        <span class="aip-field-label">结束日期</span>
        <el-date-picker v-model="endDate" type="date" value-format="YYYY-MM-DD" style="width: 150px" />
      </div>
      <div class="aip-actions">
        <el-button type="primary" :loading="loading" @click="generate">
          <el-icon v-if="!loading"><MagicStick /></el-icon> 生成简报
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>AI 正在汇总组织生活情况...</p>
    </div>

    <div v-if="result" class="aip-result">
      <div class="aip-toolbar">
        <div class="aip-result-title">{{ result.organizationName || '全部支部' }} · 组织生活简报（{{ result.activityCount }} 场活动）</div>
        <el-button size="small" @click="copyText(result.brief)"><el-icon><CopyDocument /></el-icon>复制简报</el-button>
      </div>
      <div class="aip-meta">
        <el-tag size="small" type="info">{{ result.period?.startDate?.slice(0, 10) }} 至 {{ result.period?.endDate?.slice(0, 10) }}</el-tag>
        <el-tag size="small" type="success">心得 {{ result.totalHearts || 0 }} 篇</el-tag>
        <el-tag v-if="result.attendanceRate !== null && result.attendanceRate !== undefined" size="small" type="warning">
          出勤率 {{ result.attendanceRate }}%
        </el-tag>
      </div>

      <div v-if="result.typeBreakdown && result.typeBreakdown.length" class="aip-block">
        <div class="aip-sub-title">活动类型分布</div>
        <div style="display:flex;flex-wrap:wrap;gap:10px;">
          <el-tag v-for="(t, i) in result.typeBreakdown" :key="i" size="small" style="margin-right:4px;">
            {{ t.typeName }} × {{ t.count }}
          </el-tag>
        </div>
      </div>

      <div v-if="result.brief" class="aip-block">
        <div class="aip-sub-title">AI 简报</div>
        <div class="aip-text">{{ result.brief }}</div>
      </div>

      <div v-if="result.keyPoints && result.keyPoints.length" class="aip-block">
        <div class="aip-sub-title">要点归纳</div>
        <ul class="aip-ul">
          <li v-for="(kp, i) in result.keyPoints" :key="i">{{ kp }}</li>
        </ul>
      </div>

      <div v-if="result.perActivity && result.perActivity.length" class="aip-block" style="border-bottom:none;">
        <div class="aip-sub-title">活动明细</div>
        <div v-for="(a, i) in result.perActivity" :key="i" class="aip-section-card">
          <div class="aip-sec-head">
            <span class="aip-sec-title">{{ a.title }}<span style="color:#999;font-weight:400;margin-left:8px;">{{ a.typeName }}</span></span>
            <span class="aip-sec-min">{{ String(a.activityTime).replace('T', ' ').slice(0, 10) }}</span>
          </div>
          <div class="aip-sec-content">{{ a.summary }}</div>
          <div v-if="a.keyPoints && a.keyPoints.length" class="aip-sec-content" style="margin-top:4px;color:#C8161D;">
            要点：{{ a.keyPoints.join('；') }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Loading, MagicStick, CopyDocument } from '@element-plus/icons-vue'
import { apiPost, loadOrgList } from './common'

const orgs = ref([])
const orgId = ref(null)
const startDate = ref('')
const endDate = ref('')
const loading = ref(false)
const result = ref(null)

function defaultRange() {
  const now = new Date()
  const first = new Date(now.getFullYear(), now.getMonth(), 1)
  const pad = (n) => String(n).padStart(2, '0')
  startDate.value = `${first.getFullYear()}-${pad(first.getMonth() + 1)}-${pad(first.getDate())}`
  endDate.value = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`
}

async function generate() {
  if (loading.value) return
  if (!startDate.value || !endDate.value) { ElMessage.warning('请选择起止日期'); return }
  loading.value = true
  result.value = null
  try {
    result.value = await apiPost('/meeting-activities/brief', {
      organizationId: orgId.value || undefined,
      startDate: startDate.value + 'T00:00:00',
      endDate: endDate.value + 'T23:59:59'
    })
  } catch (e) {
    ElMessage.error(e?.message || '生成失败')
  } finally {
    loading.value = false
  }
}

async function copyText(text) {
  if (!text) return
  try {
    await navigator.clipboard.writeText(text)
    ElMessage.success('简报已复制')
  } catch (e) {
    ElMessage.warning('复制失败，请手动选择复制')
  }
}

onMounted(async () => {
  orgs.value = await loadOrgList()
  defaultRange()
})
</script>
