<template>
  <div>
    <div class="aip-section-title">党员发展AI辅助</div>
    <p class="aip-section-desc">对发展材料进行<strong>完整性检查、缺失项提示、关键时间节点提醒</strong>；思想汇报可给修改建议但保留人工审核</p>

    <el-tabs v-model="tab" type="border-card">
      <!-- ===== 材料校验 ===== -->
      <el-tab-pane label="发展材料AI校验" name="check">
        <div class="aip-form">
          <div class="aip-field">
            <span class="aip-field-label">发展流程</span>
            <el-select v-model="processId" placeholder="请选择党员发展流程" style="width: 300px" filterable @change="loadProcessDetail">
              <el-option v-for="p in processes" :key="p.id" :label="p.memberName + '（' + p.stageName + ' / ' + p.statusName + '）'" :value="p.id" />
            </el-select>
          </div>
          <div class="aip-actions">
            <el-button type="primary" :loading="checking" :disabled="!processId" @click="checkMaterials">
              <el-icon v-if="!checking"><MagicStick /></el-icon> AI校验材料
            </el-button>
          </div>
        </div>
        <div v-if="checkResult" class="aip-result">
          <div class="aip-toolbar">
            <div class="aip-result-title">{{ checkResult.stageName }} · 材料校验结果</div>
            <el-tag size="small" :type="checkResult.isComplete ? 'success' : 'danger'">
              {{ checkResult.isComplete ? '材料完整' : '材料缺失' }}
            </el-tag>
          </div>
          <div v-if="checkResult.missingMaterials && checkResult.missingMaterials.length" class="aip-block">
            <div class="aip-sub-title">缺失材料</div>
            <el-tag v-for="(m, i) in checkResult.missingMaterials" :key="i" type="danger" size="small" style="margin:0 6px 6px 0;">{{ m }}</el-tag>
          </div>
          <div v-if="checkResult.issues && checkResult.issues.length" class="aip-block">
            <div class="aip-sub-title">逐项检查</div>
            <div class="aip-table-wrap">
              <table class="aip-table">
                <thead><tr><th>材料</th><th>状态</th><th>检查结果</th><th>建议</th></tr></thead>
                <tbody>
                  <tr v-for="(it, i) in checkResult.issues" :key="i">
                    <td>{{ it.material }}</td>
                    <td>{{ statusText(it.status) }}</td>
                    <td>{{ it.checkResult }}</td>
                    <td>{{ it.suggestion }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          <div v-if="checkResult.score !== null && checkResult.score !== undefined" class="aip-meta">
            <el-tag size="small" type="warning">材料完整度 {{ checkResult.score }}%</el-tag>
          </div>
          <div v-if="checkResult.suggestion" class="aip-block" style="border-bottom:none;">
            <div class="aip-sub-title">校验建议</div>
            <div class="aip-text">{{ checkResult.suggestion }}</div>
          </div>
        </div>
      </el-tab-pane>

      <!-- ===== 思想汇报建议 ===== -->
      <el-tab-pane label="思想汇报AI建议" name="report">
        <div class="aip-form" style="flex-direction:column;align-items:stretch;">
          <div class="aip-field" style="align-items:flex-start;">
            <span class="aip-field-label" style="padding-top:8px;">汇报内容</span>
            <el-input
              v-model="reportContent"
              type="textarea"
              :rows="4"
              placeholder="粘贴党员思想汇报正文，AI 将评分并给出修改建议（最终由人工审核把关）"
              style="flex:1;"
            />
          </div>
          <div class="aip-actions">
            <el-button type="primary" :loading="reporting" :disabled="!reportContent.trim()" @click="suggestReport">
              <el-icon v-if="!reporting"><MagicStick /></el-icon> 生成建议
            </el-button>
          </div>
        </div>
        <div v-if="reportResult" class="aip-result">
          <div class="aip-toolbar">
            <div class="aip-result-title">思想汇报 AI 评估</div>
            <el-tag size="small" type="warning">综合 {{ reportResult.overallScore }} 分</el-tag>
          </div>
          <div v-if="reportResult.dimensions && reportResult.dimensions.length" class="aip-block">
            <div class="aip-sub-title">评估维度</div>
            <div v-for="(d, i) in reportResult.dimensions" :key="i" class="aip-progress-row">
              <div class="aip-progress-head"><span>{{ d.name }}</span><span class="score">{{ d.score }}分</span></div>
              <div class="aip-progress-track"><div class="aip-progress-fill" :style="{ width: Math.min(d.score, 100) + '%' }"></div></div>
              <div style="font-size:12px;color:#888;margin-top:2px;">{{ d.comment }}</div>
            </div>
          </div>
          <div v-if="reportResult.strengths && reportResult.strengths.length" class="aip-block">
            <div class="aip-sub-title">优点</div>
            <ul class="aip-ul"><li v-for="(s, i) in reportResult.strengths" :key="i">{{ s }}</li></ul>
          </div>
          <div v-if="reportResult.suggestions && reportResult.suggestions.length" class="aip-block">
            <div class="aip-sub-title">修改建议</div>
            <ul class="aip-ul"><li v-for="(s, i) in reportResult.suggestions" :key="i">{{ s }}</li></ul>
          </div>
          <div v-if="reportResult.rewrittenExcerpt" class="aip-block" style="border-bottom:none;">
            <div class="aip-sub-title">改写参考片段</div>
            <div class="aip-text">{{ reportResult.rewrittenExcerpt }}</div>
          </div>
        </div>
      </el-tab-pane>

      <!-- ===== 到期提醒 ===== -->
      <el-tab-pane label="发展关键节点提醒" name="reminder">
        <div class="aip-form">
          <div class="aip-field">
            <span class="aip-field-label">所属支部</span>
            <el-select v-model="remindOrgId" placeholder="全部支部" clearable style="width: 220px">
              <el-option v-for="org in orgs" :key="org.id" :label="org.name" :value="org.id" />
            </el-select>
          </div>
          <div class="aip-actions">
            <el-button type="primary" :loading="reminding" @click="triggerReminders">
              <el-icon v-if="!reminding"><MagicStick /></el-icon> 扫描并触发提醒
            </el-button>
          </div>
        </div>
        <div v-if="reminderResult" class="aip-data">
          <div class="aip-data-title">扫描结果</div>
          <div class="aip-cards">
            <div class="aip-card"><div class="aip-num">{{ reminderResult.scanned?.probationaryDue || 0 }}</div><div class="aip-label">预备期到期</div></div>
            <div class="aip-card"><div class="aip-num">{{ reminderResult.scanned?.materialMissing || 0 }}</div><div class="aip-label">材料缺失</div></div>
            <div class="aip-card"><div class="aip-num">{{ reminderResult.scanned?.reportDue || 0 }}</div><div class="aip-label">汇报到期</div></div>
            <div class="aip-card warn"><div class="aip-num">{{ reminderResult.sentCount || 0 }}</div><div class="aip-label">已推送通知</div></div>
          </div>
        </div>
        <div v-if="reminderResult && reminderResult.reminders && reminderResult.reminders.length" class="aip-block" style="border:1px solid #eee;border-radius:8px;">
          <div class="aip-sub-title">提醒明细</div>
          <div class="aip-table-wrap">
            <table class="aip-table">
              <thead><tr><th>党员</th><th>类型</th><th>提醒内容</th><th>截止日期</th></tr></thead>
              <tbody>
                <tr v-for="(r, i) in reminderResult.reminders" :key="i">
                  <td>{{ r.memberName }}</td>
                  <td><el-tag size="small" type="warning">{{ r.type }}</el-tag></td>
                  <td>{{ r.message }}</td>
                  <td>{{ String(r.dueDate || '').replace('T', ' ').slice(0, 10) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { MagicStick } from '@element-plus/icons-vue'
import { apiGet, apiPost, loadOrgList } from './common'

const tab = ref('check')
const orgs = ref([])
const processes = ref([])
const processId = ref(null)
const checking = ref(false)
const checkResult = ref(null)

const reportContent = ref('')
const reporting = ref(false)
const reportResult = ref(null)

const remindOrgId = ref(null)
const reminding = ref(false)
const reminderResult = ref(null)

async function loadProcesses() {
  try {
    const data = await apiGet('/party-development', { page: 1, size: 100 })
    processes.value = (data?.items || data?.data?.items || []) || []
  } catch (e) {
    processes.value = []
  }
}

async function loadProcessDetail() {
  checkResult.value = null
}

async function checkMaterials() {
  if (!processId.value || checking.value) return
  checking.value = true
  checkResult.value = null
  try {
    checkResult.value = await apiPost(`/party-development/${processId.value}/material-check`, {})
  } catch (e) {
    ElMessage.error(e?.message || '校验失败')
  } finally {
    checking.value = false
  }
}

async function suggestReport() {
  if (!reportContent.value.trim() || reporting.value) return
  reporting.value = true
  reportResult.value = null
  try {
    // 优先对选定流程的党员生成建议；未选流程时传0由后端按当前用户处理
    const pid = processId.value || 0
    reportResult.value = await apiPost(`/party-development/${pid}/report-suggestion`, {
      reportContent: reportContent.value
    })
  } catch (e) {
    ElMessage.error(e?.message || '生成建议失败')
  } finally {
    reporting.value = false
  }
}

async function triggerReminders() {
  if (reminding.value) return
  reminding.value = true
  reminderResult.value = null
  try {
    reminderResult.value = await apiPost('/party-development/reminders/trigger', {
      organizationId: remindOrgId.value || undefined,
      sendNotification: true
    })
    ElMessage.success('扫描完成，已推送 ' + (reminderResult.value?.sentCount || 0) + ' 条提醒')
  } catch (e) {
    ElMessage.error(e?.message || '触发失败')
  } finally {
    reminding.value = false
  }
}

function statusText(s) {
  const map = { ok: '正常', missing: '缺失', invalid: '不规范', warning: '提醒' }
  return map[s] || s
}

onMounted(async () => {
  orgs.value = await loadOrgList()
  loadProcesses()
})
</script>
