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
    </div>

    <div v-else class="battle-game">
      <div class="game-header">
        <span>第 {{ current.index + 1 }} / {{ current.total }} 题</span>
        <span>我: {{ current.myScore }} | {{ current.opponentName }}: {{ current.opponentScore }}</span>
      </div>

      <el-card v-if="current.question" class="question-card">
        <h3>{{ current.question.stem }}</h3>
        <el-radio-group v-model="selectedAnswer" class="options">
          <el-radio v-for="(opt, i) in current.question.options" :key="i" :label="String.fromCharCode(65 + i)">
            {{ opt }}
          </el-radio>
        </el-radio-group>
        <el-button type="primary" @click="submitAnswer" :disabled="!selectedAnswer">提交答案</el-button>
      </el-card>

      <el-card v-else-if="result" class="result-card">
        <h2>对战结束</h2>
        <p>结果：{{ result.result === 'win' ? '胜利' : result.result === 'lose' ? '失败' : '平局' }}</p>
        <p>我的得分：{{ result.myScore }}</p>
        <p>对手得分：{{ result.opponentScore }}</p>
        <el-button @click="currentGame = null">返回大厅</el-button>
      </el-card>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { createBattle, getPendingBattles, acceptBattle, getBattleQuestion, submitBattleAnswer, finishBattle, getBattleResult } from '@/api/feature15'

const loading = ref(false)
const pending = ref([])
const currentGame = ref(null)
const current = ref(null)
const result = ref(null)
const selectedAnswer = ref('')
const form = reactive({ opponentId: 2, questionCount: 5 })

onMounted(loadPending)

async function loadPending() {
  pending.value = await getPendingBattles() || []
}

async function create() {
  loading.value = true
  try {
    const res = await createBattle(form)
    currentGame.value = res.gameId
    await loadQuestion()
  } catch (e) {
    ElMessage.error('创建失败')
  } finally {
    loading.value = false
  }
}

async function accept(gameId) {
  await acceptBattle(gameId)
  currentGame.value = gameId
  await loadQuestion()
}

async function loadQuestion() {
  current.value = await getBattleQuestion(currentGame.value)
  selectedAnswer.value = ''
  if (!current.value.question && current.value.index >= current.value.total) {
    await finish()
  }
}

async function submitAnswer() {
  const res = await submitBattleAnswer(currentGame.value, {
    questionId: current.value.question.questionId,
    answer: selectedAnswer.value
  })
  ElMessage.success(res.correct ? '回答正确！' : '回答错误')
  if (res.nextIndex >= current.value.total) {
    await finish()
  } else {
    await loadQuestion()
  }
}

async function finish() {
  result.value = await finishBattle(currentGame.value)
  current.value = { ...current.value, question: null }
}
</script>

<style scoped>
.battle-page { padding: 16px; }
.battle-lobby h2 { margin-bottom: 16px; }
.pending-item { display: flex; justify-content: space-between; align-items: center; padding: 8px 0; }
.game-header { display: flex; justify-content: space-between; padding: 16px; background: #f5f7fa; margin-bottom: 16px; }
.question-card h3 { margin-bottom: 16px; }
.options { display: flex; flex-direction: column; gap: 12px; margin-bottom: 16px; }
.result-card { text-align: center; }
</style>
