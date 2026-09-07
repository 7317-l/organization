<template>
  <div>
    <div class="aip-section-title">AI学习标兵评选</div>
    <p class="aip-section-desc">综合<strong>完成率、正确率、有效学习时长、活动参与</strong>等维度加权评分，AI 生成推荐名单与评选理由</p>

    <div class="aip-form">
      <div class="aip-field">
        <span class="aip-field-label">评选范围</span>
        <el-select v-model="orgId" placeholder="全部组织" clearable style="width: 220px">
          <el-option v-for="org in orgs" :key="org.id" :label="org.name" :value="org.id" />
        </el-select>
      </div>
      <div class="aip-field">
        <span class="aip-field-label">人数</span>
        <el-select v-model="topN" style="width: 100px">
          <el-option :value="5" label="前5" />
          <el-option :value="10" label="前10" />
          <el-option :value="20" label="前20" />
        </el-select>
      </div>
      <div class="aip-actions">
        <el-button type="primary" :loading="loading" @click="generate">
          <el-icon v-if="!loading"><MagicStick /></el-icon> AI评选标兵
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>AI 正在综合评分评选学习标兵...</p>
    </div>

    <div v-if="result" class="aip-result">
      <div class="aip-toolbar">
        <div class="aip-result-title">学习标兵推荐名单（{{ result.members?.length || 0 }} 人）</div>
      </div>
      <div v-if="result.scope" class="aip-meta">
        <el-tag size="small" type="info">{{ result.scope.organizationName || '全部组织' }}</el-tag>
        <el-tag size="small" type="success">参评党员 {{ result.scope.memberCount }} 人</el-tag>
      </div>
      <div v-if="result.members && result.members.length" class="aip-block" style="border-bottom:none;">
        <div v-for="(m, i) in result.members" :key="i" class="aip-section-card" :style="i < 3 ? 'border-left:3px solid #C8161D;' : ''">
          <div class="aip-sec-head">
            <span class="aip-sec-title">
              <el-tag size="small" :type="i < 3 ? 'danger' : 'info'" style="margin-right:6px;">TOP{{ m.rank }}</el-tag>
              {{ m.memberName }}
              <span style="color:#999;font-weight:400;margin-left:8px;">{{ m.organizationName }}</span>
            </span>
            <span class="aip-sec-min">{{ m.totalScore }}分 · {{ m.level }}</span>
          </div>
          <div v-if="m.dimensions && m.dimensions.length" class="aip-sec-content" style="margin-top:6px;">
            <div style="display:flex;flex-wrap:wrap;gap:4px 14px;">
              <span v-for="(d, di) in m.dimensions" :key="di" style="font-size:12px;color:#666;">
                {{ d.name }}：{{ d.score }}（权重{{ d.weight }}）
              </span>
            </div>
          </div>
          <div v-if="m.aiReason" class="aip-sec-content" style="margin-top:6px;color:#C8161D;">
            评选理由：{{ m.aiReason }}
          </div>
        </div>
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
const topN = ref(10)
const loading = ref(false)
const result = ref(null)

async function generate() {
  if (loading.value) return
  loading.value = true
  result.value = null
  try {
    result.value = await apiPost('/ai/star-members', {
      organizationId: orgId.value || undefined,
      topN: topN.value,
      includeReason: true
    })
  } catch (e) {
    ElMessage.error(e?.message || '评选失败')
  } finally {
    loading.value = false
  }
}

onMounted(async () => { orgs.value = await loadOrgList() })
</script>
