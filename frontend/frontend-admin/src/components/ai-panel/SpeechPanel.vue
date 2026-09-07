<template>
  <div>
    <div class="aip-section-title">AI党建宣讲稿生成</div>
    <p class="aip-section-desc">选择支部并输入主题，AI 将结合该支部<strong>真实党员数据</strong>（党员构成、学习时长、任务完成率、测验成绩、挂机情况、学习先进与需重点关注党员等）自动撰写有针对性的宣讲稿</p>

    <div class="aip-form">
      <div class="aip-field">
        <span class="aip-field-label">宣讲支部</span>
        <el-select v-model="orgId" placeholder="请选择支部" style="width: 220px">
          <el-option v-for="org in orgs" :key="org.id" :label="org.name" :value="org.id" />
        </el-select>
      </div>
      <div class="aip-field">
        <span class="aip-field-label">宣讲主题</span>
        <el-input v-model="topic" placeholder="默认：学习贯彻习近平新时代中国特色社会主义思想" style="width: 340px" />
      </div>
      <div class="aip-field">
        <span class="aip-field-label">目标时长</span>
        <el-select v-model="duration" style="width: 120px">
          <el-option :value="10" label="10分钟" />
          <el-option :value="15" label="15分钟" />
          <el-option :value="20" label="20分钟" />
        </el-select>
      </div>
      <div class="aip-actions">
        <el-button type="primary" :loading="loading" :disabled="!orgId" @click="generate">
          <el-icon v-if="!loading"><MagicStick /></el-icon> 生成宣讲稿
        </el-button>
      </div>
    </div>

    <div v-if="result && result.dataSummary" class="aip-data">
      <div class="aip-data-title">本次宣讲将引用的「{{ result.dataSummary.organizationName }}」党员数据</div>
      <div class="aip-cards">
        <div class="aip-card"><div class="aip-num">{{ result.dataSummary.memberCount }}</div><div class="aip-label">党员总数</div></div>
        <div class="aip-card"><div class="aip-num">{{ result.dataSummary.formalCount }}</div><div class="aip-label">正式党员</div></div>
        <div class="aip-card"><div class="aip-num">{{ result.dataSummary.probationaryCount }}</div><div class="aip-label">预备/其他</div></div>
        <div class="aip-card"><div class="aip-num">{{ result.dataSummary.totalLearningHours }}</div><div class="aip-label">累计学习(小时)</div></div>
        <div v-if="result.dataSummary.taskCompletionRate !== null && result.dataSummary.taskCompletionRate !== undefined" class="aip-card">
          <div class="aip-num">{{ result.dataSummary.taskCompletionRate }}%</div><div class="aip-label">任务完成率</div>
        </div>
        <div v-if="result.dataSummary.avgExamScore !== null && result.dataSummary.avgExamScore !== undefined" class="aip-card">
          <div class="aip-num">{{ result.dataSummary.avgExamScore }}</div><div class="aip-label">测验平均分</div>
        </div>
        <div class="aip-card warn"><div class="aip-num">{{ result.dataSummary.idleCount }}</div><div class="aip-label">挂机人次</div></div>
      </div>
      <div v-if="result.dataSummary.topLearners && result.dataSummary.topLearners.length" class="aip-line">
        <span class="aip-line-tag good">学习先进</span>
        <span>{{ result.dataSummary.topLearners.join('、') }}</span>
      </div>
      <div v-if="result.dataSummary.warningMembers && result.dataSummary.warningMembers.length" class="aip-line">
        <span class="aip-line-tag warn">需重点关注</span>
        <span>{{ result.dataSummary.warningMembers.join('、') }}</span>
      </div>
    </div>

    <div v-if="loading" class="aip-loading">
      <el-icon class="is-loading" :size="32"><Loading /></el-icon>
      <p>AI 正在结合党员数据撰写宣讲稿，请稍候...</p>
    </div>

    <div v-if="result && result.content" class="aip-result">
      <div class="aip-toolbar">
        <div class="aip-result-title">{{ result.content.title }}</div>
        <el-button size="small" @click="copyText(result.content.text)"><el-icon><CopyDocument /></el-icon>复制全文</el-button>
      </div>
      <div class="aip-meta">
        <el-tag size="small">约{{ result.content.estimatedMinutes }}分钟</el-tag>
        <el-tag size="small" type="info">约{{ result.content.wordCount }}字</el-tag>
        <el-tag size="small" type="warning">{{ result.content.targetAudience }}</el-tag>
      </div>

      <div v-if="result.content.keyPoints && result.content.keyPoints.length" class="aip-block">
        <div class="aip-sub-title">核心要点</div>
        <ul class="aip-ul">
          <li v-for="(kp, i) in result.content.keyPoints" :key="i">{{ kp }}</li>
        </ul>
      </div>

      <div class="aip-block">
        <div class="aip-sub-title">宣讲稿正文</div>
        <div class="aip-text">{{ result.content.text }}</div>
      </div>

      <div v-if="result.content.outline && result.content.outline.length" class="aip-block">
        <div class="aip-sub-title">宣讲大纲</div>
        <ol class="aip-ol">
          <li v-for="(o, i) in result.content.outline" :key="i">{{ o }}</li>
        </ol>
      </div>

      <div v-if="result.content.sections && result.content.sections.length" class="aip-block" style="border-bottom:none;">
        <div class="aip-sub-title">分节结构</div>
        <div v-for="(s, i) in result.content.sections" :key="i" class="aip-section-card">
          <div class="aip-sec-head">
            <span class="aip-sec-title">{{ s.heading }}</span>
            <span class="aip-sec-min">{{ s.minutes }}分钟</span>
          </div>
          <div class="aip-sec-content">{{ s.content }}</div>
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
const topic = ref('学习贯彻习近平新时代中国特色社会主义思想')
const duration = ref(15)
const loading = ref(false)
const result = ref(null)

async function generate() {
  if (!orgId.value || loading.value) return
  loading.value = true
  result.value = null
  try {
    result.value = await apiPost('/ai-content/speech-with-data', {
      organizationId: orgId.value,
      topic: topic.value.trim() || undefined,
      durationMinutes: duration.value
    })
    ElMessage.success(result.value?.summary || '宣讲稿生成成功')
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
    ElMessage.success('宣讲稿全文已复制，可直接粘贴使用')
  } catch (e) {
    ElMessage.warning('自动复制失败，请手动选择正文复制')
  }
}

onMounted(async () => { orgs.value = await loadOrgList() })
</script>
