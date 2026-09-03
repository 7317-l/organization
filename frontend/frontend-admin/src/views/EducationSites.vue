<template>
  <div class="education-sites-page">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>红色教育基地管理</span>
          <el-button type="primary" @click="openDialog()">新增基地</el-button>
        </div>
      </template>

      <el-input v-model="keyword" placeholder="搜索基地名称/地址" style="width: 300px; margin-bottom: 16px" clearable @keyup.enter="loadData" />

      <el-table :data="list" stripe>
        <el-table-column prop="name" label="基地名称" min-width="150" />
        <el-table-column prop="address" label="地址" min-width="200" show-overflow-tooltip />
        <el-table-column prop="historicalFacts" label="历史背景" min-width="200" show-overflow-tooltip />
        <el-table-column prop="createdAt" label="创建时间" width="180">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="180">
          <template #default="{ row }">
            <el-button link type="primary" @click="openDialog(row)">编辑</el-button>
            <el-button link type="danger" @click="remove(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-model:current-page="page"
        v-model:page-size="size"
        :total="total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        @size-change="loadData"
        @current-change="loadData"
        style="margin-top: 16px; justify-content: flex-end"
      />
    </el-card>

    <el-dialog v-model="dialogVisible" :title="form.id ? '编辑基地' : '新增基地'" width="600px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="基地名称" required>
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="地址">
          <el-input v-model="form.address" />
        </el-form-item>
        <el-form-item label="简介">
          <el-input v-model="form.description" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="历史背景">
          <el-input v-model="form.historicalFacts" type="textarea" :rows="3" />
        </el-form-item>
        <el-form-item label="AI 解读">
          <el-input v-model="form.aiInterpretation" type="textarea" :rows="3" />
        </el-form-item>
        <el-form-item label="封面图">
          <el-input v-model="form.coverUrl" placeholder="图片URL" />
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
import { ElMessage, ElMessageBox } from 'element-plus'
import { getEducationSites, createEducationSite, updateEducationSite, deleteEducationSite } from '@/api/feature15'

const list = ref([])
const total = ref(0)
const page = ref(1)
const size = ref(10)
const keyword = ref('')
const dialogVisible = ref(false)
const form = reactive({
  id: null,
  name: '',
  address: '',
  description: '',
  historicalFacts: '',
  aiInterpretation: '',
  coverUrl: ''
})

onMounted(loadData)

async function loadData() {
  const res = await getEducationSites({ page: page.value, size: size.value, keyword: keyword.value })
  list.value = res.data || []
  total.value = res.total || 0
}

function openDialog(row) {
  if (row) {
    Object.assign(form, row)
  } else {
    Object.assign(form, { id: null, name: '', address: '', description: '', historicalFacts: '', aiInterpretation: '', coverUrl: '' })
  }
  dialogVisible.value = true
}

async function save() {
  if (!form.name) {
    ElMessage.warning('请填写基地名称')
    return
  }
  if (form.id) {
    await updateEducationSite(form.id, form)
  } else {
    await createEducationSite(form)
  }
  ElMessage.success('保存成功')
  dialogVisible.value = false
  loadData()
}

async function remove(id) {
  await ElMessageBox.confirm('确定删除该基地？', '提示', { type: 'warning' })
  await deleteEducationSite(id)
  ElMessage.success('删除成功')
  loadData()
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
