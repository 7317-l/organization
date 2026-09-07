<template>
  <div>
    <div class="aip-section-title">个性化学习推荐</div>
    <p class="aip-section-desc">根据你的<strong>错题、历史学习记录、支部任务</strong>生成学习内容推荐排序，并显示每条<strong>推荐原因</strong></p>

    <div class="aip-form">
      <div class="aip-field">
        <span class="aip-field-label">推荐数量</span>
        <el-select v-model="limit" style="width: 120px">
          <el-option :value="5" label="5 条" />
          <el-option :value="10" label="10 条" />
        </el-select>
      </div>
      <div class="aip-actions">
        <el-button type="primary" :loading="loading" @click="load">
          <el-icon v-if="!loading"><MagicStick /></el-icon> 获取推荐
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>AI 正在根据你的学习情况生成推荐...</p>
    </div>

    <div v-if="result" class="aip-result">
      <div class="aip-toolbar">
        <div class="aip-result-title">为你推荐的学习内容</div>
      </div>
      <div v-if="result.reason" class="aip-block" style="background:#fff7f7;">
        <div class="aip-sub-title">推荐原因</div>
        <div class="aip-text">{{ result.reason }}</div>
      </div>
      <div v-if="result.contents && result.contents.length" class="aip-block">
        <div class="aip-sub-title">推荐内容（{{ result.contents.length }} 条）</div>
        <div v-for="(c, i) in result.contents" :key="c.id || i" class="aip-section-card">
          <div class="aip-sec-head">
            <span class="aip-sec-title">{{ c.title }}</span>
            <span class="aip-sec-min">{{ c.contentTypeName || ('类型' + c.contentType) }}</span>
          </div>
          <div class="aip-sec-content">
            <el-tag v-if="c.categoryName" size="small" style="margin:2px 4px 2px 0;">{{ c.categoryName }}</el-tag>
            <el-tag v-for="(t, ti) in (c.tags || [])" :key="ti" size="small" type="info" style="margin:2px 4px 2px 0;">{{ t }}</el-tag>
            <span v-if="c.createdAt" style="color:#999;font-size:12px;margin-left:6px;">{{ formatDate(c.createdAt) }}</span>
          </div>
        </div>
      </div>
      <div v-else class="aip-empty">
        <p>暂无可推荐内容，请先完成一些学习任务</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { Loading, MagicStick } from '@element-plus/icons-vue'
import { apiGet } from './common'

const limit = ref(5)
const loading = ref(false)
const result = ref(null)

async function load() {
  if (loading.value) return
  loading.value = true
  result.value = null
  try {
    result.value = await apiGet('/mobile/recommendations', { limit: limit.value })
  } catch (e) {
    result.value = null
  } finally {
    loading.value = false
  }
}

function formatDate(v) {
  if (!v) return ''
  const d = new Date(v)
  if (isNaN(d.getTime())) return ''
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}
</script>
