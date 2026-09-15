<template>
  <div class="battle-page">
    <div v-if="!currentGame" class="battle-lobby">
      <h2>党史 PK 对战</h2>

      <el-card v-if="pending.length > 0" style="margin-bottom: 16px">
        <template #header>待应战</template>
        <div v-for="p in pending" :key="p.gameId" class="pending-item">
          <span>{{ p.challengerName }} 向你发起挑战（{{ p.questionCount }}题）</span>
          <el-button type="primary" size="small" @click="accept(p.gameId)">应战</el-button>
        </div>
      </el-card>

      <el-card>
        <template #header>发起挑战</template>
        <el-form :model="form" label-width="80px">
          <el-form-item label="对手ID">
            <el-input-number v-model="form.opponentId" :min="1" />
          </el-form-item>
          <el-form-item label="题目数">
            <el-input-number v-model="form.questionCount" :min="3" :max="10" />
          </el-form-item>
          <el-button type="primary" @click="create" :loading="loading">发起对战</el-button>
        </el-form>
      </el-card>

      <el-card v-if="history.length > 0" style="margin-top: 16px">
        <template #header>历史对局</template>
        <el-table :data="history" size="small">
          <el-table-column prop="battleTime" label="时间" width="180" />
          <el-table-column prop="opponentName" label="对手" width="120" />
          <el-table-column prop="myScore" label="我的得分" width="100" />
          <el-table-column prop="opponentScore" label="对手得分" width="100" />
          <el-table-column label="结果" width="80">
            <template #default="{ row }">
              <el-tag :type="row.result === 'win' ? 'success' : row.result === 'lose' ? 'danger' : 'info'">
                {{ row.result === 'win' ? '胜' : row.result === 'lose' ? '负' : '平' }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </div>

    <div v-else class="battle-game">
      <div class="game-header">
        <span>第 {{ (current?.index ?? 0) + 1 }} / {{ current?.total ?? 0 }} 题</span>
        <span>我: {{ current?.myScore ?? 0 }} | {{ current?.opponentName }}: {{ current?.opponentScore ?? 0 }}</span>
        <el-button size="small" type="danger" plain @click="forfeit">弃权退出</el-button>
      </div>

      <el-card v-if="current?.question && !result" class="question-card">
        <h3>{{ current.question.stem }}</h3>
        <el-radio-group v-model="selectedAnswer" class="options">
          <el-radio v-for="(opt, i) in current.question.options" :key="i" :label="String.fromCharCode(65 + i)">
            {{ opt }}
          </el-radio>
        </el-radio-group>
        <el-button type="primary" @click="submitAnswer" :disabled="!selectedAnswer || submitting">提交答案</el-button>
      </el-card>

      <el-card v-else-if="result" class="result-card">
        <h2>对战结束</h2>
        <p>结果：{{ result.result === 'win' ? '胜利' : result.result === 'lose' ? '失败' : '平局' }}</p>
        <p>我的得分：{{ result.myScore }}</p>
        <p>对手得分：{{ result.opponentScore }}</p>
        <el-button type="primary" @click="exitGame">返回大厅</el-button>
      </el-card>

      <el-card v-else class="loading-card">
        <el-empty description="加载题目中..." />
      </el-card>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { createBattle, getPendingBattles, acceptBattle, getBattleQuestion, submitBattleAnswer, finishBattle, getBattleResult } from '@/api/feature15'

const loading = ref(false)
const submitting = ref(false)
const pending = ref([])
const history = ref([])
const currentGame = ref(null)
const current = ref(null)
const result = ref(null)
const selectedAnswer = ref('')
const form = reactive({ opponentId: 2, questionCount: 5 })

onMounted(() => {
  loadPending()
  loadHistory()
})

async function loadPending() {
  try {
    pending.value = await getPendingBattles() || []
  } catch (e) { pending.value = [] }
}

async function loadHistory() {
  // 历史对局从battle_records获取，暂时留空，后续可加接口
  history.value = []
}

async function create() {
  loading.value = true
  try {
    const res = await createBattle(form)
    currentGame.value = res.gameId
    result.value = null
    await loadQuestion()
  } catch (e) {
    ElMessage.error(e.message || '创建失败')
  } finally {
    loading.value = false
  }
}

async function accept(gameId) {
  try {
    await acceptBattle(gameId)
    currentGame.value = gameId
    result.value = null
    await loadQuestion()
  } catch (e) {
    ElMessage.error(e.message || '应战失败')
  }
}

async function loadQuestion() {
  try {
    current.value = await getBattleQuestion(currentGame.value)
    selectedAnswer.value = ''
    if (!current.value.question && current.value.index >= current.value.total) {
      await finish()
    }
  } catch (e) {
    ElMessage.error(e.message || '加载题目失败')
  }
}

async function submitAnswer() {
  if (!current.value?.question) return
  submitting.value = true
  try {
    const res = await submitBattleAnswer(currentGame.value, {
      questionId: current.value.question.questionId,
      answer: selectedAnswer.value
    })
    ElMessage.success(res.correct ? '回答正确！' : '回答错误')
    current.value.myScore = res.myScore
    current.value.opponentScore = res.opponentScore
    if (res.nextIndex >= current.value.total) {
      await finish()
    } else {
      await loadQuestion()
    }
  } catch (e) {
    ElMessage.error(e.message || '提交失败')
  } finally {
    submitting.value = false
  }
}

async function finish() {
  try {
    result.value = await finishBattle(currentGame.value)
    current.value = { ...current.value, question: null }
  } catch (e) {
    try {
      result.value = await getBattleResult(currentGame.value)
    } catch {
      ElMessage.error('获取结果失败')
    }
  }
}

async function forfeit() {
  try {
    await ElMessageBox.confirm('确定要弃权退出本场对战吗？', '提示', { type: 'warning' })
    const { forfeitBattle } = await import('@/api/feature15')
    if (forfeitBattle) await forfeitBattle(currentGame.value)
    exitGame()
  } catch (e) {
    if (e !== 'cancel') {
      // 即使弃权接口失败也允许退出
      exitGame()
    }
  }
}

function exitGame() {
  currentGame.value = null
  current.value = null
  result.value = null
  selectedAnswer.value = ''
  loadPending()
}
</script>

<style scoped>
.battle-page { padding: 16px; }
.battle-lobby h2 { margin-bottom: 16px; }
.pending-item { display: flex; justify-content: space-between; align-items: center; padding: 8px 0; }
.game-header { display: flex; justify-content: space-between; align-items: center; padding: 12px 16px; background: #f5f7fa; margin-bottom: 16px; border-radius: 4px; }
.question-card h3 { margin-bottom: 16px; }
.options { display: flex; flex-direction: column; gap: 12px; margin-bottom: 16px; }
.result-card { text-align: center; padding: 24px; }
.result-card h2 { margin-bottom: 16px; color: #C8161D; }
.loading-card { text-align: center; }
</style>
