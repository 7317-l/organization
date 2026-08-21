<template>
  <div class="learning-content-page">
    <el-card shadow="never">
      <el-tabs v-model="activeTab" @tab-change="handleTabChange">
        <!-- Tab1 素材库 -->
        <el-tab-pane label="素材库" name="content">
          <div class="tab-toolbar">
            <el-input v-model="contentQuery.keyword" placeholder="标题关键词" clearable style="width:200px" @clear="loadContents" @keyup.enter="loadContents" />
            <el-tree-select
              v-model="contentQuery.categoryId"
              :data="categoryTree"
              :props="treeProps"
              node-key="id"
              check-strictly
              placeholder="分类筛选"
              clearable
              style="width:200px"
              @change="loadContents"
            />
            <el-select v-model="contentQuery.contentType" placeholder="类型" clearable style="width:120px" @change="loadContents">
              <el-option label="文章" value="article" />
              <el-option label="视频" value="video" />
              <el-option label="音频" value="audio" />
              <el-option label="文档" value="document" />
            </el-select>
            <el-button type="primary" @click="loadContents"><el-icon><Search /></el-icon>查询</el-button>
            <el-button type="success" @click="openContentDialog(null)"><el-icon><Plus /></el-icon>发布内容</el-button>
          </div>

          <el-table :data="contentList" v-loading="contentLoading" border style="width:100%">
            <el-table-column type="index" label="序号" width="60" />
            <el-table-column prop="title" label="标题" min-width="220" show-overflow-tooltip />
            <el-table-column label="类型" width="100">
              <template #default="{ row }">
                <el-tag :type="row.contentType === 'video' ? 'danger' : row.contentType === 'article' ? '' : 'info'" size="small">
                  {{ contentTypeText(row.contentType) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="categoryName" label="分类" width="140" />
            <el-table-column label="状态" width="100">
              <template #default="{ row }">
                <el-tag :type="row.isPublic ? 'success' : 'info'" size="small">
                  {{ row.isPublic ? '已上架' : '已下架' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="createdAt" label="发布时间" width="180">
              <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="220" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="openContentDialog(row)">编辑</el-button>
                <el-button v-if="row.isPublic" link type="warning" size="small" @click="handleOffline(row)">下架</el-button>
                <el-button v-else link type="success" size="small" @click="handleOnline(row)">上架</el-button>
                <el-button link type="danger" size="small" @click="handleDeleteContent(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!contentLoading && contentList.length === 0" description="暂无内容" />
          <el-pagination
            v-if="contentTotal > 0"
            class="pagination"
            background
            layout="total, sizes, prev, pager, next, jumper"
            :total="contentTotal"
            :page-size="contentQuery.size"
            :current-page="contentQuery.page"
            :page-sizes="[10, 20, 50]"
            @size-change="handleContentSizeChange"
            @current-change="handleContentPageChange"
          />
        </el-tab-pane>

        <!-- Tab2 任务派发 -->
        <el-tab-pane label="任务派发" name="task">
          <div class="tab-toolbar">
            <el-input v-model="taskQuery.keyword" placeholder="搜索任务名称" clearable style="width:200px" @clear="loadTasks" @keyup.enter="loadTasks" />
            <el-select v-model="taskQuery.status" placeholder="状态" clearable style="width:140px" @change="loadTasks">
              <el-option label="未开始" value="not_started" />
              <el-option label="进行中" value="in_progress" />
              <el-option label="已截止" value="expired" />
              <el-option label="已完成" value="completed" />
            </el-select>
            <el-button type="primary" @click="loadTasks"><el-icon><Search /></el-icon>查询</el-button>
            <el-button type="success" @click="openTaskDialog(null)"><el-icon><Plus /></el-icon>创建任务</el-button>
            <el-button @click="loadTasks"><el-icon><Refresh /></el-icon>刷新</el-button>
          </div>
          <el-table :data="taskList" v-loading="taskLoading" border style="width:100%">
            <el-table-column type="index" label="序号" width="60" />
            <el-table-column prop="taskName" label="任务名称" min-width="200" show-overflow-tooltip />
            <el-table-column label="绑定素材" width="120">
              <template #default="{ row }">
                {{ row.contentCount || (row.contentIds ? row.contentIds.length : 0) }}项
              </template>
            </el-table-column>
            <el-table-column prop="targetOrgName" label="派发范围" width="160" />
            <el-table-column prop="deadline" label="截止时间" width="180">
              <template #default="{ row }">{{ formatDate(row.deadline) }}</template>
            </el-table-column>
            <el-table-column label="完成率" width="160">
              <template #default="{ row }">
                <el-progress
                  :percentage="row.completionRate || 0"
                  :stroke-width="10"
                  :color="getTaskProgressColor(row.completionRate)"
                />
              </template>
            </el-table-column>
            <el-table-column label="状态" width="110">
              <template #default="{ row }">
                <el-tag :type="getTaskStatusType(row.status)" size="small">
                  {{ getTaskStatusText(row.status, row.deadline) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="200" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="viewTaskCompletion(row)">查看</el-button>
                <el-button link type="warning" size="small" @click="remindTask(row)">催办</el-button>
                <el-button link type="primary" size="small" @click="openTaskDialog(row)">编辑</el-button>
                <el-button link type="danger" size="small" @click="handleDeleteTask(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!taskLoading && taskList.length === 0" description="暂无任务" />
          <el-pagination
            v-if="taskTotal > 0"
            class="pagination"
            background
            layout="total, sizes, prev, pager, next"
            :total="taskTotal"
            :page-size="taskQuery.size"
            :current-page="taskQuery.page"
            :page-sizes="[10, 20, 50]"
            @size-change="handleTaskSizeChange"
            @current-change="handleTaskPageChange"
          />
        </el-tab-pane>

        <!-- Tab3 AI素材生成 -->
        <el-tab-pane label="AI素材生成" name="ai">
          <div class="ai-card">
            <div class="ai-card-icon">🤖✨</div>
            <div class="ai-card-title">AI智能素材生成工具</div>
            <div class="ai-card-desc">上传党建文档 / 报告原文 / 会议纪要，AI自动抽取知识点，生成配套题库、学习卡片、摘要导图</div>
          </div>
          <el-form :model="aiForm" label-width="100px" class="ai-form">
            <el-form-item label="素材类型">
              <el-radio-group v-model="aiForm.type">
                <el-radio value="article">文章</el-radio>
                <el-radio value="video">视频脚本</el-radio>
                <el-radio value="audio">音频稿</el-radio>
                <el-radio value="document">文档</el-radio>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="主题内容">
              <el-input
                v-model="aiForm.content"
                type="textarea"
                :rows="5"
                placeholder="请输入需要生成素材的主题或要点，例如：二十大报告精神学习要点"
              />
            </el-form-item>
            <el-form-item label="参考文档">
              <el-upload :show-file-list="false" :before-upload="handleAiUpload" accept=".doc,.docx,.pdf,.txt">
                <el-button><el-icon><Upload /></el-icon>上传文档</el-button>
              </el-upload>
              <span v-if="aiForm.fileName" class="file-name">{{ aiForm.fileName }}</span>
            </el-form-item>
            <el-form-item>
              <el-button type="primary" :loading="aiGenerating" @click="generateAiContent">
                <el-icon><MagicStick /></el-icon>开始AI生成
              </el-button>
            </el-form-item>
          </el-form>
          <el-card v-if="aiResult" class="ai-result" shadow="never">
            <template #header>
              <div class="card-header">
                <el-icon color="#C8161D"><Document /></el-icon>
                <span>生成结果</span>
                <el-button link type="primary" style="margin-left:auto" @click="copyResult">复制</el-button>
              </div>
            </template>
            <div class="ai-result-text">{{ aiResult }}</div>
          </el-card>
        </el-tab-pane>
      </el-tabs>
    </el-card>

    <!-- 内容发布/编辑弹窗 -->
    <el-dialog v-model="contentDialogVisible" :title="contentForm.id ? '编辑内容' : '发布内容'" width="640px">
      <el-form :model="contentForm" label-width="90px">
        <el-form-item label="标题">
          <el-input v-model="contentForm.title" placeholder="请输入标题" />
        </el-form-item>
        <el-form-item label="内容类型">
          <el-select v-model="contentForm.contentType" placeholder="请选择" style="width:100%">
            <el-option label="文章" value="article" />
            <el-option label="视频" value="video" />
            <el-option label="音频" value="audio" />
            <el-option label="文档" value="document" />
          </el-select>
        </el-form-item>
        <el-form-item label="正文">
          <el-input v-model="contentForm.body" type="textarea" :rows="5" placeholder="请输入正文内容" />
        </el-form-item>
        <el-form-item label="视频链接" v-if="contentForm.contentType === 'video'">
          <el-input v-model="contentForm.videoUrl" placeholder="请输入视频URL" />
        </el-form-item>
        <el-form-item label="分类">
          <el-tree-select
            v-model="contentForm.categoryId"
            :data="categoryTree"
            :props="treeProps"
            node-key="id"
            check-strictly
            placeholder="请选择分类"
            style="width:100%"
          />
        </el-form-item>
        <el-form-item label="标签">
          <el-select v-model="contentForm.tagIds" multiple placeholder="请选择标签" style="width:100%">
            <el-option v-for="t in tagList" :key="t.id" :label="t.name" :value="t.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="是否公开">
          <el-switch v-model="contentForm.isPublic" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="contentDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="contentSubmitting" @click="submitContent">确定</el-button>
      </template>
    </el-dialog>

    <!-- 任务创建/编辑弹窗 -->
    <el-dialog v-model="taskDialogVisible" :title="taskForm.id ? '编辑任务' : '创建任务'" width="560px">
      <el-form :model="taskForm" label-width="100px">
        <el-form-item label="任务名称">
          <el-input v-model="taskForm.taskName" placeholder="如：9月党章专题学习" />
        </el-form-item>
        <el-form-item label="目标组织">
          <el-tree-select
            v-model="taskForm.targetOrgId"
            :data="orgTree"
            :props="treeProps"
            node-key="id"
            check-strictly
            placeholder="请选择目标组织"
            style="width:100%"
          />
        </el-form-item>
        <el-form-item label="截止时间">
          <el-date-picker
            v-model="taskForm.deadline"
            type="datetime"
            placeholder="选择截止时间"
            style="width:100%"
            value-format="YYYY-MM-DD HH:mm:ss"
          />
        </el-form-item>
        <el-form-item label="关联内容">
          <el-select v-model="taskForm.contentIds" multiple filterable placeholder="请选择学习内容" style="width:100%">
            <el-option v-for="c in allContentOptions" :key="c.id" :label="c.title" :value="c.id" />
          </el-select>
          <div class="form-tip">共 {{ allContentOptions.length }} 条可选，已勾选 {{ taskForm.contentIds.length }} 条</div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="taskDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="taskSubmitting" @click="submitTask">确定</el-button>
      </template>
    </el-dialog>

    <!-- 任务完成详情弹窗 -->
    <el-dialog v-model="completionVisible" title="任务完成详情" width="600px">
      <el-table :data="completionList" v-loading="completionLoading" border>
        <el-table-column prop="memberName" label="党员" width="140" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'completed' ? 'success' : 'warning'" size="small">
              {{ row.status === 'completed' ? '已完成' : '未完成' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="completedAt" label="完成时间">
          <template #default="{ row }">{{ formatDate(row.completedAt) }}</template>
        </el-table-column>
        <el-table-column prop="progress" label="进度" width="120">
          <template #default="{ row }">
            <el-progress :percentage="row.progress || 0" :stroke-width="8" />
          </template>
        </el-table-column>
      </el-table>
      <el-empty v-if="!completionLoading && completionList.length === 0" description="暂无数据" />
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getContents, createContent, updateContent, deleteContent,
  getContentCategoriesTree, getContentTags
} from '@/api/content'
import {
  getTasks, createTask, updateTask, deleteTask, getTaskCompletion
} from '@/api/task'
import { getOrganizationTree } from '@/api/organization'
import { aiContentGenerate } from '@/api/ai'
import { formatDate, contentTypeText } from '@/utils/format'

const activeTab = ref('content')
const treeProps = { label: 'name', children: 'children' }

// ========== 素材库 ==========
const contentList = ref([])
const contentLoading = ref(false)
const contentTotal = ref(0)
const contentQuery = reactive({ page: 1, size: 10, keyword: '', categoryId: '', contentType: '' })
const contentDialogVisible = ref(false)
const contentSubmitting = ref(false)
const contentForm = reactive({
  id: null, title: '', body: '', videoUrl: '', contentType: 'article',
  categoryId: null, isPublic: true, tagIds: []
})
const categoryTree = ref([])
const tagList = ref([])
const allContentOptions = ref([])

async function loadCategories() {
  try {
    const res = await getContentCategoriesTree()
    categoryTree.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e) { /* */ }
}
async function loadTags() {
  try {
    const res = await getContentTags()
    tagList.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e) { /* */ }
}

async function loadContents() {
  contentLoading.value = true
  try {
    const params = { page: contentQuery.page, size: contentQuery.size }
    if (contentQuery.keyword) params.keyword = contentQuery.keyword
    if (contentQuery.categoryId) params.categoryId = contentQuery.categoryId
    if (contentQuery.contentType) params.contentType = contentQuery.contentType
    const res = await getContents(params)
    contentList.value = res.items || res.data || []
    contentTotal.value = res.total || 0
    // 同时加载所有内容供任务创建选择
    if (allContentOptions.value.length === 0) {
      const allRes = await getContents({ page: 1, size: 500 })
      allContentOptions.value = allRes.items || allRes.data || []
    }
  } catch (e) { /* */ }
  finally { contentLoading.value = false }
}

function handleContentPageChange(p) { contentQuery.page = p; loadContents() }
function handleContentSizeChange(s) { contentQuery.size = s; contentQuery.page = 1; loadContents() }

function openContentDialog(row) {
  if (row) {
    Object.assign(contentForm, {
      id: row.id, title: row.title, body: row.body, videoUrl: row.videoUrl,
      contentType: row.contentType, categoryId: row.categoryId,
      isPublic: row.isPublic, tagIds: row.tagIds || []
    })
  } else {
    Object.assign(contentForm, {
      id: null, title: '', body: '', videoUrl: '', contentType: 'article',
      categoryId: null, isPublic: true, tagIds: []
    })
  }
  contentDialogVisible.value = true
}

async function submitContent() {
  if (!contentForm.title.trim()) return ElMessage.warning('请输入标题')
  contentSubmitting.value = true
  try {
    const payload = {
      title: contentForm.title, body: contentForm.body, videoUrl: contentForm.videoUrl,
      contentType: contentForm.contentType, categoryId: contentForm.categoryId,
      isPublic: contentForm.isPublic, tagIds: contentForm.tagIds
    }
    if (contentForm.id) {
      await updateContent(contentForm.id, payload)
      ElMessage.success('编辑成功')
    } else {
      await createContent(payload)
      ElMessage.success('发布成功')
    }
    contentDialogVisible.value = false
    loadContents()
  } catch (e) { /* */ }
  finally { contentSubmitting.value = false }
}

async function handleOffline(row) {
  try {
    await ElMessageBox.confirm(`确定下架「${row.title}」吗？`, '提示', { type: 'warning' })
    await updateContent(row.id, { isPublic: false })
    ElMessage.success('已下架')
    loadContents()
  } catch (e) { /* */ }
}

async function handleOnline(row) {
  try {
    await updateContent(row.id, { isPublic: true })
    ElMessage.success('已上架')
    loadContents()
  } catch (e) { /* */ }
}

async function handleDeleteContent(row) {
  try {
    await ElMessageBox.confirm(`确定删除「${row.title}」吗？`, '提示', { type: 'warning' })
    await deleteContent(row.id)
    ElMessage.success('删除成功')
    loadContents()
  } catch (e) { /* */ }
}

// ========== 任务派发 ==========
const taskList = ref([])
const taskLoading = ref(false)
const taskTotal = ref(0)
const taskQuery = reactive({ page: 1, size: 10, keyword: '', status: '' })
const taskDialogVisible = ref(false)
const taskSubmitting = ref(false)
const taskForm = reactive({ id: null, taskName: '', targetOrgId: null, deadline: '', contentIds: [] })
const orgTree = ref([])
const completionVisible = ref(false)
const completionLoading = ref(false)
const completionList = ref([])

async function loadOrgTree() {
  try {
    const res = await getOrganizationTree()
    orgTree.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e) { /* */ }
}

async function loadTasks() {
  taskLoading.value = true
  try {
    const params = { page: taskQuery.page, size: taskQuery.size }
    if (taskQuery.keyword) params.keyword = taskQuery.keyword
    if (taskQuery.status) params.status = taskQuery.status
    const res = await getTasks(params)
    taskList.value = res.items || res.data || []
    taskTotal.value = res.total || 0
  } catch (e) { /* */ }
  finally { taskLoading.value = false }
}

function handleTaskPageChange(p) { taskQuery.page = p; loadTasks() }
function handleTaskSizeChange(s) { taskQuery.size = s; taskQuery.page = 1; loadTasks() }

function getTaskProgressColor(rate) {
  if (rate >= 80) return '#52C41A'
  if (rate >= 50) return '#C8161D'
  return '#E6A23C'
}

function getTaskStatusType(status) {
  const map = {
    not_started: 'info',
    in_progress: 'success',
    expired: 'warning',
    completed: 'success'
  }
  return map[status] || 'info'
}

function getTaskStatusText(status, deadline) {
  if (status === 'completed') return '已完成'
  if (status === 'in_progress') return '进行中'
  if (status === 'not_started') return '未开始'
  if (status === 'expired') return '已截止'
  // 根据截止时间判断
  if (deadline && new Date(deadline) < new Date()) return '已截止'
  return '进行中'
}

function openTaskDialog(row) {
  // 确保有内容选项
  if (allContentOptions.value.length === 0) {
    getContents({ page: 1, size: 500 }).then((res) => {
      allContentOptions.value = res.items || res.data || []
    })
  }
  if (row) {
    Object.assign(taskForm, {
      id: row.id, taskName: row.taskName, targetOrgId: row.targetOrgId,
      deadline: row.deadline, contentIds: row.contentIds || []
    })
  } else {
    Object.assign(taskForm, { id: null, taskName: '', targetOrgId: null, deadline: '', contentIds: [] })
  }
  taskDialogVisible.value = true
}

async function submitTask() {
  if (!taskForm.taskName.trim()) return ElMessage.warning('请输入任务名称')
  if (!taskForm.targetOrgId) return ElMessage.warning('请选择目标组织')
  if (!taskForm.deadline) return ElMessage.warning('请选择截止时间')
  taskSubmitting.value = true
  try {
    const payload = {
      taskName: taskForm.taskName, targetOrgId: taskForm.targetOrgId,
      deadline: taskForm.deadline, contentIds: taskForm.contentIds
    }
    if (taskForm.id) {
      await updateTask(taskForm.id, payload)
      ElMessage.success('编辑成功')
    } else {
      await createTask(payload)
      ElMessage.success('创建成功')
    }
    taskDialogVisible.value = false
    loadTasks()
  } catch (e) { /* */ }
  finally { taskSubmitting.value = false }
}

async function handleDeleteTask(row) {
  try {
    await ElMessageBox.confirm(`确定删除任务「${row.taskName}」吗？`, '提示', { type: 'warning' })
    await deleteTask(row.id)
    ElMessage.success('删除成功')
    loadTasks()
  } catch (e) { /* */ }
}

function remindTask(row) {
  ElMessage.success(`已向任务「${row.taskName}」的未完成人员发送催办通知`)
}

async function viewTaskCompletion(row) {
  completionVisible.value = true
  completionLoading.value = true
  try {
    const res = await getTaskCompletion(row.id)
    completionList.value = res.items || res.data || (Array.isArray(res) ? res : [])
  } catch (e) { /* */ }
  finally { completionLoading.value = false }
}

// ========== AI素材生成 ==========
const aiForm = reactive({ type: 'article', content: '', fileName: '' })
const aiGenerating = ref(false)
const aiResult = ref('')

function handleAiUpload(file) {
  aiForm.fileName = file.name
  return false
}

async function generateAiContent() {
  if (!aiForm.content.trim()) return ElMessage.warning('请输入主题内容')
  aiGenerating.value = true
  try {
    const res = await aiContentGenerate({ content: aiForm.content, type: aiForm.type })
    aiResult.value = res.content || res.result || res.generatedContent || JSON.stringify(res)
    ElMessage.success('生成成功')
  } catch (e) { /* */ }
  finally { aiGenerating.value = false }
}

function copyResult() {
  navigator.clipboard.writeText(aiResult.value)
  ElMessage.success('已复制')
}

function handleTabChange(name) {
  if (name === 'task' && taskList.value.length === 0) loadTasks()
}

onMounted(() => {
  loadCategories()
  loadTags()
  loadOrgTree()
  loadContents()
})
</script>

<style scoped>
.learning-content-page { padding: 0; }
.tab-toolbar { display: flex; gap: 10px; margin-bottom: 16px; flex-wrap: wrap; align-items: center; }
.pagination { margin-top: 16px; justify-content: flex-end; display: flex; }
.form-tip { color: #909399; font-size: 12px; margin-top: 4px; }

/* AI卡片 */
.ai-card {
  background: linear-gradient(135deg, #fff7e6 0%, #fff1f0 100%);
  border: 1px dashed #ffd591;
  border-radius: 8px;
  padding: 28px;
  text-align: center;
  margin-bottom: 20px;
}
.ai-card-icon { font-size: 40px; margin-bottom: 12px; }
.ai-card-title { font-size: 17px; font-weight: 600; margin-bottom: 8px; color: #303133; }
.ai-card-desc { color: #8c8c8c; font-size: 13px; line-height: 1.6; }

.ai-form { max-width: 700px; margin: 0 auto; }
.file-name { margin-left: 12px; color: #67c23a; font-size: 13px; }
.ai-result { margin-top: 16px; }
.card-header { display: flex; align-items: center; gap: 8px; font-weight: 600; }
.ai-result-text { white-space: pre-wrap; line-height: 1.8; color: #303133; max-height: 400px; overflow-y: auto; }
</style>
