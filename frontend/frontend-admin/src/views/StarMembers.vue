<template>
  <div class="star-members-page">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>AI 学习标兵评选</span>
          <el-button type="primary" @click="generate" :loading="loading">生成标兵榜单</el-button>
        </div>
      </template>

      <el-form :inline="true" :model="form">
        <el-form-item label="组织范围">
          <el-select v-model="form.organizationId" placeholder="全部组织" clearable>
            <el-option v-for="org in orgs" :key="org.id" :label="org.name" :value="org.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="榜单人数">
          <el-input-number v-model="form.topN" :min="3" :max="50" />
        </el-form-item>
      </el-form>

      <el-table v-if="result" :data="result.members" stripe>
        <el-table-column label="排名" width="80">
          <template #default="{ row }">
            <el-tag :type="row.rank <= 3 ? 'warning' : 'info'">{{ row.rank }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="memberName" label="姓名" width="120" />
        <el-table-column prop="organizationName" label="所属支部" width="180" />
        <el-table-column prop="totalScore" label="综合得分" width="100">
          <template #default="{ row }">
            <span style="font-weight: bold; color: #409eff">{{ row.totalScore }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="level" label="等级" width="80">
          <template #default="{ row }">
            <el-tag :type="row.level === '优秀' ? 'success' : row.level === '良好' ? 'primary' : 'info'">{{ row.level }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="维度得分">
          <template #default="{ row }">
            <div class="dimensions">
              <span v-for="d in row.dimensions" :key="d.name" class="dim-tag">
                {{ d.name }}: {{ d.score }}
              </span>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="aiReason" label="AI 评语" min-width="200" show-overflow-tooltip />
      </el-table>

      <el-empty v-else description="点击生成按钮，AI 将基于学习数据评选学习标兵" />
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { generateStarMembers } from '@/api/feature15'
import { getOrganizationTree } from '@/api/organization'

const loading = ref(false)
const result = ref(null)
const orgs = ref([])
const form = reactive({
  organizationId: null,
  topN: 10
})

onMounted(async () => {
  try {
    const tree = await getOrganizationTree()
    orgs.value = flattenOrgTree(tree || [])
  } catch (e) {}
})

function flattenOrgTree(tree) {
  const result = []
  function walk(nodes) {
    for (const n of nodes) {
      result.push({ id: n.id, name: n.name })
      if (n.children) walk(n.children)
    }
  }
  walk(tree)
  return result
}

async function generate() {
  loading.value = true
  try {
    result.value = await generateStarMembers(form)
    ElMessage.success('标兵榜单生成成功')
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
.dimensions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.dim-tag {
  font-size: 12px;
  color: #666;
  background: #f5f7fa;
  padding: 2px 8px;
  border-radius: 4px;
}
</style>
