<template>
  <div>
    <div class="aip-section-title">AI辅助互助匹配</div>
    <p class="aip-section-desc">根据<strong>知识优势和薄弱板块</strong>匹配党员结对学习，AI 给出推荐理由与匹配分数</p>

    <div class="aip-form">
      <div class="aip-field">
        <span class="aip-field-label">我的薄弱板块</span>
        <el-input
          v-model="weaknessInput"
          placeholder="多个标签用逗号分隔，如：党史,二十大报告,党章"
          style="width: 300px"
        />
      </div>
      <div class="aip-field">
        <span class="aip-field-label">推荐人数</span>
        <el-select v-model="count" style="width: 100px">
          <el-option :value="3" label="3人" />
          <el-option :value="5" label="5人" />
          <el-option :value="8" label="8人" />
        </el-select>
      </div>
      <div class="aip-actions">
        <el-button type="primary" :loading="loading" :disabled="!tags.length" @click="match">
          <el-icon v-if="!loading"><MagicStick /></el-icon> 智能匹配
        </el-button>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>AI 正在匹配优势互补的帮扶对子...</p>
    </div>

    <div v-if="result && result.recommendations && result.recommendations.length" class="aip-result">
      <div class="aip-toolbar">
        <div class="aip-result-title">推荐结对名单（{{ result.recommendations.length }} 人）</div>
      </div>
      <div v-for="(r, i) in result.recommendations" :key="i" class="aip-block" :style="i === result.recommendations.length - 1 ? 'border-bottom:none;' : ''">
        <div class="aip-sec-head">
          <span class="aip-sec-title">
            <el-tag size="small" type="danger" style="margin-right:6px;">推荐{{ i + 1 }}</el-tag>
            {{ r.memberName }}
            <span style="color:#999;font-weight:400;margin-left:8px;">{{ r.organizationName }}</span>
          </span>
          <span class="aip-sec-min">{{ r.score }}分</span>
        </div>
        <div v-if="r.weaknessTags && r.weaknessTags.length" class="aip-sec-content" style="margin-top:6px;">
          知识画像：
          <el-tag v-for="(t, ti) in r.weaknessTags" :key="ti" size="small" style="margin:2px 4px 2px 0;">{{ t }}</el-tag>
        </div>
        <div v-if="r.matchReason" class="aip-sec-content" style="margin-top:6px;color:#C8161D;">
          匹配理由：{{ r.matchReason }}
        </div>
      </div>
    </div>
    <div v-else-if="result" class="aip-empty">
      <el-icon :size="40"><Connection /></el-icon>
      <p>未找到合适的结对对象，请调整薄弱板块标签</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { Loading, MagicStick, Connection } from '@element-plus/icons-vue'
import { apiPost } from './common'

const weaknessInput = ref('')
const count = ref(5)
const loading = ref(false)
const result = ref(null)

const tags = computed(() =>
  weaknessInput.value.split(/[,，;；]/).map(s => s.trim()).filter(Boolean)
)

async function match() {
  if (!tags.value.length || loading.value) return
  loading.value = true
  result.value = null
  try {
    result.value = await apiPost('/pair-help/recommend', {
      myWeaknessTags: tags.value,
      count: count.value
    })
  } catch (e) {
    ElMessage.error(e?.message || '匹配失败')
  } finally {
    loading.value = false
  }
}
</script>
