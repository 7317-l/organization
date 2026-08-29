<template>
  <div class="exam-management-page">
    <el-card shadow="never">
      <el-tabs v-model="activeTab" @tab-change="handleTabChange">
        <!-- Tab1 题库管理 -->
        <el-tab-pane label="题库管理" name="question">
          <div class="tab-toolbar">
            <el-input v-model="questionQuery.keyword" placeholder="搜索题目内容" clearable style="width:220px" @clear="loadQuestions" @keyup.enter="loadQuestions" />
            <el-select v-model="questionQuery.type" placeholder="题目类型" clearable style="width:140px" @change="loadQuestions">
              <el-option label="单选题" :value="0" />
              <el-option label="多选题" :value="1" />
              <el-option label="判断题" :value="2" />
            </el-select>
            <el-select v-model="questionQuery.categoryId" placeholder="分类" clearable style="width:160px" @change="loadQuestions">
              <el-option v-for="c in questionCategories" :key="c.id" :label="c.name" :value="c.id" />
            </el-select>
            <el-button type="primary" @click="loadQuestions"><el-icon><Search /></el-icon>查询</el-button>
            <el-button type="success" @click="openQuestionDialog(null)"><el-icon><Plus /></el-icon>新增题目</el-button>
            <el-upload :show-file-list="false" :before-upload="handleImportQuestions" accept=".xlsx,.xls">
              <el-button><el-icon><Upload /></el-icon>批量导入</el-button>
            </el-upload>
            <el-button @click="handleExportQuestions"><el-icon><Download /></el-icon>导出</el-button>
          </div>

          <el-table :data="questionList" v-loading="questionLoading" border style="width:100%">
            <el-table-column type="index" label="序号" width="60" />
            <el-table-column prop="stem" label="题目内容" min-width="320" show-overflow-tooltip />
            <el-table-column label="类型" width="100">
              <template #default="{ row }">
                <el-tag :type="row.questionType === 1 ? 'danger' : row.questionType === 2 ? 'success' : ''" size="small">
                  {{ questionTypeText(row.questionType) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="score" label="分值" width="80" />
            <el-table-column prop="categoryName" label="分类" width="140" />
            <el-table-column prop="updatedAt" label="更新时间" width="180">
              <template #default="{ row }">{{ formatDate(row.updatedAt || row.createdAt) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="160" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="openQuestionDialog(row)">编辑</el-button>
                <el-button link type="danger" size="small" @click="handleDeleteQuestion(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!questionLoading && questionList.length === 0" description="暂无题目" />
          <el-pagination
            v-if="questionTotal > 0"
            class="pagination"
            background
            layout="total, sizes, prev, pager, next, jumper"
            :total="questionTotal"
            :page-size="questionQuery.size"
            :current-page="questionQuery.page"
            :page-sizes="[10, 20, 50]"
            @size-change="handleQuestionSizeChange"
            @current-change="handleQuestionPageChange"
          />
        </el-tab-pane>

        <!-- Tab2 试卷组卷 -->
        <el-tab-pane label="试卷组卷" name="paper">
          <div class="tab-toolbar">
            <el-input v-model="paperQuery.keyword" placeholder="搜索试卷名" clearable style="width:200px" @clear="loadPapers" @keyup.enter="loadPapers" />
            <el-button type="primary" @click="loadPapers"><el-icon><Search /></el-icon>查询</el-button>
            <el-button type="success" @click="openPaperDialog(null)"><el-icon><Plus /></el-icon>创建试卷</el-button>
            <el-button @click="loadPapers"><el-icon><Refresh /></el-icon>刷新</el-button>
          </div>
          <el-table :data="paperList" v-loading="paperLoading" border style="width:100%">
            <el-table-column type="index" label="序号" width="60" />
            <el-table-column prop="name" label="试卷名称" min-width="220" show-overflow-tooltip />
            <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
            <el-table-column label="题目数" width="100">
              <template #default="{ row }">
                {{ row.questionCount || (row.questionIds ? row.questionIds.length : 0) }}题
              </template>
            </el-table-column>
            <el-table-column prop="totalScore" label="总分" width="100" />
            <el-table-column prop="createdAt" label="创建时间" width="180">
              <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="280" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="openPaperDialog(row)">编辑</el-button>
                <el-button link type="success" size="small" @click="openTestDialog(row)">发布测验</el-button>
                <el-button link type="primary" size="small" @click="previewPaper(row)">预览</el-button>
                <el-button link type="danger" size="small" @click="handleDeletePaper(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!paperLoading && paperList.length === 0" description="暂无试卷" />
          <el-pagination
            v-if="paperTotal > 0"
            class="pagination"
            background
            layout="total, prev, pager, next"
            :total="paperTotal"
            :page-size="paperQuery.size"
            :current-page="paperQuery.page"
            @current-change="handlePaperPageChange"
          />
        </el-tab-pane>

        <!-- Tab3 测验记录 -->
        <el-tab-pane label="测验记录" name="test">
          <div class="tab-toolbar">
            <el-select v-model="testQuery.paperId" placeholder="选择测验" clearable style="width:220px" @change="loadTests">
              <el-option v-for="p in paperList" :key="p.id" :label="p.name" :value="p.id" />
            </el-select>
            <el-button type="primary" @click="loadTests"><el-icon><Search /></el-icon>查询</el-button>
            <el-button type="success" @click="handleExportResults"><el-icon><Download /></el-icon>导出成绩Excel</el-button>
            <el-button @click="loadTests"><el-icon><Refresh /></el-icon>刷新</el-button>
          </div>
          <el-table :data="testList" v-loading="testLoading" border style="width:100%">
            <el-table-column type="index" label="序号" width="60" />
            <el-table-column prop="memberName" label="姓名" width="120" />
            <el-table-column prop="organizationName" label="支部" width="160" />
            <el-table-column prop="paperName" label="测验名称" min-width="200" show-overflow-tooltip />
            <el-table-column label="得分" width="100">
              <template #default="{ row }">
                <span :style="{ color: getScoreColor(row.score, row.totalScore), fontWeight: 600 }">
                  {{ row.score }}
                </span>
                <span style="color:#909399"> / {{ row.totalScore }}</span>
              </template>
            </el-table-column>
            <el-table-column prop="timeUsed" label="用时" width="100">
              <template #default="{ row }">{{ row.timeUsedMinutes || row.duration || '-' }}{{ row.timeUsedMinutes || row.duration ? '分' : '' }}</template>
            </el-table-column>
            <el-table-column prop="submitTime" label="提交时间" width="180">
              <template #default="{ row }">{{ formatDate(row.submitTime || row.submittedAt || row.createdAt) }}</template>
            </el-table-column>
            <el-table-column label="结果" width="100">
              <template #default="{ row }">
                <el-tag :type="getResultType(row.score, row.totalScore, row.passScore)" size="small">
                  {{ getResultText(row.score, row.totalScore, row.passScore) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="140" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="viewResultDetail(row)">详情</el-button>
                <el-button link type="warning" size="small" @click="remindRetake(row)">补考</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!testLoading && testList.length === 0" description="暂无测验记录" />
          <el-pagination
            v-if="testTotal > 0"
            class="pagination"
            background
            layout="total, sizes, prev, pager, next"
            :total="testTotal"
            :page-size="testQuery.size"
            :current-page="testQuery.page"
            :page-sizes="[10, 20, 50, 100]"
            @size-change="handleTestSizeChange"
            @current-change="handleTestPageChange"
          />
        </el-tab-pane>
      </el-tabs>
    </el-card>

    <!-- 题目新增/编辑弹窗 -->
    <el-dialog v-model="questionDialogVisible" :title="questionForm.id ? '编辑题目' : '新增题目'" width="640px">
      <el-form :model="questionForm" label-width="90px">
        <el-form-item label="题目类型">
          <el-select v-model="questionForm.questionType" placeholder="请选择" style="width:100%">
            <el-option label="单选题" :value="0" />
            <el-option label="多选题" :value="1" />
            <el-option label="判断题" :value="2" />
          </el-select>
        </el-form-item>
        <el-form-item label="分类">
          <el-select v-model="questionForm.categoryId" placeholder="请选择分类" style="width:100%">
            <el-option v-for="c in questionCategories" :key="c.id" :label="c.name" :value="c.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="题干">
          <el-input v-model="questionForm.stem" type="textarea" :rows="3" placeholder="请输入题干" />
        </el-form-item>
        <el-form-item label="选项" v-if="[0, 1].includes(questionForm.questionType)">
          <div v-for="(opt, idx) in questionForm.options" :key="idx" class="option-row">
            <el-input v-model="opt.label" style="width:60px" placeholder="A" />
            <el-input v-model="opt.text" style="flex:1;margin-left:8px" placeholder="选项内容" />
            <el-button link type="danger" @click="questionForm.options.splice(idx, 1)">删除</el-button>
          </div>
          <el-button size="small" @click="questionForm.options.push({ label: '', text: '' })">添加选项</el-button>
        </el-form-item>
        <el-form-item label="正确答案">
          <el-input v-model="questionForm.correctAnswer" placeholder="如 A 或 A,B 或 正确" />
        </el-form-item>
        <el-form-item label="分值">
          <el-input-number v-model="questionForm.score" :min="1" :max="100" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="questionDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="questionSubmitting" @click="submitQuestion">确定</el-button>
      </template>
    </el-dialog>

    <!-- 试卷创建/编辑弹窗 -->
    <el-dialog v-model="paperDialogVisible" :title="paperForm.id ? '编辑试卷' : '创建试卷'" width="640px">
      <el-form :model="paperForm" label-width="90px">
        <el-form-item label="试卷名称">
          <el-input v-model="paperForm.name" placeholder="请输入试卷名称" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="paperForm.description" type="textarea" :rows="2" placeholder="请输入描述" />
        </el-form-item>
        <el-form-item label="选择题目">
          <el-select v-model="paperForm.questionIds" multiple filterable placeholder="请选择题目" style="width:100%">
            <el-option v-for="q in allQuestions" :key="q.id" :label="q.stem" :value="q.id" />
          </el-select>
          <div class="form-tip">共 {{ allQuestions.length }} 题可选，已选 {{ paperForm.questionIds.length }} 题</div>
        </el-form-item>
        <el-form-item label="总分">
          <el-input-number v-model="paperForm.totalScore" :min="1" :max="500" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="paperDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="paperSubmitting" @click="submitPaper">确定</el-button>
      </template>
    </el-dialog>

    <!-- 发布测验弹窗 -->
    <el-dialog v-model="testDialogVisible" title="发布测验" width="520px">
      <el-form :model="testForm" label-width="110px">
        <el-form-item label="试卷">
          <el-input :value="testForm.paperName" disabled />
        </el-form-item>
        <el-form-item label="目标组织">
          <el-tree-select
            v-model="testForm.targetOrgId"
            :data="orgTree"
            :props="{ label: 'name', children: 'children' }"
            node-key="id"
            check-strictly
            placeholder="请选择目标组织"
            style="width:100%"
          />
        </el-form-item>
        <el-form-item label="限时(分钟)">
          <el-input-number v-model="testForm.timeLimitMinutes" :min="5" :max="300" />
        </el-form-item>
        <el-form-item label="截止时间">
          <el-date-picker
            v-model="testForm.deadline"
            type="datetime"
            placeholder="选择截止时间"
            style="width:100%"
            value-format="YYYY-MM-DD HH:mm:ss"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="testDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="testSubmitting" @click="submitTest">发布</el-button>
      </template>
    </el-dialog>

    <!-- 成绩详情弹窗 -->
    <el-dialog v-model="resultsVisible" title="测验成绩详情" width="700px">
      <div v-loading="resultsLoading">
        <div v-if="currentResult" class="result-header">
          <span><b>姓名：</b>{{ currentResult.memberName }}</span>
          <span><b>测验：</b>{{ currentResult.paperName }}</span>
          <span><b>得分：</b><span :style="{color: getScoreColor(currentResult.score, currentResult.totalScore), fontWeight:600}">{{ currentResult.score }}</span> / {{ currentResult.totalScore }}</span>
        </div>
        <el-table :data="answerList" border>
          <el-table-column type="index" label="题号" width="60" />
          <el-table-column prop="stem" label="题目" min-width="250" show-overflow-tooltip />
          <el-table-column prop="userAnswer" label="考生答案" width="120" />
          <el-table-column prop="correctAnswer" label="正确答案" width="120" />
          <el-table-column label="得分" width="80">
            <template #default="{ row }">
              <span :style="{color: row.score > 0 ? '#52C41A' : '#F5222D'}">{{ row.score }}</span>
            </template>
          </el-table-column>
        </el-table>
        <el-empty v-if="!resultsLoading && answerList.length === 0" description="暂无答题详情" :image-size="80" />
      </div>
      <template #footer>
        <el-button @click="resultsVisible = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 试卷预览弹窗 -->
    <el-dialog v-model="previewVisible" title="试卷预览" width="640px">
      <div v-loading="previewLoading">
        <h3 style="text-align:center;margin-bottom:16px">{{ previewPaperData?.name }}</h3>
        <p style="color:#909399;text-align:center;margin-bottom:20px">
          总分：{{ previewPaperData?.totalScore }}分 | 题目数：{{ previewQuestions.length }}题
        </p>
        <div v-for="(q, idx) in previewQuestions" :key="idx" class="preview-question">
          <div class="q-title">{{ idx + 1 }}. ({{ q.score }}分) {{ q.stem }}</div>
          <div v-if="q.options && q.options.length" class="q-options">
            <div v-for="(opt, oi) in q.options" :key="oi" class="q-opt">{{ typeof opt === 'string' ? String.fromCharCode(65 + oi) + '. ' + opt : (opt.label || String.fromCharCode(65 + oi)) + '. ' + (opt.text || '') }}</div>
          </div>
        </div>
        <el-empty v-if="!previewLoading && previewQuestions.length === 0" description="暂无题目" :image-size="80" />
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getQuestions, createQuestion, updateQuestion, deleteQuestion,
  importQuestions, getQuestionCategories
} from '@/api/question'
import {
  getExamPapers, createExamPaper, updateExamPaper, deleteExamPaper,
  getExamTests, createExamTest, getExamTestResults
} from '@/api/exam'
import { getOrganizationTree } from '@/api/organization'
import { formatDate, questionTypeText } from '@/utils/format'

const activeTab = ref('question')

// ========== 题库 ==========
const questionList = ref([])
const questionLoading = ref(false)
const questionTotal = ref(0)
const questionQuery = reactive({ page: 1, size: 10, keyword: '', type: '', categoryId: '' })
const questionDialogVisible = ref(false)
const questionSubmitting = ref(false)
const questionForm = reactive({
  id: null, questionType: 0, stem: '', options: [], correctAnswer: '', score: 5, categoryId: null
})
const questionCategories = ref([])
const allQuestions = ref([])

async function loadQuestionCategories() {
  try {
    const res = await getQuestionCategories()
    questionCategories.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e) { /* */ }
}

async function loadQuestions() {
  questionLoading.value = true
  try {
    const params = { page: questionQuery.page, size: questionQuery.size }
    if (questionQuery.keyword) params.keyword = questionQuery.keyword
    if (questionQuery.type) params.type = questionQuery.type
    if (questionQuery.categoryId) params.categoryId = questionQuery.categoryId
    const res = await getQuestions(params)
    questionList.value = res.items || res.data || []
    questionTotal.value = res.total || 0
  } catch (e) { /* */ }
  finally { questionLoading.value = false }
}

async function loadAllQuestions() {
  try {
    const res = await getQuestions({ page: 1, size: 500 })
    allQuestions.value = res.items || res.data || []
  } catch (e) { /* */ }
}

function handleQuestionPageChange(p) { questionQuery.page = p; loadQuestions() }
function handleQuestionSizeChange(s) { questionQuery.size = s; questionQuery.page = 1; loadQuestions() }

function openQuestionDialog(row) {
  if (row) {
    Object.assign(questionForm, {
      id: row.id, questionType: row.questionType, stem: row.stem,
      options: Array.isArray(row.options) ? row.options.map((t, i) => ({ label: String.fromCharCode(65 + i), text: typeof t === 'string' ? t : (t.text || '') })) : [],
      correctAnswer: row.correctAnswer, score: row.score, categoryId: row.categoryId
    })
  } else {
    Object.assign(questionForm, {
      id: null, questionType: 0, stem: '', options: [{ label: 'A', text: '' }, { label: 'B', text: '' }],
      correctAnswer: '', score: 5, categoryId: null
    })
  }
  questionDialogVisible.value = true
}

async function submitQuestion() {
  if (!questionForm.stem.trim()) return ElMessage.warning('请输入题干')
  questionSubmitting.value = true
  try {
    const payload = {
      questionType: questionForm.questionType, stem: questionForm.stem,
      options: questionForm.options.map((o) => (typeof o === 'string' ? o : o.text)),
      correctAnswer: questionForm.correctAnswer,
      score: questionForm.score, categoryId: questionForm.categoryId
    }
    if (questionForm.id) {
      await updateQuestion(questionForm.id, payload)
      ElMessage.success('编辑成功')
    } else {
      await createQuestion(payload)
      ElMessage.success('新增成功')
    }
    questionDialogVisible.value = false
    loadQuestions()
  } catch (e) { /* */ }
  finally { questionSubmitting.value = false }
}

async function handleDeleteQuestion(row) {
  try {
    await ElMessageBox.confirm('确定删除该题目吗？', '提示', { type: 'warning' })
    await deleteQuestion(row.id)
    ElMessage.success('删除成功')
    loadQuestions()
  } catch (e) { /* */ }
}

async function handleImportQuestions(file) {
  try {
    await importQuestions(file)
    ElMessage.success('导入成功')
    loadQuestions()
  } catch (e) { /* */ }
  return false
}

function handleExportQuestions() {
  ElMessage.info('题库导出：请在后端实现导出接口后对接')
}

// ========== 试卷 ==========
const paperList = ref([])
const paperLoading = ref(false)
const paperTotal = ref(0)
const paperQuery = reactive({ page: 1, size: 10, keyword: '' })
const paperDialogVisible = ref(false)
const paperSubmitting = ref(false)
const paperForm = reactive({ id: null, name: '', description: '', questionIds: [], totalScore: 100 })
const previewVisible = ref(false)
const previewLoading = ref(false)
const previewPaperData = ref(null)
const previewQuestions = ref([])

async function loadPapers() {
  paperLoading.value = true
  try {
    const res = await getExamPapers()
    const list = res.items || res.data || (Array.isArray(res) ? res : [])
    paperList.value = list
    paperTotal.value = res.total || list.length || 0
  } catch (e) { /* */ }
  finally { paperLoading.value = false }
}

function handlePaperPageChange(p) { paperQuery.page = p; loadPapers() }

function openPaperDialog(row) {
  loadAllQuestions()
  if (row) {
    Object.assign(paperForm, {
      id: row.id, name: row.name, description: row.description,
      questionIds: row.questionIds ? [...row.questionIds] : [], totalScore: row.totalScore || 100
    })
  } else {
    Object.assign(paperForm, { id: null, name: '', description: '', questionIds: [], totalScore: 100 })
  }
  paperDialogVisible.value = true
}

async function submitPaper() {
  if (!paperForm.name.trim()) return ElMessage.warning('请输入试卷名称')
  paperSubmitting.value = true
  try {
    const payload = {
      name: paperForm.name, description: paperForm.description,
      questionIds: paperForm.questionIds, totalScore: paperForm.totalScore
    }
    if (paperForm.id) {
      await updateExamPaper(paperForm.id, payload)
      ElMessage.success('编辑成功')
    } else {
      await createExamPaper(payload)
      ElMessage.success('创建成功')
    }
    paperDialogVisible.value = false
    loadPapers()
  } catch (e) { /* */ }
  finally { paperSubmitting.value = false }
}

async function handleDeletePaper(row) {
  try {
    await ElMessageBox.confirm(`确定删除试卷「${row.name}」吗？`, '提示', { type: 'warning' })
    await deleteExamPaper(row.id)
    ElMessage.success('删除成功')
    loadPapers()
  } catch (e) { /* */ }
}

async function previewPaper(row) {
  previewVisible.value = true
  previewLoading.value = true
  previewPaperData.value = row
  try {
    // 根据试卷的questionIds获取题目详情
    if (row.questionIds && row.questionIds.length > 0) {
      const res = await getQuestions({ page: 1, size: 500 })
      const all = res.items || res.data || []
      previewQuestions.value = all.filter((q) => row.questionIds.includes(q.id))
    } else {
      previewQuestions.value = []
    }
  } catch (e) { /* */ }
  finally { previewLoading.value = false }
}

// ========== 测验 ==========
const testList = ref([])
const testLoading = ref(false)
const testTotal = ref(0)
const testQuery = reactive({ page: 1, size: 20, paperId: '' })
const testDialogVisible = ref(false)
const testSubmitting = ref(false)
const testForm = reactive({ paperId: null, paperName: '', targetOrgId: null, timeLimitMinutes: 60, deadline: '' })
const orgTree = ref([])
const resultsVisible = ref(false)
const resultsLoading = ref(false)
const resultsList = ref([])
const answerList = ref([])
const currentResult = ref(null)

async function loadOrgTree() {
  try {
    const res = await getOrganizationTree()
    orgTree.value = Array.isArray(res) ? res : (res.items || [])
  } catch (e) { /* */ }
}

async function loadTests() {
  testLoading.value = true
  try {
    const params = { page: testQuery.page, size: testQuery.size }
    if (testQuery.paperId) params.paperId = testQuery.paperId
    const res = await getExamTests(params)
    testList.value = res.items || res.data || (Array.isArray(res) ? res : [])
    testTotal.value = res.total || 0
  } catch (e) { /* */ }
  finally { testLoading.value = false }
}

function handleTestPageChange(p) { testQuery.page = p; loadTests() }
function handleTestSizeChange(s) { testQuery.size = s; testQuery.page = 1; loadTests() }

function openTestDialog(row) {
  testForm.paperId = row.id
  testForm.paperName = row.name
  testForm.targetOrgId = null
  testForm.timeLimitMinutes = 60
  testForm.deadline = ''
  testDialogVisible.value = true
}

async function submitTest() {
  if (!testForm.targetOrgId) return ElMessage.warning('请选择目标组织')
  if (!testForm.deadline) return ElMessage.warning('请选择截止时间')
  testSubmitting.value = true
  try {
    await createExamTest({
      paperId: testForm.paperId, targetOrgId: testForm.targetOrgId,
      timeLimitMinutes: testForm.timeLimitMinutes, deadline: testForm.deadline
    })
    ElMessage.success('发布成功')
    testDialogVisible.value = false
    loadTests()
  } catch (e) { /* */ }
  finally { testSubmitting.value = false }
}

function getScoreColor(score, total) {
  if (!total) return '#303133'
  const rate = (score / total) * 100
  if (rate >= 80) return '#52C41A'
  if (rate >= 60) return '#E6A23C'
  return '#F5222D'
}

function getResultType(score, total, passScore) {
  if (!total) return 'info'
  const pass = passScore || total * 0.6
  if (score >= pass) return 'success'
  return 'danger'
}

function getResultText(score, total, passScore) {
  if (!total) return '-'
  const pass = passScore || total * 0.6
  return score >= pass ? '及格' : '不及格'
}

async function viewResultDetail(row) {
  currentResult.value = row
  resultsVisible.value = true
  resultsLoading.value = true
  try {
    // 尝试获取测验成绩详情
    const testId = row.testId || row.id
    if (testId) {
      const res = await getExamTestResults(testId)
      const data = res.items || res.data || res
      if (Array.isArray(data)) {
        // 找到当前用户的记录
        const userRecord = data.find((r) => r.memberName === row.memberName)
        answerList.value = userRecord?.answers || userRecord?.details || []
      } else if (data.answers) {
        answerList.value = data.answers
      }
    }
  } catch (e) { /* */ }
  finally { resultsLoading.value = false }
}

function remindRetake(row) {
  ElMessage.success(`已向「${row.memberName}」发送补考通知`)
}

function handleExportResults() {
  if (testList.value.length === 0) {
    ElMessage.warning('暂无成绩数据可导出')
    return
  }
  // 生成CSV导出
  try {
    const headers = ['姓名', '支部', '测验名称', '得分', '总分', '用时(分钟)', '提交时间', '结果']
    const rows = testList.value.map((r) => [
      r.memberName || '',
      r.organizationName || '',
      r.paperName || '',
      r.score || 0,
      r.totalScore || '',
      r.timeUsedMinutes || r.duration || '',
      formatDate(r.submitTime || r.submittedAt || r.createdAt),
      getResultText(r.score, r.totalScore, r.passScore)
    ])
    const csvContent = '\uFEFF' + [headers, ...rows].map((row) => row.map((cell) => `"${cell}"`).join(',')).join('\n')
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `测验成绩_${formatDate(new Date(), 'YYYY-MM-DD')}.csv`
    a.click()
    window.URL.revokeObjectURL(url)
    ElMessage.success('导出成功')
  } catch (e) {
    ElMessage.error('导出失败')
  }
}

function handleTabChange(name) {
  if (name === 'paper' && paperList.value.length === 0) loadPapers()
  if (name === 'test' && testList.value.length === 0) {
    if (paperList.value.length === 0) loadPapers()
    loadTests()
  }
}

onMounted(() => {
  loadQuestionCategories()
  loadOrgTree()
  loadQuestions()
})
</script>

<style scoped>
.exam-management-page { padding: 0; }
.tab-toolbar { display: flex; gap: 10px; margin-bottom: 16px; flex-wrap: wrap; align-items: center; }
.pagination { margin-top: 16px; justify-content: flex-end; display: flex; }
.option-row { display: flex; align-items: center; margin-bottom: 8px; }
.form-tip { color: #909399; font-size: 12px; margin-top: 4px; }

.result-header {
  display: flex;
  gap: 20px;
  background: #fafafa;
  padding: 12px 16px;
  border-radius: 6px;
  margin-bottom: 16px;
  font-size: 13px;
  flex-wrap: wrap;
}
.result-header b { color: #909399; font-weight: 500; }

.preview-question {
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid #f0f0f0;
}
.preview-question:last-child { border-bottom: none; }
.q-title {
  font-weight: 500;
  color: #303133;
  margin-bottom: 8px;
  line-height: 1.6;
}
.q-options { padding-left: 20px; }
.q-opt {
  margin-bottom: 4px;
  color: #606266;
  line-height: 1.6;
}
</style>
