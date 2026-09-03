<template>
  <div class="roadmap-page">
    <h2>我的学习路线图</h2>

    <el-card>
      <el-form :inline="true" :model="form">
        <el-form-item label="学习目标">
          <el-input v-model="form.target" placeholder="如：提升党史理论水平" />
        </el-form-item>
        <el-form-item label="周期(天)">
          <el-input-number v-model="form.periodDays" :min="7" :max="90" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="generate" :loading="loading">生成路线图</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <div v-if="roadmap" class="roadmap-content">
      <el-card style="margin-top: 16px">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="党员">{{ roadmap.memberName }}</el-descriptions-item>
          <el-descriptions-item label="当前水平">{{ roadmap.currentLevel }}</el-descriptions-item>
          <el-descriptions-item label="学习目标">{{ roadmap.target }}</el-descriptions-item>
          <el-descriptions-item label="总周期">{{ roadmap.totalDays }}天</el-descriptions-item>
        </el-descriptions>
      </el-card>

      <el-card v-for="stage in roadmap.stages" :key="stage.stageNo" style="margin-top: 16px">
        <template #header>
          <span>阶段 {{ stage.stageNo }}：{{ stage.stageName }}（{{ stage.durationDays }}天）</span>
        </template>
        <h4>学习目标</h4>
        <ul>
          <li v-for="(obj, i) in stage.objectives" :key="i">{{ obj }}</li>
        </ul>
        <h4>推荐内容</h4>
        <el-list>
          <el-list-item v-for="c in stage.contents" :key="c.contentId">
            <strong>{{ c.title }}</strong>
            <p style="color: #999; font-size: 12px">{{ c.reason }}</p>
          </el-list-item>
        </el-list>
        <h4 v-if="stage.exam">测验要求</h4>
        <p v-if="stage.exam">建议完成 {{ stage.exam.suggestedCount }} 道题，目标分数 {{ stage.exam.targetScore }}分</p>
        <h4>关键指标</h4>
        <el-tag v-for="kpi in stage.kpis" :key="kpi.metric" style="margin-right: 8px">
          {{ kpi.metric }}: {{ kpi.target }}
        </el-tag>
      </el-card>

      <el-alert :title="roadmap.nextAction" type="success" :closable="false" style="margin-top: 16px" />
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { generateRoadmap } from '@/api/feature15'

const loading = ref(false)
const roadmap = ref(null)
const form = reactive({
  target: '提升党建理论水平',
  periodDays: 30
})

async function generate() {
  loading.value = true
  try {
    roadmap.value = await generateRoadmap(form)
    ElMessage.success('路线图生成成功')
  } catch (e) {
    ElMessage.error('生成失败')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.roadmap-page { padding: 16px; }
.roadmap-page h2 { margin-bottom: 16px; }
</style>
