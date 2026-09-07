<template>
  <div>
    <div class="aip-section-title">AI素材生成（题目/知识卡片）</div>
    <p class="aip-section-desc">粘贴党建文章/PDF内容或输入主题，AI 自动生成<strong>单选/多选/判断题与知识卡片</strong>，管理员审核后可复制入库</p>

    <div class="aip-form" style="flex-direction:column;align-items:stretch;">
      <div class="aip-field" style="align-items:flex-start;">
        <span class="aip-field-label" style="padding-top:8px;">源文本</span>
        <el-input
          v-model="sourceText"
          type="textarea"
          :rows="4"
          placeholder="粘贴党建文章、政策文件原文（长度不限），AI 将基于内容生成题目与知识卡片；留空则按主题生成"
          style="flex:1;"
        />
      </div>
      <div style="display:flex;flex-wrap:wrap;gap:12px 20px;align-items:center;">
        <div class="aip-field">
          <span class="aip-field-label">主题（可选）</span>
          <el-input v-model="topic" placeholder="生成内容主题" style="width: 220px" />
        </div>
        <div class="aip-field">
          <span class="aip-field-label">单选</span>
          <el-input-number v-model="singleCount" :min="0" :max="10" style="width: 90px" />
        </div>
        <div class="aip-field">
          <span class="aip-field-label">多选</span>
          <el-input-number v-model="multiCount" :min="0" :max="10" style="width: 90px" />
        </div>
        <div class="aip-field">
          <span class="aip-field-label">判断</span>
          <el-input-number v-model="tfCount" :min="0" :max="10" style="width: 90px" />
        </div>
        <div class="aip-field">
          <el-checkbox v-model="withCards">同时生成知识卡片</el-checkbox>
        </div>
        <div class="aip-actions">
          <el-button type="primary" :loading="loading" :disabled="!sourceText && !topic" @click="generate">
            <el-icon v-if="!loading"><MagicStick /></el-icon> 生成素材
          </el-button>
        </div>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>AI 正在生成题目与知识卡片...</p>
    </div>

    <div v-if="result" class="aip-result">
      <div class="aip-toolbar">
        <div class="aip-result-title">生成结果（{{ (result.questions || []).length }} 题 · {{ (result.flashCards || []).length }} 卡片）</div>
        <el-button size="small" @click="copyAll"><el-icon><CopyDocument /></el-icon>复制全部</el-button>
      </div>

      <div v-if="result.summary" class="aip-meta">
        <el-tag size="small" type="info">{{ result.summary }}</el-tag>
      </div>

      <div v-if="result.questions && result.questions.length" class="aip-block">
        <div class="aip-sub-title">题目</div>
        <div v-for="(q, i) in result.questions" :key="i" class="aip-section-card">
          <div class="aip-sec-head">
            <span class="aip-sec-title">{{ i + 1 }}. {{ q.stem }}</span>
            <el-tag size="small" :type="q.questionType === 0 ? 'primary' : q.questionType === 1 ? 'success' : 'warning'">
              {{ q.questionTypeName || typeName(q.questionType) }}
            </el-tag>
          </div>
          <div class="aip-sec-content">
            <div v-if="q.options && q.options.length">
              <div v-for="(opt, oi) in q.options" :key="oi" style="line-height:1.8;">{{ String.fromCharCode(65 + oi) }}. {{ opt }}</div>
            </div>
            <div style="margin-top:6px;"><strong>答案：</strong>{{ q.correctAnswer }}（{{ q.score }}分）</div>
          </div>
        </div>
      </div>

      <div v-if="result.flashCards && result.flashCards.length" class="aip-block" style="border-bottom:none;">
        <div class="aip-sub-title">知识卡片</div>
        <div v-for="(c, i) in result.flashCards" :key="i" class="aip-section-card">
          <div class="aip-sec-head">
            <span class="aip-sec-title">卡片{{ i + 1 }}{{ c.tag ? ' · ' + c.tag : '' }}</span>
          </div>
          <div class="aip-sec-content">
            <strong>正面：</strong>{{ c.front }}
          </div>
          <div class="aip-sec-content" style="margin-top:4px;">
            <strong>背面：</strong>{{ c.back }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ElMessage } from 'element-plus'
import { Loading, MagicStick, CopyDocument } from '@element-plus/icons-vue'
import { apiPost } from './common'

const sourceText = ref('')
const topic = ref('')
const singleCount = ref(5)
const multiCount = ref(3)
const tfCount = ref(2)
const withCards = ref(true)
const loading = ref(false)
const result = ref(null)

async function generate() {
  if ((!sourceText.value.trim() && !topic.value.trim()) || loading.value) return
  loading.value = true
  result.value = null
  try {
    result.value = await apiPost('/ai-content/generate', {
      contentType: 'questions',
      sourceText: sourceText.value.trim() || undefined,
      topic: topic.value.trim() || undefined,
      singleChoiceCount: singleCount.value,
      multiChoiceCount: multiCount.value,
      trueFalseCount: tfCount.value,
      generateFlashCards: withCards.value
    })
  } catch (e) {
    ElMessage.error(e?.message || '生成失败')
  } finally {
    loading.value = false
  }
}

function typeName(t) {
  const map = { 0: '单选', 1: '多选', 2: '判断' }
  return map[t] || '题目'
}

function copyAll() {
  if (!result.value) return
  const lines = []
  ;(result.value.questions || []).forEach((q, i) => {
    lines.push(`${i + 1}.【${q.questionTypeName || typeName(q.questionType)}】${q.stem}`)
    if (q.options && q.options.length) q.options.forEach((o, oi) => lines.push(`${String.fromCharCode(65 + oi)}. ${o}`))
    lines.push(`答案：${q.correctAnswer}`)
  })
  ;(result.value.flashCards || []).forEach((c, i) => {
    lines.push(`卡片${i + 1}：${c.front} → ${c.back}`)
  })
  navigator.clipboard.writeText(lines.join('\n')).then(
    () => ElMessage.success('已复制全部内容'),
    () => ElMessage.warning('复制失败，请手动选择复制')
  )
}
</script>
