<template>
  <div>
    <div class="aip-section-title">AI个人综合学习报告</div>
    <p class="aip-section-desc">选择党员，AI 结合其学习时长、任务完成率、考试正确率、错题分布等生成<strong>周期综合报告</strong>：综合评分、维度拆解、评价与阶段建议</p>

    <div class="aip-form">
      <div class="aip-field">
        <span class="aip-field-label">所属支部</span>
        <el-select v-model="orgId" placeholder="请选择支部" style="width: 220px" @change="onOrgChange">
          <el-option v-for="org in orgs" :key="org.id" :label="org.name" :value="org.id" />
        </el-select>
      </div>
      <div class="aip-field">
        <span class="aip-field-label">党员</span>
        <el-select v-model="memberId" placeholder="请选择党员" style="width: 180px" :loading="memberLoading" filterable>
          <el-option v-for="m in members" :key="m.id" :label="m.name" :value="m.id" />
        </el-select>
      </div>
      <div class="aip-actions">
        <el-button type="primary" :loading="loading" :disabled="!memberId" @click="generate">
          <el-icon v-if="!loading"><MagicStick /></el-icon> 生成报告
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>AI 正在分析学习数据，请稍候...</p>
    </div>

    <div v-if="result" class="aip-result">
      <div class="aip-toolbar">
        <div class="aip-result-title">{{ result.memberName }} · AI学习报告</div>
      </div>
      <div class="aip-meta">
        <el-tag size="small" type="danger">综合 {{ result.overallScore }} 分</el-tag>
        <el-tag size="small" type="warning">{{ result.level }}</el-tag>
      </div>

      <div v-if="result.dimensions && result.dimensions.length" class="aip-block">
        <div class="aip-sub-title">评价维度</div>
        <div v-for="(d, i) in result.dimensions" :key="i" class="aip-progress-row">
          <div class="aip-progress-head">
            <span>{{ d.name }}</span>
            <span class="score">{{ d.score }}分</span>
          </div>
          <div class="aip-progress-track">
            <div class="aip-progress-fill" :style="{ width: Math.min(d.score, 100) + '%' }"></div>
          </div>
          <div style="font-size:12px;color:#888;margin-top:2px;">{{ d.comment }}</div>
        </div>
      </div>

      <div class="aip-block">
        <div class="aip-sub-title">综合评价</div>
        <div class="aip-text">{{ result.summary }}</div>
      </div>

      <div v-if="result.suggestions && result.suggestions.length" class="aip-block">
        <div class="aip-sub-title">阶段建议</div>
        <ul class="aip-ul">
          <li v-for="(s, i) in result.suggestions" :key="i">{{ s }}</li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Loading, MagicStick } from '@element-plus/icons-vue'
import { apiPost, useOrgMember } from './common'

const { orgs, orgId, members, memberId, memberLoading, loadOrgs, onOrgChange } = useOrgMember()
const loading = ref(false)
const result = ref(null)

async function generate() {
  if (!memberId.value || loading.value) return
  loading.value = true
  result.value = null
  try {
    result.value = await apiPost('/mobile/report/ai-assessment', { memberId: memberId.value })
  } catch (e) {
    result.value = null
  } finally {
    loading.value = false
  }
}

onMounted(loadOrgs)
</script>
