<template>
  <div class="meeting-brief-page">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>三会一课简报生成</span>
          <el-button type="primary" @click="generate" :loading="loading">生成简报</el-button>
        </div>
      </template>

      <el-form :inline="true" :model="form">
        <el-form-item label="开始日期">
          <el-date-picker v-model="form.startDate" type="date" value-format="YYYY-MM-DD" />
        </el-form-item>
        <el-form-item label="结束日期">
          <el-date-picker v-model="form.endDate" type="date" value-format="YYYY-MM-DD" />
        </el-form-item>
        <el-form-item label="活动类型">
          <el-select v-model="form.type" placeholder="全部类型" clearable>
            <el-option label="支部党员大会" :value="0" />
            <el-option label="支部委员会" :value="1" />
            <el-option label="党小组会" :value="2" />
            <el-option label="党课" :value="3" />
            <el-option label="主题党日" :value="4" />
          </el-select>
        </el-form-item>
      </el-form>

      <div v-if="result" class="brief-content">
        <el-descriptions :column="3" border>
          <el-descriptions-item label="活动总数">{{ result.activityCount }}</el-descriptions-item>
          <el-descriptions-item label="心得总数">{{ result.totalHearts }}</el-descriptions-item>
          <el-descriptions-item label="参与率">{{ result.attendanceRate }}%</el-descriptions-item>
        </el-descriptions>

        <el-divider content-position="left">类型分布</el-divider>
        <el-row :gutter="16">
          <el-col :span="6" v-for="t in result.typeBreakdown" :key="t.type">
            <el-card shadow="hover">
              <div class="type-card">
                <div class="type-name">{{ t.typeName }}</div>
                <div class="type-count">{{ t.count }} 次</div>
              </div>
            </el-card>
          </el-col>
        </el-row>

        <el-divider content-position="left">简报正文</el-divider>
        <div class="brief-text">{{ result.brief }}</div>

        <el-divider content-position="left">核心要点</el-divider>
        <el-timeline>
          <el-timeline-item v-for="(p, i) in result.keyPoints" :key="i" :timestamp="`要点 ${i + 1}`">
            {{ p }}
          </el-timeline-item>
        </el-timeline>
      </div>

      <el-empty v-else description="选择日期范围后点击生成简报" />
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { generateMeetingBrief } from '@/api/feature15'

const loading = ref(false)
const result = ref(null)
const form = reactive({
  startDate: new Date(new Date().setDate(1)).toISOString().slice(0, 10),
  endDate: new Date().toISOString().slice(0, 10),
  type: null
})

async function generate() {
  if (!form.startDate || !form.endDate) {
    ElMessage.warning('请选择日期范围')
    return
  }
  loading.value = true
  try {
    result.value = await generateMeetingBrief({
      ...form,
      startDate: new Date(form.startDate),
      endDate: new Date(form.endDate)
    })
    ElMessage.success('简报生成成功')
  } catch (e) {
    ElMessage.error('生成失败')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.type-card {
  text-align: center;
  padding: 10px 0;
}
.type-name {
  font-size: 14px;
  color: #666;
}
.type-count {
  font-size: 24px;
  font-weight: bold;
  color: #409eff;
  margin-top: 8px;
}
.brief-text {
  background: #f5f7fa;
  padding: 20px;
  border-radius: 8px;
  line-height: 1.8;
  color: #333;
}
</style>
