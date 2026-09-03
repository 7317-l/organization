<template>
  <div class="pair-help-page">
    <h2>薄弱点互助</h2>

    <el-tabs v-model="activeTab">
      <el-tab-pane label="推荐帮扶" name="recommend">
        <el-button type="primary" @click="loadRecommend" :loading="loading">获取推荐</el-button>
        <el-card v-for="r in recommendations" :key="r.memberId" style="margin-top: 12px">
          <div class="rec-item">
            <div>
              <strong>{{ r.memberName }}</strong>（{{ r.organizationName }}）
              <p>匹配度：{{ r.score }}</p>
              <p>{{ r.matchReason }}</p>
            </div>
            <el-button type="primary" @click="requestHelp(r.memberId)">申请结对</el-button>
          </div>
        </el-card>
      </el-tab-pane>

      <el-tab-pane label="我的结对" name="my">
        <el-button @click="loadMy">刷新</el-button>
        <h3>进行中</h3>
        <el-card v-for="p in myPairs.active" :key="p.recordId" style="margin: 8px 0">
          <p>搭档：{{ p.partnerName }}（{{ p.role === 'helper' ? '我是帮扶人' : '我是受助者' }}）</p>
          <p>开始时间：{{ formatDate(p.startTime) }}</p>
          <el-input v-model="logContent" placeholder="记录帮扶内容" style="margin: 8px 0" />
          <el-button size="small" @click="logHelp(p.recordId)">记录</el-button>
          <el-button size="small" type="success" @click="complete(p.recordId)">完成结对</el-button>
        </el-card>
        <el-empty v-if="myPairs.active.length === 0" description="暂无进行中的结对" />

        <h3 style="margin-top: 20px">历史记录</h3>
        <el-card v-for="p in myPairs.history" :key="p.recordId" style="margin: 8px 0">
          <p>搭档：{{ p.partnerName }}</p>
          <p>开始时间：{{ formatDate(p.startTime) }}</p>
        </el-card>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { recommendPairHelp, requestPairHelp, getMyPairHelp, logPairHelp, completePairHelp } from '@/api/feature15'

const activeTab = ref('recommend')
const loading = ref(false)
const recommendations = ref([])
const myPairs = reactive({ active: [], history: [] })
const logContent = ref('')

async function loadRecommend() {
  loading.value = true
  try {
    const res = await recommendPairHelp({ count: 5 })
    recommendations.value = res.recommendations || []
  } catch (e) {
    ElMessage.error('获取推荐失败')
  } finally {
    loading.value = false
  }
}

async function requestHelp(helperId) {
  await requestPairHelp({ helperId })
  ElMessage.success('结对申请已发送')
}

async function loadMy() {
  const res = await getMyPairHelp()
  myPairs.active = res.active || []
  myPairs.history = res.history || []
}

async function logHelp(recordId) {
  if (!logContent.value) return
  await logPairHelp(recordId, { content: logContent.value })
  ElMessage.success('记录成功')
  logContent.value = ''
}

async function complete(recordId) {
  await completePairHelp(recordId, { outcomeSummary: '结对完成' })
  ElMessage.success('结对已完成')
  loadMy()
}

function formatDate(d) {
  return d ? new Date(d).toLocaleString() : ''
}
</script>

<style scoped>
.pair-help-page { padding: 16px; }
.rec-item { display: flex; justify-content: space-between; align-items: center; }
</style>
