<template>
  <div class="organization-page">
    <el-card shadow="never">
      <el-tabs v-model="activeTab" @tab-change="handleTabChange">
        <!-- Tab1 组织架构 -->
        <el-tab-pane label="组织架构" name="org">
          <div class="tab-toolbar">
            <el-button type="primary" @click="openOrgDialog(null)">
              <el-icon><Plus /></el-icon>新增根组织
            </el-button>
            <el-button @click="loadOrgTree">
              <el-icon><Refresh /></el-icon>刷新
            </el-button>
          </div>
          <el-tree
            ref="treeRef"
            :data="orgTree"
            :props="treeProps"
            node-key="id"
            default-expand-all
            v-loading="orgLoading"
          >
            <template #default="{ node, data }">
              <span class="tree-node">
                <span>{{ node.label }}</span>
                <span class="tree-actions">
                  <el-button link type="primary" size="small" @click.stop="openOrgDialog(data)">新增子级</el-button>
                  <el-button link type="primary" size="small" @click.stop="openOrgDialog(data, true)">编辑</el-button>
                  <el-button link type="danger" size="small" @click.stop="handleDeleteOrg(data)">删除</el-button>
                </span>
              </span>
            </template>
          </el-tree>
          <el-empty v-if="!orgLoading && orgTree.length === 0" description="暂无组织数据" />
        </el-tab-pane>

        <!-- Tab2 党员管理 -->
        <el-tab-pane label="党员管理" name="member">
          <div class="tab-toolbar">
            <el-input v-model="memberQuery.name" placeholder="姓名" clearable style="width:160px" @clear="loadMembers" @keyup.enter="loadMembers" />
            <el-select v-model="memberQuery.orgId" placeholder="所属组织" clearable style="width:180px" @change="loadMembers">
              <el-option v-for="o in orgFlatList" :key="o.id" :label="o.name" :value="o.id" />
            </el-select>
            <el-select v-model="memberQuery.role" placeholder="角色" clearable style="width:140px" @change="loadMembers">
              <el-option label="党员" value="member" />
              <el-option label="支部书记" value="secretary" />
              <el-option label="管理员" value="admin" />
            </el-select>
            <el-button type="primary" @click="loadMembers"><el-icon><Search /></el-icon>查询</el-button>
            <el-button type="success" @click="openMemberDialog(null)"><el-icon><Plus /></el-icon>新增</el-button>
            <el-upload :show-file-list="false" :before-upload="handleImport" accept=".xlsx,.xls">
              <el-button><el-icon><Upload /></el-icon>批量导入</el-button>
            </el-upload>
            <el-button @click="handleExport"><el-icon><Download /></el-icon>导出</el-button>
          </div>

          <el-table :data="memberList" v-loading="memberLoading" border style="width:100%">
            <el-table-column type="index" label="序号" width="60" />
            <el-table-column prop="name" label="姓名" width="120" />
            <el-table-column prop="phone" label="手机号" width="140" />
            <el-table-column label="角色" width="120">
              <template #default="{ row }">
                <el-tag size="small">{{ roleText(row.role) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="organizationName" label="所属组织" />
            <el-table-column label="状态" width="100">
              <template #default="{ row }">
                <el-tag :type="row.status === 0 || row.status === 'disabled' ? 'info' : 'success'" size="small">
                  {{ row.status === 0 || row.status === 'disabled' ? '禁用' : '启用' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="createdAt" label="加入时间" width="180">
              <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="240" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="openMemberDialog(row)">编辑</el-button>
                <el-button link type="primary" size="small" @click="openRoleDialog(row)">分配角色</el-button>
                <el-button link type="danger" size="small" @click="handleDeleteMember(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!memberLoading && memberList.length === 0" description="暂无党员数据" />
          <el-pagination
            v-if="memberTotal > 0"
            class="pagination"
            background
            layout="total, sizes, prev, pager, next, jumper"
            :total="memberTotal"
            :page-size="memberQuery.size"
            :current-page="memberQuery.page"
            :page-sizes="[10, 20, 50, 100]"
            @size-change="handleMemberSizeChange"
            @current-change="handleMemberPageChange"
          />
        </el-tab-pane>

        <!-- Tab3 挂机统计 -->
        <el-tab-pane label="挂机统计" name="anticheat">
          <div class="tab-toolbar">
            <el-select v-model="antiCheatQuery.orgId" placeholder="选择组织" clearable style="width:200px" @change="loadAntiCheat">
              <el-option v-for="o in orgFlatList" :key="o.id" :label="o.name" :value="o.id" />
            </el-select>
            <el-button type="primary" @click="loadAntiCheat"><el-icon><Search /></el-icon>查询</el-button>
            <el-button @click="loadAntiCheat"><el-icon><Refresh /></el-icon>刷新</el-button>
          </div>
          <el-table :data="antiCheatList" v-loading="antiCheatLoading" border style="width:100%">
            <el-table-column type="index" label="序号" width="60" />
            <el-table-column prop="memberName" label="姓名" width="140" />
            <el-table-column prop="organizationName" label="所属支部" />
            <el-table-column label="总学习时长" width="130">
              <template #default="{ row }">
                {{ formatMinutes(row.validLearningMinutes + row.idleMinutes) }}
              </template>
            </el-table-column>
            <el-table-column prop="validLearningMinutes" label="有效学习(分钟)" width="140" />
            <el-table-column prop="idleMinutes" label="挂机(分钟)" width="120" />
            <el-table-column label="有效率" width="120">
              <template #default="{ row }">
                <span :style="{ color: getValidRateColor(row.idleRate), fontWeight: 500 }">
                  {{ getValidRate(row.idleRate) }}%
                </span>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="120" fixed="right">
              <template #default="{ row }">
                <el-button link type="warning" size="small" @click="remindMember(row)">
                  <el-icon><Bell /></el-icon>提醒
                </el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!antiCheatLoading && antiCheatList.length === 0" description="暂无挂机记录" />
          <el-alert
            v-if="antiCheatList.length > 0"
            type="info"
            :closable="false"
            class="info-bar"
            title="统计说明：有效率 = 有效学习时长 / 总学习时长 × 100%，有效率低于60%的党员建议重点关注。"
            show-icon
          />
        </el-tab-pane>

        <!-- Tab4 发展流程 -->
        <el-tab-pane label="发展流程" name="development">
          <div class="tab-toolbar">
            <el-select v-model="devQuery.stage" placeholder="全部阶段" clearable style="width:160px" @change="loadDevelopments">
              <el-option label="入党申请人" value="applicant" />
              <el-option label="积极分子" value="activist" />
              <el-option label="发展对象" value="candidate" />
              <el-option label="预备党员" value="probationary" />
              <el-option label="正式党员" value="full_member" />
            </el-select>
            <el-input v-model="devQuery.name" placeholder="搜索姓名" clearable style="width:160px" @clear="loadDevelopments" @keyup.enter="loadDevelopments" />
            <el-button type="primary" @click="loadDevelopments"><el-icon><Search /></el-icon>查询</el-button>
            <el-button @click="loadDevelopments"><el-icon><Refresh /></el-icon>刷新</el-button>
          </div>
          <el-table :data="developmentList" v-loading="devLoading" border style="width:100%">
            <el-table-column type="index" label="序号" width="60" />
            <el-table-column prop="name" label="姓名" width="120" />
            <el-table-column prop="phone" label="手机号" width="140" />
            <el-table-column label="当前阶段" width="140">
              <template #default="{ row }">
                <el-tag :type="stageTagType(row.stage)" size="small">{{ stageText(row.stage) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="organizationName" label="所属组织" />
            <el-table-column label="提交时间" width="180">
              <template #default="{ row }">{{ formatDate(row.submitTime || row.applyDate || row.createdAt) }}</template>
            </el-table-column>
            <el-table-column label="状态" width="120">
              <template #default="{ row }">
                <el-tag :type="statusTagType(row.status)" size="small">{{ statusText(row.status) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="进度" width="200">
              <template #default="{ row }">
                <el-progress :percentage="row.progress || calcProgress(row.stage)" :stroke-width="10" :color="progressColor(row.progress || calcProgress(row.stage))" />
              </template>
            </el-table-column>
            <el-table-column label="操作" width="100" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="viewDevDetail(row)">查看</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!devLoading && developmentList.length === 0" description="暂无发展流程数据" />
          <el-pagination
            v-if="devTotal > 0"
            class="pagination"
            background
            layout="total, sizes, prev, pager, next"
            :total="devTotal"
            :page-size="devQuery.size"
            :current-page="devQuery.page"
            :page-sizes="[10, 20, 50]"
            @size-change="handleDevSizeChange"
            @current-change="handleDevPageChange"
          />
        </el-tab-pane>
      </el-tabs>
    </el-card>

    <!-- 组织新增/编辑弹窗 -->
    <el-dialog v-model="orgDialogVisible" :title="orgForm.id ? '编辑组织' : '新增组织'" width="480px">
      <el-form :model="orgForm" label-width="80px">
        <el-form-item label="组织名称">
          <el-input v-model="orgForm.name" placeholder="请输入组织名称" />
        </el-form-item>
        <el-form-item label="上级组织" v-if="!orgForm.isEdit">
          <el-input :value="orgForm.parentName || '根组织'" disabled />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="orgDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="orgSubmitting" @click="submitOrg">确定</el-button>
      </template>
    </el-dialog>

    <!-- 党员新增/编辑弹窗 -->
    <el-dialog v-model="memberDialogVisible" :title="memberForm.id ? '编辑党员' : '新增党员'" width="520px">
      <el-form :model="memberForm" label-width="90px">
        <el-form-item label="姓名">
          <el-input v-model="memberForm.name" placeholder="请输入姓名" />
        </el-form-item>
        <el-form-item label="手机号">
          <el-input v-model="memberForm.phone" placeholder="请输入手机号" />
        </el-form-item>
        <el-form-item label="密码" v-if="!memberForm.id">
          <el-input v-model="memberForm.password" type="password" placeholder="请输入初始密码" />
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="memberForm.role" placeholder="请选择角色" style="width:100%">
            <el-option label="党员" value="member" />
            <el-option label="支部书记" value="secretary" />
            <el-option label="管理员" value="admin" />
          </el-select>
        </el-form-item>
        <el-form-item label="所属组织">
          <el-tree-select
            v-model="memberForm.organizationId"
            :data="orgTree"
            :props="treeProps"
            node-key="id"
            check-strictly
            placeholder="请选择组织"
            style="width:100%"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="memberDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="memberSubmitting" @click="submitMember">确定</el-button>
      </template>
    </el-dialog>

    <!-- 分配角色弹窗 -->
    <el-dialog v-model="roleDialogVisible" title="分配角色" width="400px">
      <el-form label-width="80px">
        <el-form-item label="党员">
          <el-input :value="roleTarget?.name" disabled />
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="roleForm.role" placeholder="请选择角色" style="width:100%">
            <el-option label="党员" value="member" />
            <el-option label="支部书记" value="secretary" />
            <el-option label="管理员" value="admin" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="roleDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="roleSubmitting" @click="submitRole">确定</el-button>
      </template>
    </el-dialog>

    <!-- 发展流程详情弹窗 -->
    <el-dialog v-model="devDetailVisible" title="发展流程详情" width="640px">
      <div v-loading="devDetailLoading">
        <div v-if="devDetail" class="dev-detail-header">
          <div class="dev-info-row">
            <span><b>姓名：</b>{{ devDetail.name || devDetail.memberName }}</span>
            <span><b>所属支部：</b>{{ devDetail.organizationName }}</span>
            <span><b>当前阶段：</b>{{ stageText(devDetail.stage) }}</span>
          </div>
          <div class="dev-info-row">
            <span><b>入党申请日：</b>{{ formatDate(devDetail.applyDate) }}</span>
            <span><b>培养联系人：</b>{{ devDetail.contactName || '-' }}</span>
            <span><b>状态：</b>{{ statusText(devDetail.status) }}</span>
          </div>
        </div>

        <!-- 时间线 -->
        <el-timeline v-if="devDetailTimeline.length > 0" class="dev-timeline">
          <el-timeline-item
            v-for="(step, idx) in devDetailTimeline"
            :key="idx"
            :type="step.type"
            :icon="step.icon"
            :timestamp="step.date"
            placement="top"
          >
            <div class="timeline-title">{{ step.title }}</div>
            <div class="timeline-note" v-if="step.note">{{ step.note }}</div>
            <div class="timeline-ai" v-if="step.aiCheck" :class="{ warning: step.aiWarning }">
              {{ step.aiCheck }}
            </div>
          </el-timeline-item>
        </el-timeline>
        <el-empty v-else description="暂无时间线数据" :image-size="80" />

        <!-- 下一节点提醒 -->
        <el-alert
          v-if="devDetail && devDetail.nextStep"
          :title="devDetail.nextStep"
          type="warning"
          show-icon
          :closable="false"
          class="next-step-alert"
        />
      </div>
      <template #footer>
        <el-button @click="devDetailVisible = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getOrganizationTree, createOrganization, updateOrganization, deleteOrganization
} from '@/api/organization'
import {
  getMembers, createMember, updateMember, deleteMember, assignMemberRole, importMembers, exportMembers
} from '@/api/member'
import { getAntiCheat } from '@/api/statistics'
import { getPartyDevelopments, getPartyDevelopmentDetail } from '@/api/partyDevelopment'
import { formatDate, roleText } from '@/utils/format'

const activeTab = ref('org')
const treeRef = ref(null)
const treeProps = { label: 'name', children: 'children' }

// ========== 组织架构 ==========
const orgTree = ref([])
const orgLoading = ref(false)
const orgDialogVisible = ref(false)
const orgSubmitting = ref(false)
const orgForm = reactive({ id: null, name: '', parentId: null, parentName: '', isEdit: false })

const orgFlatList = computed(() => {
  const list = []
  const walk = (nodes) => {
    nodes.forEach((n) => {
      list.push({ id: n.id, name: n.name })
      if (n.children && n.children.length) walk(n.children)
    })
  }
  walk(orgTree.value)
  return list
})

async function loadOrgTree() {
  orgLoading.value = true
  try {
    const res = await getOrganizationTree()
    orgTree.value = Array.isArray(res) ? res : (res.items || res.data || [])
  } catch (e) {
    // 错误已提示
  } finally {
    orgLoading.value = false
  }
}

function openOrgDialog(data, isEdit = false) {
  if (isEdit) {
    orgForm.id = data.id
    orgForm.name = data.name
    orgForm.parentId = data.parentId
    orgForm.parentName = ''
    orgForm.isEdit = true
  } else {
    orgForm.id = null
    orgForm.name = ''
    orgForm.parentId = data ? data.id : null
    orgForm.parentName = data ? data.name : ''
    orgForm.isEdit = false
  }
  orgDialogVisible.value = true
}

async function submitOrg() {
  if (!orgForm.name.trim()) {
    ElMessage.warning('请输入组织名称')
    return
  }
  orgSubmitting.value = true
  try {
    if (orgForm.id) {
      await updateOrganization(orgForm.id, { name: orgForm.name })
      ElMessage.success('编辑成功')
    } else {
      await createOrganization({ name: orgForm.name, parentId: orgForm.parentId })
      ElMessage.success('新增成功')
    }
    orgDialogVisible.value = false
    loadOrgTree()
  } catch (e) {
    //
  } finally {
    orgSubmitting.value = false
  }
}

async function handleDeleteOrg(data) {
  try {
    await ElMessageBox.confirm(`确定删除组织「${data.name}」吗？该操作将同时删除其下所有子节点，不可恢复。`, '提示', { type: 'warning' })
    await deleteOrganization(data.id)
    ElMessage.success('删除成功')
    loadOrgTree()
  } catch (e) {
    // 取消或失败
  }
}

// ========== 党员管理 ==========
const memberList = ref([])
const memberLoading = ref(false)
const memberTotal = ref(0)
const memberQuery = reactive({ page: 1, size: 10, name: '', orgId: '', role: '' })
const memberDialogVisible = ref(false)
const memberSubmitting = ref(false)
const memberForm = reactive({ id: null, name: '', phone: '', password: '', role: 'member', organizationId: null })

async function loadMembers() {
  memberLoading.value = true
  try {
    const params = { page: memberQuery.page, size: memberQuery.size }
    if (memberQuery.name) params.name = memberQuery.name
    if (memberQuery.orgId !== '' && memberQuery.orgId !== null && memberQuery.orgId !== undefined) {
      params.orgId = memberQuery.orgId
    }
    if (memberQuery.role !== '' && memberQuery.role !== null && memberQuery.role !== undefined) {
      params.role = memberQuery.role
    }
    const res = await getMembers(params)
    memberList.value = res.items || res.data || []
    memberTotal.value = res.total || 0
  } catch (e) {
    //
  } finally {
    memberLoading.value = false
  }
}

function handleMemberPageChange(p) {
  memberQuery.page = p
  loadMembers()
}
function handleMemberSizeChange(s) {
  memberQuery.size = s
  memberQuery.page = 1
  loadMembers()
}

function openMemberDialog(row) {
  if (row) {
    memberForm.id = row.id
    memberForm.name = row.name
    memberForm.phone = row.phone
    memberForm.password = ''
    memberForm.role = row.role
    memberForm.organizationId = row.organizationId
  } else {
    memberForm.id = null
    memberForm.name = ''
    memberForm.phone = ''
    memberForm.password = ''
    memberForm.role = 'member'
    memberForm.organizationId = null
  }
  memberDialogVisible.value = true
}

async function submitMember() {
  if (!memberForm.name.trim()) return ElMessage.warning('请输入姓名')
  if (!memberForm.phone.trim()) return ElMessage.warning('请输入手机号')
  memberSubmitting.value = true
  try {
    const payload = {
      name: memberForm.name,
      phone: memberForm.phone,
      role: memberForm.role,
      organizationId: memberForm.organizationId
    }
    if (memberForm.id) {
      await updateMember(memberForm.id, payload)
      ElMessage.success('编辑成功')
    } else {
      if (!memberForm.password) return ElMessage.warning('请输入初始密码')
      await createMember({ ...payload, password: memberForm.password })
      ElMessage.success('新增成功')
    }
    memberDialogVisible.value = false
    loadMembers()
  } catch (e) {
    //
  } finally {
    memberSubmitting.value = false
  }
}

async function handleDeleteMember(row) {
  try {
    await ElMessageBox.confirm(`确定删除党员「${row.name}」吗？`, '提示', { type: 'warning' })
    await deleteMember(row.id)
    ElMessage.success('删除成功')
    loadMembers()
  } catch (e) {
    //
  }
}

async function handleImport(file) {
  try {
    await importMembers(file)
    ElMessage.success('导入成功')
    loadMembers()
  } catch (e) {
    //
  }
  return false
}

async function handleExport() {
  try {
    const blob = await exportMembers()
    const url = window.URL.createObjectURL(new Blob([blob]))
    const a = document.createElement('a')
    a.href = url
    a.download = `党员列表_${Date.now()}.xlsx`
    a.click()
    window.URL.revokeObjectURL(url)
  } catch (e) {
    //
  }
}

// 分配角色
const roleDialogVisible = ref(false)
const roleSubmitting = ref(false)
const roleTarget = ref(null)
const roleForm = reactive({ role: 'member' })

function openRoleDialog(row) {
  roleTarget.value = row
  roleForm.role = row.role
  roleDialogVisible.value = true
}

async function submitRole() {
  if (roleForm.role === null || roleForm.role === undefined || roleForm.role === '') {
    return ElMessage.warning('请选择角色')
  }
  roleSubmitting.value = true
  try {
    await assignMemberRole(roleTarget.value.id, roleForm.role)
    ElMessage.success('角色分配成功')
    roleDialogVisible.value = false
    loadMembers()
  } catch (e) {
    //
  } finally {
    roleSubmitting.value = false
  }
}

// ========== 挂机统计 ==========
const antiCheatList = ref([])
const antiCheatLoading = ref(false)
const antiCheatQuery = reactive({ orgId: '' })

async function loadAntiCheat() {
  antiCheatLoading.value = true
  try {
    const params = {}
    if (antiCheatQuery.orgId) params.orgId = antiCheatQuery.orgId
    const res = await getAntiCheat(params)
    antiCheatList.value = res.items || res.data || (Array.isArray(res) ? res : [])
  } catch (e) {
    //
  } finally {
    antiCheatLoading.value = false
  }
}

function formatMinutes(min) {
  if (!min && min !== 0) return '-'
  const hours = Math.floor(min / 60)
  const mins = min % 60
  return hours > 0 ? `${hours}.${Math.round(mins / 6)}小时` : `${min}分钟`
}

function getValidRate(idleRate) {
  if (idleRate === null || idleRate === undefined || idleRate === '') return '-'
  const rate = typeof idleRate === 'number' ? idleRate : parseFloat(idleRate)
  if (isNaN(rate)) return '-'
  // idleRate 可能是 0-1 或 0-100
  const normalized = rate <= 1 ? rate * 100 : rate
  return (100 - normalized).toFixed(1)
}

function getValidRateColor(idleRate) {
  const valid = parseFloat(getValidRate(idleRate))
  if (isNaN(valid)) return '#909399'
  if (valid >= 80) return '#52C41A'
  if (valid >= 60) return '#FA8C16'
  return '#F5222D'
}

function remindMember(row) {
  ElMessage.success(`已向「${row.memberName}」发送学习提醒`)
}

// ========== 发展流程 ==========
const developmentList = ref([])
const devLoading = ref(false)
const devTotal = ref(0)
const devQuery = reactive({ page: 1, size: 10, stage: '', name: '' })
const devDetailVisible = ref(false)
const devDetailLoading = ref(false)
const devDetail = ref(null)
const devDetailTimeline = ref([])

const STAGE_MAP = {
  applicant: { text: '入党申请人', progress: 10, tag: 'info' },
  activist: { text: '积极分子', progress: 30, tag: 'warning' },
  candidate: { text: '发展对象', progress: 55, tag: '' },
  probationary: { text: '预备党员', progress: 80, tag: 'danger' },
  full_member: { text: '正式党员', progress: 100, tag: 'success' }
}

function stageText(stage) {
  return STAGE_MAP[stage]?.text || stage || '-'
}

function stageTagType(stage) {
  return STAGE_MAP[stage]?.tag || 'info'
}

function calcProgress(stage) {
  return STAGE_MAP[stage]?.progress || 0
}

function statusText(status) {
  const map = {
    pending: '审核中',
    reviewing: '审核中',
    approved: '已通过',
    passed: '已通过',
    rejected: '已驳回',
    need_material: '需补充材料',
    converting: '转正中',
    completed: '已完成',
    done: '已完成'
  }
  return map[status] || status || '审核中'
}

function statusTagType(status) {
  const map = {
    pending: 'warning',
    reviewing: 'warning',
    approved: 'success',
    passed: 'success',
    rejected: 'danger',
    need_material: 'danger',
    converting: '',
    completed: 'info',
    done: 'info'
  }
  return map[status] || 'warning'
}

function progressColor(p) {
  if (p >= 80) return '#52C41A'
  if (p >= 50) return '#C8161D'
  return '#E6A23C'
}

async function loadDevelopments() {
  devLoading.value = true
  try {
    const params = { page: devQuery.page, size: devQuery.size }
    if (devQuery.stage) params.stage = devQuery.stage
    if (devQuery.name) params.name = devQuery.name
    const res = await getPartyDevelopments(params)
    developmentList.value = res.items || res.data || (Array.isArray(res) ? res : [])
    devTotal.value = res.total || 0
  } catch (e) {
    //
  } finally {
    devLoading.value = false
  }
}

function handleDevPageChange(p) {
  devQuery.page = p
  loadDevelopments()
}
function handleDevSizeChange(s) {
  devQuery.size = s
  devQuery.page = 1
  loadDevelopments()
}

async function viewDevDetail(row) {
  devDetailVisible.value = true
  devDetailLoading.value = true
  devDetail.value = row
  try {
    const res = await getPartyDevelopmentDetail(row.id)
    const detail = res.data || res
    devDetail.value = { ...row, ...detail }
    // 构建时间线
    buildTimeline(detail)
  } catch (e) {
    // 如果详情接口失败，使用列表数据构建基础时间线
    buildTimeline(row)
  } finally {
    devDetailLoading.value = false
  }
}

function buildTimeline(detail) {
  const steps = detail.timeline || detail.steps || detail.history || []
  if (Array.isArray(steps) && steps.length > 0) {
    devDetailTimeline.value = steps.map((s) => ({
      title: s.title || s.stageName || s.name,
      date: formatDate(s.date || s.time || s.createdAt),
      note: s.note || s.description || s.content,
      type: s.status === 'done' || s.status === 'completed' ? 'success' : s.status === 'current' || s.status === 'doing' ? 'primary' : 'info',
      icon: s.status === 'done' ? 'CircleCheck' : null,
      aiCheck: s.aiCheck || s.aiResult,
      aiWarning: s.aiWarning
    }))
  } else {
    // 根据当前阶段生成默认时间线
    const currentStage = detail.stage
    const allStages = [
      { key: 'applicant', title: '递交入党申请书', date: detail.applyDate },
      { key: 'activist', title: '确定为积极分子' },
      { key: 'candidate', title: '列为发展对象' },
      { key: 'probationary', title: '接收为预备党员' },
      { key: 'full_member', title: '转为正式党员' }
    ]
    const stageOrder = ['applicant', 'activist', 'candidate', 'probationary', 'full_member']
    const currentIdx = stageOrder.indexOf(currentStage)
    devDetailTimeline.value = allStages.map((s, idx) => ({
      title: s.title,
      date: s.date ? formatDate(s.date) : (idx <= currentIdx ? '已完成' : '待进行'),
      note: idx < currentIdx ? '该阶段已完成' : idx === currentIdx ? '当前进行中' : '尚未开始',
      type: idx < currentIdx ? 'success' : idx === currentIdx ? 'primary' : 'info',
      icon: idx < currentIdx ? 'CircleCheck' : null
    }))
  }
}

function handleTabChange(name) {
  if (name === 'member' && memberList.value.length === 0) loadMembers()
  if (name === 'anticheat' && antiCheatList.value.length === 0) loadAntiCheat()
  if (name === 'development' && developmentList.value.length === 0) loadDevelopments()
}

onMounted(() => {
  loadOrgTree()
})
</script>

<style scoped>
.organization-page { padding: 0; }
.tab-toolbar {
  display: flex;
  gap: 10px;
  margin-bottom: 16px;
  flex-wrap: wrap;
  align-items: center;
}
.tree-node {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  padding-right: 8px;
}
.tree-actions { opacity: 0; transition: opacity 0.2s; }
.tree-node:hover .tree-actions { opacity: 1; }
.pagination { margin-top: 16px; justify-content: flex-end; display: flex; }
.info-bar { margin-top: 16px; }

/* 发展流程详情 */
.dev-detail-header {
  background: #fafafa;
  border-radius: 6px;
  padding: 14px 16px;
  margin-bottom: 18px;
}
.dev-info-row {
  display: flex;
  gap: 20px;
  font-size: 13px;
  color: #606266;
  margin-bottom: 8px;
  flex-wrap: wrap;
}
.dev-info-row:last-child { margin-bottom: 0; }
.dev-info-row b { color: #909399; font-weight: 500; }

.dev-timeline { padding: 8px 4px; }
.timeline-title {
  font-weight: 600;
  font-size: 14px;
  color: #303133;
  margin-bottom: 4px;
}
.timeline-note {
  color: #606266;
  font-size: 13px;
  line-height: 1.6;
  margin-bottom: 6px;
}
.timeline-ai {
  background: #f6ffed;
  border: 1px solid #b7eb8f;
  border-left: 3px solid #52c41a;
  padding: 6px 10px;
  border-radius: 3px;
  font-size: 12px;
  color: #52c41a;
  line-height: 1.6;
}
.timeline-ai.warning {
  background: #fff1f0;
  border-color: #ffccc7;
  border-left-color: #f5222d;
  color: #f5222d;
}
.next-step-alert { margin-top: 16px; }
</style>
