<template>
  <div>
    <div class="aip-section-title">AI分阶段学习路线图</div>
    <p class="aip-section-desc">将学习建议升级为<strong>可执行路线</strong>：阶段1学什么 → 阶段2做什么练习 → 阶段3复测，并显示阶段目标与完成状态</p>

    <div class="aip-form">
      <div class="aip-field">
        <span class="aip-field-label">所属支部</span>
        <el-select v-model="orgId" placeholder="请选择支部" style="width: 200px" @change="onOrgChange">
          <el-option v-for="org in orgs" :key="org.id" :label="org.name" :value="org.id" />
        </el-select>
      </div>
      <div class="aip-field">
        <span class="aip-field-label">党员</span>
        <el-select v-model="memberId" placeholder="请选择党员" style="width: 160px" :loading="memberLoading" filterable>
          <el-option v-for="m in members" :key="m.id" :label="m.name" :value="m.id" />
        </el-select>
      </div>
      <div class="aip-field">
        <span class="aip-field-label">目标</span>
        <el-input v-model="target" placeholder="学习目标" style="width: 200px" />
      </div>
      <div class="aip-field">
        <span class="aip-field-label">周期(天)</span>
        <el-input-number v-model="periodDays" :min="7" :max="90" :step="7" style="width: 120px" />
      </div>
      <div class="aip-actions">
        <el-button type="primary" :loading="loading" :disabled="!memberId" @click="generate">
          <el-icon v-if="!loading"><MagicStick /></el-icon> 生成路线
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>AI 正在制定学习路线图...</p>
    </div>

    <div v-if="result" class="aip-result">
      <div class="aip-toolbar">
        <div class="aip-result-title">{{ result.memberName }} · {{ result.currentLevel || '学习' }} → {{ result.target || target }}</div>
      </div>
      <div class="aip-meta">
        <el-tag size="small" type="danger">周期 {{ result.totalDays || periodDays }} 天</el-tag>
        <el-tag v-for="(t, i) in result.focusTags" :key="i" size="small" type="warning" style="margin-right:4px;">{{ t }}</el-tag>
      </div>

      <div v-for="(stage, si) in result.stages" :key="si" class="aip-block">
        <div class="aip-sec-head" style="display:flex;justify-content:space-between;align-items:center;">
          <span class="aip-sub-title" style="margin:0;">阶段{{ stage.stageNo }} · {{ stage.stageName }}</span>
          <span class="aip-sec-min">{{ stage.durationDays }}天</span>
        </div>
        <div v-if="stage.objectives && stage.objectives.length" class="aip-sec-content" style="margin-top:6px;">
          <strong>阶段目标：</strong>
          <ul class="aip-ul">
            <li v-for="(o, oi) in stage.objectives" :key="oi">{{ o }}</li>
          </ul>
        </div>
        <div v-if="stage.contents && stage.contents.length" class="aip-sec-content" style="margin-top:6px;">
          <strong>学习内容：</strong>
          <ul class="aip-ul">
            <li v-for="(c, ci) in stage.contents" :key="ci">{{ c.title }}<span v-if="c.reason" style="color:#999;">（{{ c.reason }}）</span></li>
          </ul>
        </div>
        <div v-if="stage.exam" class="aip-sec-content" style="margin-top:6px;">
          <strong>阶段测验：</strong>{{ stage.exam.suggestedCount }} 题，目标 {{ stage.exam.targetScore }} 分
        </div>
        <div v-if="stage.kpis && stage.kpis.length" class="aip-sec-content" style="margin-top:6px;">
          <strong>阶段KPI：</strong>
          <span v-for="(k, ki) in stage.kpis" :key="ki" style="margin-right:12px;">{{ k.metric }} ≥ {{ k.target }}</span>
        </div>
      </div>

      <div v-if="result.nextAction" class="aip-block" style="border-bottom:none;">
        <div class="aip-sub-title">下一步行动</div>
        <div class="aip-text">{{ result.nextAction }}</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Loading, MagicStick } from '@element-plus/icons-vue'
import { apiPost, useOrgMember } from './common'

const { orgs, orgId, members, memberId, memberLoading, loadOrgs, onOrgChange } = useOrgMember()
const target = ref('系统掌握党史理论，提升测验成绩')
const periodDays = ref(30)
const loading = ref(false)
const result = ref(null)

async function generate() {
  if (!memberId.value || loading.value) return
  loading.value = true
  result.value = null
  try {
    result.value = await apiPost('/ai/roadmap', {
      memberId: memberId.value,
      target: target.value.trim() || undefined,
      periodDays: periodDays.value
    })
  } catch (e) {
    result.value = null
  } finally {
    loading.value = false
  }
}

onMounted(loadOrgs)
</script>
