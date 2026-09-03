<template>
  <div class="rectification-page">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>支部整改闭环管理</span>
          <el-button type="primary" @click="openDialog()">新增整改项</el-button>
        </div>
      </template>

      <el-form :inline="true" :model="query">
        <el-form-item label="季度">
          <el-input v-model="query.quarter" placeholder="如 2026Q3" clearable />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="query.status" placeholder="全部" clearable>
            <el-option label="待整改" :value="0" />
            <el-option label="整改中" :value="1" />
            <el-option label="已完成" :value="2" />
            <el-option label="已关闭" :value="3" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">查询</el-button>
        </el-form-item>
      </el-form>

      <el-table :data="list" stripe>
        <el-table-column prop="quarter" label="季度" width="100" />
        <el-table-column prop="issue" label="问题" min-width="200" show-overflow-tooltip />
        <el-table-column prop="suggestion" label="整改建议" min-width="200" show-overflow-tooltip />
        <el-table-column prop="statusName" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)">{{ row.statusName }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button v-if="row.status !== 2" link type="success" @click="complete(row.id)">完成</el-button>
            <el-button v-if="row.status === 0" link type="primary" @click="start(row.id)">开始整改</el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-model:current-page="page"
        v-model:page-size="size"
        :total="total"
        layout="total, prev, pager, next"
        @current-change="loadData"
        style="margin-top: 16px; justify-content: flex-end"
      />
    </el-card>

    <el-dialog v-model="dialogVisible" title="新增整改项" width="500px">
      <el-form :model="form" label-width="80px">
        <el-form-item label="季度" required>
          <el-input v-model="form.quarter" placeholder="如 2026Q3" />
        </el-form-item>
        <el-form-item label="问题" required>
          <el-input v-model="form.issue" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="建议" required>
          <el-input v-model="form.suggestion" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getRectifications, createRectification, completeRectification, updateRectificationStatus } from '@/api/feature15'

const list = ref([])
const total = ref(0)
const page = ref(1)
const size = ref(10)
const query = reactive({ quarter: '', status: null })
const dialogVisible = ref(false)
const form = reactive({ quarter: '', issue: '', suggestion: '' })

onMounted(loadData)

async function loadData() {
  const res = await getRectifications({
    organizationId: 1,
    quarter: query.quarter || undefined,
    status: query.status,
    page: page.value,
    size: size.value
  })
  list.value = res.data || []
  total.value = res.total || 0
}

function openDialog() {
  Object.assign(form, { quarter: '', issue: '', suggestion: '' })
  dialogVisible.value = true
}

async function save() {
  if (!form.quarter || !form.issue || !form.suggestion) {
    ElMessage.warning('请填写完整信息')
    return
  }
  await createRectification(form)
  ElMessage.success('创建成功')
  dialogVisible.value = false
  loadData()
}

async function complete(id) {
  await completeRectification(id, { remark: '整改完成' })
  ElMessage.success('已标记完成')
  loadData()
}

async function start(id) {
  await updateRectificationStatus(id, { status: 1 })
  ElMessage.success('已开始整改')
  loadData()
}

function statusType(s) {
  return s === 0 ? 'warning' : s === 1 ? 'primary' : s === 2 ? 'success' : 'info'
}

function formatDate(d) {
  return d ? new Date(d).toLocaleString() : ''
}
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
