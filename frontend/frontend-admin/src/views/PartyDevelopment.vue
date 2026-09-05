<template>
  <div class="party-development-page">
    <div class="page-header">
      <h2>党员发展台账</h2>
      <el-button type="primary" @click="openCreateDialog">
        <el-icon><Plus /></el-icon>新建发展记录
      </el-button>
    </div>

    <!-- 筛选栏 -->
    <div class="filter-bar">
      <el-input v-model="query.keyword" placeholder="搜索姓名/支部" clearable style="width:200px" @clear="loadList" @keyup.enter="loadList" />
      <el-select v-model="query.stage" placeholder="发展阶段" clearable style="width:160px" @change="loadList">
        <el-option label="入党积极分子" :value="1" />
        <el-option label="发展对象" :value="2" />
        <el-option label="预备党员" :value="3" />
        <el-option label="正式党员" :value="4" />
      </el-select>
      <el-select v-model="query.status" placeholder="状态" clearable style="width:140px" @change="loadList">
        <el-option label="草稿" :value="0" />
        <el-option label="待审核" :value="1" />
        <el-option label="已通过" :value="2" />
        <el-option label="已驳回" :value="3" />
      </el-select>
      <el-button @click="loadList">查询</el-button>
    </div>

    <!-- 列表 -->
    <el-table :data="list" v-loading="loading" border style="width:100%">
      <el-table-column prop="memberName" label="姓名" width="100" />
      <el-table-column prop="organizationName" label="所属支部" min-width="150" show-overflow-tooltip />
      <el-table-column prop="stageName" label="发展阶段" width="110" />
      <el-table-column prop="statusName" label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="statusTagType(row.status)">{{ row.statusName }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="submitDate" label="提交时间" width="170" />
      <el-table-column prop="reviewerName" label="审核人" width="100" />
      <el-table-column label="操作" width="380" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="viewDetail(row)">详情</el-button>
          <el-button v-if="row.status === 0" link type="warning" size="small" @click="handleSubmit(row)">提交审核</el-button>
          <el-button v-if="row.status === 1" link type="success" size="small" @click="openReviewDialog(row)">审核</el-button>
          <el-button v-if="row.status === 2 && row.stage < 4" link type="primary" size="small" @click="handleAdvance(row)">阶段推进</el-button>
          <el-button link type="info" size="small" @click="handleAiCheck(row)">AI校验</el-button>
          <el-button link type="danger" size="small" @click="handleMaterialCheck(row)">材料检查</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-pagination
      v-if="total > 0"
      :current-page="query.page"
      :page-size="query.size"
      :total="total"
      layout="total, prev, pager, next"
      @current-change="handlePageChange"
      style="margin-top:16px; justify-content:flex-end"
    />

    <!-- 创建弹窗 -->
    <el-dialog v-model="createVisible" title="新建党员发展记录" width="560px">
      <el-form :model="createForm" label-width="100px">
        <el-form-item label="党员">
          <el-select v-model="createForm.memberId" filterable placeholder="选择党员" style="width:100%">
            <el-option v-for="m in memberOptions" :key="m.id" :label="m.name" :value="m.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="发展阶段">
          <el-select v-model="createForm.stage" style="width:100%">
            <el-option label="入党积极分子" :value="1" />
            <el-option label="发展对象" :value="2" />
            <el-option label="预备党员" :value="3" />
            <el-option label="正式党员" :value="4" />
          </el-select>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="createForm.remark" type="textarea" :rows="3" placeholder="备注信息" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleCreate">确定</el-button>
      </template>
    </el-dialog>

    <!-- 审核弹窗 -->
    <el-dialog v-model="reviewVisible" title="审核发展记录" width="500px">
      <el-form :model="reviewForm" label-width="100px">
        <el-form-item label="审核结果">
          <el-radio-group v-model="reviewForm.approved">
            <el-radio :value="true">通过</el-radio>
            <el-radio :value="false">驳回</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="审核意见">
          <el-input v-model="reviewForm.comment" type="textarea" :rows="3" placeholder="请输入审核意见" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="reviewVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleReview">提交审核</el-button>
      </template>
    </el-dialog>

    <!-- 详情弹窗 -->
    <el-dialog v-model="detailVisible" title="发展记录详情" width="600px">
      <el-descriptions :column="2" border v-if="currentRow">
        <el-descriptions-item label="姓名">{{ currentRow.memberName }}</el-descriptions-item>
        <el-descriptions-item label="所属支部">{{ currentRow.organizationName }}</el-descriptions-item>
        <el-descriptions-item label="发展阶段">{{ currentRow.stageName }}</el-descriptions-item>
        <el-descriptions-item label="状态">{{ currentRow.statusName }}</el-descriptions-item>
        <el-descriptions-item label="提交时间">{{ currentRow.submitDate }}</el-descriptions-item>
        <el-descriptions-item label="审核人">{{ currentRow.reviewerName || '-' }}</el-descriptions-item>
        <el-descriptions-item label="审核意见" :span="2">{{ currentRow.reviewComment || '-' }}</el-descriptions-item>
        <el-descriptions-item label="备注" :span="2">{{ currentRow.remark || '-' }}</el-descriptions-item>
      </el-descriptions>
    </el-dialog>

    <!-- AI校验结果弹窗 -->
    <el-dialog v-model="aiCheckVisible" title="AI材料校验结果" width="600px">
      <div v-loading="aiChecking">
        <pre style="white-space:pre-wrap; font-family:inherit; margin:0">{{ aiCheckResult }}</pre>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import {
  getPartyDevelopmentList, createPartyDevelopment,
  submitPartyDevelopment, reviewPartyDevelopment,
  advancePartyDevelopment, aiCheckPartyDevelopment
} from '@/api/partyDevelopment'
import request from '@/api/request'

const loading = ref(false)
const submitting = ref(false)
const list = ref([])
const total = ref(0)
const query = reactive({ page: 1, size: 10, keyword: '', stage: null, status: null })

const createVisible = ref(false)
const reviewVisible = ref(false)
const detailVisible = ref(false)
const aiCheckVisible = ref(false)
const aiChecking = ref(false)
const aiCheckResult = ref('')
const currentRow = ref(null)
const memberOptions = ref([])

const createForm = reactive({ memberId: null, stage: 1, remark: '' })
const reviewForm = reactive({ approved: true, comment: '' })

const stageNames = { 1: '入党积极分子', 2: '发展对象', 3: '预备党员', 4: '正式党员' }
const statusNames = { 0: '草稿', 1: '待审核', 2: '已通过', 3: '已驳回' }

function statusTagType(status) {
  return status === 2 ? 'success' : status === 1 ? 'warning' : status === 3 ? 'danger' : 'info'
}

async function loadList() {
  loading.value = true
  try {
    const params = { page: query.page, size: query.size }
    if (query.keyword) params.keyword = query.keyword
    if (query.stage !== null) params.stage = query.stage
    if (query.status !== null) params.status = query.status
    const res = await getPartyDevelopmentList(params)
    list.value = (res.items || res.data || []).map(item => ({
      ...item,
      stageName: stageNames[item.stage] || item.stageName || '未知',
      statusName: statusNames[item.status] || item.statusName || '未知'
    }))
    total.value = res.total || 0
  } catch (e) {
    ElMessage.error('加载失败')
  } finally {
    loading.value = false
  }
}

async function loadMembers() {
  try {
    const res = await request.get('/party-members', { params: { page: 1, size: 500 } })
    memberOptions.value = res.items || res.data || []
  } catch (e) { /* */ }
}

function handlePageChange(p) { query.page = p; loadList() }

function openCreateDialog() {
  Object.assign(createForm, { memberId: null, stage: 1, remark: '' })
  createVisible.value = true
}

async function handleCreate() {
  if (!createForm.memberId) return ElMessage.warning('请选择党员')
  submitting.value = true
  try {
    await createPartyDevelopment(createForm)
    ElMessage.success('创建成功')
    createVisible.value = false
    loadList()
  } catch (e) { /* */ }
  finally { submitting.value = false }
}

async function handleSubmit(row) {
  try {
    await ElMessageBox.confirm(`确定提交「${row.memberName}」的发展记录审核吗？`, '提示', { type: 'warning' })
    await submitPartyDevelopment(row.id, {})
    ElMessage.success('已提交审核')
    loadList()
  } catch (e) { /* */ }
}

function openReviewDialog(row) {
  currentRow.value = row
  Object.assign(reviewForm, { approved: true, comment: '' })
  reviewVisible.value = true
}

async function handleReview() {
  submitting.value = true
  try {
    await reviewPartyDevelopment(currentRow.value.id, reviewForm)
    ElMessage.success(reviewForm.approved ? '审核通过' : '已驳回')
    reviewVisible.value = false
    loadList()
  } catch (e) { /* */ }
  finally { submitting.value = false }
}

async function handleAdvance(row) {
  try {
    await ElMessageBox.confirm(`确定将「${row.memberName}」推进到下一阶段吗？`, '提示', { type: 'warning' })
    await advancePartyDevelopment(row.id)
    ElMessage.success('阶段已推进')
    loadList()
  } catch (e) { /* */ }
}

async function handleAiCheck(row) {
  aiCheckVisible.value = true
  aiChecking.value = true
  aiCheckResult.value = ''
  try {
    const res = await aiCheckPartyDevelopment(row.id)
    aiCheckResult.value = JSON.stringify(res.data || res, null, 2)
  } catch (e) {
    aiCheckResult.value = 'AI校验失败'
  } finally {
    aiChecking.value = false
  }
}

async function handleMaterialCheck(row) {
  try {
    const res = await request.post(`/party-development/${row.id}/material-check`, { materials: [] })
    ElMessageBox.alert(JSON.stringify(res.data || res, null, 2), '材料检查结果', { dangerouslyUseHTMLString: false })
  } catch (e) { /* */ }
}

function viewDetail(row) {
  currentRow.value = row
  detailVisible.value = true
}

onMounted(() => {
  loadList()
  loadMembers()
})
</script>

<style scoped>
.party-development-page { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { margin: 0; font-size: 20px; }
.filter-bar { display: flex; gap: 10px; margin-bottom: 16px; flex-wrap: wrap; }
</style>
