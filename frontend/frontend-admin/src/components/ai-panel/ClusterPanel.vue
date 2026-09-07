<template>
  <div>
    <div class="aip-section-title">错题知识点智能聚类</div>
    <p class="aip-section-desc">将党员长期错题按知识点自动聚类，输出<strong>薄弱知识板块 TOP、错误频率和专项巩固建议</strong></p>

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
        <el-button type="primary" :loading="loading" :disabled="!memberId" @click="cluster">
          <el-icon v-if="!loading"><MagicStick /></el-icon> 聚类分析
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>正在对错题进行知识点聚类...</p>
    </div>

    <div v-if="result" class="aip-result">
      <div class="aip-toolbar">
        <div class="aip-result-title">{{ result.memberName }} · 薄弱知识点聚类</div>
      </div>

      <div v-if="result.clusters && result.clusters.length" class="aip-block">
        <div class="aip-sub-title">错题聚类结果（按薄弱程度排序）</div>
        <div v-for="(c, i) in result.clusters" :key="i" class="aip-section-card">
          <div class="aip-sec-head">
            <span class="aip-sec-title">TOP{{ i + 1 }} · {{ c.clusterName }}</span>
            <span class="aip-sec-min">错题 {{ c.errorCount }} 题 · 严重度 {{ c.severity }}</span>
          </div>
          <div class="aip-sec-content">
            涉及知识点：
            <el-tag v-for="(t, ti) in c.knowledgeTags" :key="ti" size="small" style="margin:2px 4px 2px 0;">{{ t }}</el-tag>
          </div>
        </div>
      </div>

      <div v-if="result.topWeaknessTags && result.topWeaknessTags.length" class="aip-block">
        <div class="aip-sub-title">薄弱知识 TOP 标签</div>
        <el-tag v-for="(t, i) in result.topWeaknessTags" :key="i" type="danger" size="small" style="margin:0 6px 6px 0;">{{ t }}</el-tag>
      </div>

      <div v-if="result.suggestion" class="aip-block">
        <div class="aip-sub-title">专项巩固建议</div>
        <div class="aip-text">{{ result.suggestion }}</div>
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

async function cluster() {
  if (!memberId.value || loading.value) return
  loading.value = true
  result.value = null
  try {
    result.value = await apiPost('/kmeans/cluster', { partyMemberId: memberId.value, clusterCount: 3 })
  } catch (e) {
    result.value = null
  } finally {
    loading.value = false
  }
}

onMounted(loadOrgs)
</script>
