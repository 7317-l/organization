<template>
  <div class="ai-chat-overlay">

    <!-- ==========================================
         AI 主面板
         ========================================== -->

    <div class="ai-chat">

      <!-- ========================================
           顶部
           ======================================== -->

      <header class="ai-header">

        <div class="ai-header-left">

          <div class="ai-logo">
            <span class="ai-logo-core">AI</span>
          </div>

          <div class="ai-header-title">

            <div class="ai-title">
              AI 学习助手
            </div>

            <div class="ai-subtitle">
              智能分析 · 错题辅导 · 学习诊断
            </div>

          </div>

        </div>


        <div class="ai-header-right">

          <div class="ai-status">
            <span class="ai-status-dot"></span>
            AI 在线
          </div>

          <button
            class="close-button"
            @click="closeAI"
          >
            ×
          </button>

        </div>

      </header>


      <!-- ========================================
           主体
           ======================================== -->

      <div class="ai-body">

        <!-- ======================================
             左侧导航
             ====================================== -->

        <aside class="ai-sidebar">

          <button
            class="sidebar-item"
            :class="{
              active:
                activePanel === 'chat'
            }"
            @click="
              activePanel = 'chat'
            "
          >

            <span class="sidebar-icon">
              💬
            </span>

            <span>
              普通AI对话
            </span>

          </button>


          <button
            class="sidebar-item"
            :class="{
              active:
                activePanel === 'analysis'
            }"
            @click="
              activePanel = 'analysis'
            "
          >

            <span class="sidebar-icon">
              ✦
            </span>

            <span>
              AI分析
            </span>

          </button>


          <button
            class="sidebar-item"
            :class="{
              active:
                activePanel === 'summary'
            }"
            @click="
              activePanel = 'summary'
            "
          >

            <span class="sidebar-icon">
              📈
            </span>

            <span>
              AI学习总结
            </span>

          </button>


          <button
            class="sidebar-item"
            :class="{
              active:
                activePanel === 'wrong'
            }"
            @click="
              activePanel = 'wrong'
            "
          >

            <span class="sidebar-icon">
              ❌
            </span>

            <span>
              错题本
            </span>

            <span
              v-if="wrongAnswers.length > 0"
              class="sidebar-count"
            >
              {{ wrongAnswers.length }}
            </span>

          </button>


          <button
            class="sidebar-item"
            :class="{
              active:
                activePanel === 'study'
            }"
            @click="
              activePanel = 'study'
            "
          >

            <span class="sidebar-icon">
              📊
            </span>

            <span>
              学习情况
            </span>

          </button>


          <div class="sidebar-divider"></div>


          <button
            class="sidebar-item"
            :class="{
              active:
                activePanel === 'history'
            }"
            @click="
              activePanel = 'history'
            "
          >

            <span class="sidebar-icon">
              📝
            </span>

            <span>
              答题记录
            </span>

          </button>


          <!-- ====================================
               当前识别题目
               ==================================== -->

          <div
            v-if="hasQuestion"
            class="sidebar-question-card"
          >

            <div class="sidebar-question-label">
              当前题目
            </div>

            <div class="sidebar-question-text">
              {{ questionData.question }}
            </div>

          </div>

        </aside>


        <!-- ======================================
             右侧内容
             ====================================== -->

        <main class="ai-content">

          <!-- ====================================
               AI分析
               ==================================== -->

          <section
            v-if="activePanel === 'chat'"
            class="panel chat-panel"
          >

            <div class="panel-header">

              <div>

                <h2>
                  普通AI对话
                </h2>

                <p>
                  可以像使用普通智能助手一样自由提问
                </p>

              </div>

            </div>

            <div class="chat-messages normal-chat-messages">

              <div
                v-for="(message, index) in chatMessages"
                :key="index"
                class="chat-message"
                :class="message.role"
              >

                <div
                  v-if="message.role === 'assistant'"
                  class="chat-avatar"
                >
                  AI
                </div>

                <div class="chat-bubble">
                  {{ message.content }}
                </div>

              </div>

            </div>

            <div class="chat-input-area">

              <input
                v-model="chatInput"
                type="text"
                placeholder="请输入你想咨询的问题"
                @keyup.enter="sendMessage"
              />

              <button
                class="send-button"
                :disabled="!chatInput.trim()"
                @click="sendMessage"
              >
                发送
              </button>

            </div>

          </section>


          <section
            v-if="activePanel === 'analysis'"
            class="panel analysis-panel"
          >

            <div class="panel-header">

              <div>

                <h2>
                  AI智能分析
                </h2>

                <p>
                  AI仅识别当前正在作答的题目，并判断该题是否需要分析
                </p>

              </div>


              <button
                class="analyze-button"
                :disabled="
                  analysisLoading ||
                  !hasQuestion
                "
                @click="startAnalysis"
              >

                <span
                  v-if="analysisLoading"
                  class="loading-spinner"
                ></span>

                <span v-if="analysisLoading">
                  正在分析...
                </span>

                <span v-else>
                  ✦ 分析当前题目
                </span>

              </button>

            </div>


            <!-- ==================================
                 自动错题提示
                 ================================== -->

            <div
              v-if="
                autoHelp &&
                autoHelp.question
              "
              class="auto-help-banner"
            >

              <div class="auto-help-banner-icon">
                💡
              </div>

              <div class="auto-help-banner-content">

                <div class="auto-help-banner-title">
                  AI检测到这是一道错题
                </div>

                <div class="auto-help-banner-text">
                  已自动为你准备本题解析，帮助你理解错误原因。
                </div>

              </div>

            </div>


            <!-- ==================================
                 没有题目
                 ================================== -->

            <div
              v-if="!hasQuestion"
              class="empty-state"
            >

              <div class="empty-icon">
                ✦
              </div>

              <div class="empty-title">
                暂无可分析题目
              </div>

              <div class="empty-text">
                请先完成一道题目，AI将根据你的答题情况进行分析。
              </div>

            </div>


            <!-- ==================================
                 题目卡片
                 ================================== -->

            <div
              v-if="hasQuestion"
              class="question-analysis-card"
            >

              <div class="question-card-top">

                <div class="question-number">
                  第 {{ questionNumber }} 题
                </div>

                <div
                  v-if="questionData.submitted"
                  class="answer-status"
                  :class="{
                    correct:
                      questionData.correct,
                    wrong:
                      !questionData.correct
                  }"
                >

                  <span
                    v-if="questionData.correct"
                  >
                    ✓ 回答正确
                  </span>

                  <span
                    v-else
                  >
                    ✕ 回答错误
                  </span>

                </div>

              </div>


              <div class="question-title">
                {{ questionData.question }}
              </div>


              <!-- =================================
                   选项
                   ================================= -->

              <div class="question-options">

                <div
                  v-for="(option, index) in normalizedOptions"
                  :key="option.key || index"
                  class="question-option"
                  :class="{
                    selected:
                      isSelectedOption(option),
                    correct:
                      questionData.submitted &&
                      isCorrectOption(option),
                    wrong:
                      questionData.submitted &&
                      isSelectedOption(option) &&
                      !isCorrectOption(option)
                  }"
                >

                  <span class="option-letter">
                    {{ option.key }}
                  </span>

                  <span class="option-text">
                    {{ option.text }}
                  </span>

                  <span
                    v-if="
                      questionData.submitted &&
                      isCorrectOption(option)
                    "
                    class="option-result"
                  >
                    ✓
                  </span>

                  <span
                    v-else-if="
                      questionData.submitted &&
                      isSelectedOption(option) &&
                      !isCorrectOption(option)
                    "
                    class="option-result wrong-icon"
                  >
                    ×
                  </span>

                </div>

              </div>

            </div>


            <!-- ==================================
                 AI分析内容
                 ================================== -->

            <div
              v-if="
                hasQuestion &&
                analysisResult
              "
              class="analysis-result"
            >

              <div class="analysis-result-header">

                <div class="analysis-ai-avatar">
                  AI
                </div>

                <div>

                  <div class="analysis-ai-name">
                    AI 学习助手
                  </div>

                  <div class="analysis-ai-tip">
                    针对本题生成的学习分析
                  </div>

                </div>

              </div>


              <!-- 正误判断 -->

              <div class="analysis-section">

                <div class="analysis-section-title">
                  <span class="section-icon">
                    🎯
                  </span>

                  答题判断
                </div>

                <div class="analysis-section-content">
                  {{ analysisResult.judgement }}
                </div>

              </div>


              <!-- 正确答案 -->

              <div class="analysis-section">

                <div class="analysis-section-title">
                  <span class="section-icon">
                    ✓
                  </span>

                  正确答案
                </div>

                <div class="answer-box">

                  <span class="answer-label">
                    {{ questionData.correctAnswer }}
                  </span>

                  <span>
                    {{
                      correctOptionText
                    }}
                  </span>

                </div>

              </div>


              <!-- 知识点 -->

              <div class="analysis-section">

                <div class="analysis-section-title">
                  <span class="section-icon">
                    📚
                  </span>

                  核心知识点
                </div>

                <div class="knowledge-box">

                  <div class="knowledge-name">
                    {{
                      questionData.knowledgePoint ||
                      "本题核心知识点"
                    }}
                  </div>

                  <div class="knowledge-description">
                    {{
                      analysisResult.knowledge
                    }}
                  </div>

                </div>

              </div>


              <!-- 错误原因 -->

              <div
                v-if="
                  !questionData.correct
                "
                class="analysis-section"
              >

                <div class="analysis-section-title">
                  <span class="section-icon">
                    ⚠
                  </span>

                  错误原因
                </div>

                <div class="mistake-box">

                  <div class="mistake-title">
                    为什么容易做错？
                  </div>

                  <div class="mistake-content">
                    {{
                      analysisResult.mistake
                    }}
                  </div>

                </div>

              </div>


              <!-- 解题思路 -->

              <div class="analysis-section">

                <div class="analysis-section-title">
                  <span class="section-icon">
                    💡
                  </span>

                  解题思路
                </div>

                <div class="steps">

                  <div
                    v-for="
                      (step, index)
                      in analysisResult.steps
                    "
                    :key="index"
                    class="step-item"
                  >

                    <div class="step-number">
                      {{ index + 1 }}
                    </div>

                    <div class="step-content">
                      {{ step }}
                    </div>

                  </div>

                </div>

              </div>


              <!-- 学习建议 -->

              <div class="analysis-section">

                <div class="analysis-section-title">
                  <span class="section-icon">
                    🚀
                  </span>

                  学习建议
                </div>

                <div class="suggestion-box">
                  {{
                    analysisResult.suggestion
                  }}
                </div>

              </div>

            </div>


            <!-- ==================================
                 AI聊天区域
                 ================================== -->

            <div
              v-if="hasQuestion"
              class="chat-section"
            >

              <div class="chat-title">
                还有疑问？继续问 AI
              </div>


              <div class="chat-messages">

                <div
                  v-for="
                    (message, index)
                    in chatMessages
                  "
                  :key="index"
                  class="chat-message"
                  :class="message.role"
                >

                  <div
                    v-if="
                      message.role === 'assistant'
                    "
                    class="chat-avatar"
                  >
                    AI
                  </div>

                  <div class="chat-bubble">
                    {{ message.content }}
                  </div>

                </div>

              </div>


              <div class="chat-input-area">

                <input
                  v-model="chatInput"
                  type="text"
                  placeholder="例如：为什么这个选项是错的？"
                  @keyup.enter="sendMessage"
                />

                <button
                  class="send-button"
                  :disabled="
                    !chatInput.trim()
                  "
                  @click="sendMessage"
                >
                  发送
                </button>

              </div>

            </div>

          </section>


          <!-- ====================================
               错题本
               ==================================== -->

          <section
            v-if="activePanel === 'summary'"
            class="panel analysis-panel"
          >

            <div class="panel-header">

              <div>
                <h2>
                  AI学习总结
                </h2>

                <p>
                  根据你的错题数据分析薄弱知识点，并制定强化方案
                </p>
              </div>


              <button
                class="analyze-button"
                :disabled="summaryLoading"
                @click="startSummary"
              >
                <span v-if="summaryLoading">
                  正在生成总结...
                </span>

                <span v-else>
                  📊 生成学习总结
                </span>

              </button>

            </div>


            <div
              v-if="summaryResult"
              class="analysis-result"
            >
              {{ summaryResult }}
            </div>


            <div
              v-else
              class="empty-state"
            >
              点击按钮，让AI根据你的错题记录生成学习建议。
            </div>

          </section>


          <section
            v-if="activePanel === 'wrong'"
            class="panel"
          >

            <div class="panel-header">

              <div>

                <h2>
                  我的错题本
                </h2>

                <p>
                  系统自动收集你答错过的题目
                </p>

              </div>

              <div class="wrong-total">
                共 {{ wrongAnswers.length }} 道错题
              </div>

            </div>


            <!-- 没有错题 -->

            <div
              v-if="
                wrongAnswers.length === 0
              "
              class="empty-state"
            >

              <div class="empty-icon success">
                ✓
              </div>

              <div class="empty-title">
                暂时没有错题
              </div>

              <div class="empty-text">
                继续保持，你的答题表现很好！
              </div>

            </div>


            <!-- 错题列表 -->

            <div
              v-else
              class="wrong-list"
            >

              <div
                v-for="
                  (record, index)
                  in wrongAnswers
                "
                :key="
                  record.id ||
                  index
                "
                class="wrong-item"
                :class="{
                  active:
                    selectedWrongId ===
                    (
                      record.id ||
                      index
                    )
                }"
                @click="
                  selectWrongAnswer(
                    record
                  )
                "
              >

                <div class="wrong-item-number">
                  {{ index + 1 }}
                </div>

                <div class="wrong-item-content">

                  <div class="wrong-item-question">
                    {{ record.question }}
                  </div>

                  <div class="wrong-item-meta">

                    <span>
                      你的答案：
                      {{
                        record.userAnswer ||
                        "未作答"
                      }}
                    </span>

                    <span>
                      正确答案：
                      {{
                        record.correctAnswer
                      }}
                    </span>

                  </div>

                  <div
                    v-if="
                      record.knowledgePoint
                    "
                    class="wrong-item-knowledge"
                  >
                    {{ record.knowledgePoint }}
                  </div>

                </div>

                <div class="wrong-arrow">
                  →
                </div>

              </div>

            </div>


            <!-- 错题详情 -->

            <div
              v-if="selectedWrong"
              class="wrong-detail"
            >

              <div class="wrong-detail-header">

                <div>

                  <div class="detail-label">
                    错题解析
                  </div>

                  <div class="detail-title">
                    {{ selectedWrong.question }}
                  </div>

                </div>

                <button
                  class="detail-close"
                  @click="
                    selectedWrongId = null
                  "
                >
                  ×
                </button>

              </div>


              <div class="detail-answer-row">

                <div class="detail-answer wrong-answer">
                  <span>
                    你的答案
                  </span>

                  <strong>
                    {{
                      selectedWrong.userAnswer ||
                      "未作答"
                    }}
                  </strong>
                </div>


                <div class="detail-answer correct-answer">
                  <span>
                    正确答案
                  </span>

                  <strong>
                    {{
                      selectedWrong.correctAnswer
                    }}
                  </strong>
                </div>

              </div>


              <div
                v-if="
                  selectedWrong.knowledgePoint
                "
                class="detail-knowledge"
              >

                <div class="detail-knowledge-title">
                  本题知识点
                </div>

                <div>
                  {{
                    selectedWrong.knowledgePoint
                  }}
                </div>

              </div>


              <button
                class="detail-analyze-button"
                @click="
                  analyzeWrongQuestion(
                    selectedWrong
                  )
                "
              >
                ✦ AI分析这道错题
              </button>

            </div>

          </section>


          <!-- ====================================
               学习情况
               ==================================== -->

          <section
            v-if="activePanel === 'study'"
            class="panel"
          >

            <div class="panel-header">

              <div>

                <h2>
                  学习情况
                </h2>

                <p>
                  根据你的历史答题记录生成学习数据
                </p>

              </div>

            </div>


            <div class="statistics-grid">

              <div class="stat-card">

                <div class="stat-icon">
                  📝
                </div>

                <div class="stat-value">
                  {{ totalAnswers }}
                </div>

                <div class="stat-label">
                  总答题数
                </div>

              </div>


              <div class="stat-card">

                <div class="stat-icon">
                  ✓
                </div>

                <div class="stat-value success-text">
                  {{ correctAnswers }}
                </div>

                <div class="stat-label">
                  正确题数
                </div>

              </div>


              <div class="stat-card">

                <div class="stat-icon">
                  ✕
                </div>

                <div class="stat-value danger-text">
                  {{ wrongAnswers.length }}
                </div>

                <div class="stat-label">
                  错题数
                </div>

              </div>


              <div class="stat-card">

                <div class="stat-icon">
                  🎯
                </div>

                <div class="stat-value">
                  {{ accuracy }}%
                </div>

                <div class="stat-label">
                  正确率
                </div>

              </div>

            </div>


            <!-- 学习评价 -->

            <div class="study-evaluation">

              <div class="evaluation-title">
                AI学习评价
              </div>

              <div class="evaluation-content">
                {{ studyEvaluation }}
              </div>

            </div>


            <!-- 知识点统计 -->

            <div class="knowledge-stat-card">

              <div class="knowledge-stat-title">
                错题知识点分布
              </div>

              <div
                v-if="
                  knowledgeStatistics.length === 0
                "
                class="knowledge-empty"
              >
                暂无足够数据
              </div>

              <div
                v-else
                class="knowledge-list"
              >

                <div
                  v-for="
                    item in knowledgeStatistics
                  "
                  :key="
                    item.name
                  "
                  class="knowledge-stat-item"
                >

                  <div class="knowledge-stat-info">

                    <span>
                      {{ item.name }}
                    </span>

                    <span>
                      {{ item.count }} 题
                    </span>

                  </div>

                  <div class="progress-bar">

                    <div
                      class="progress-bar-inner"
                      :style="{
                        width:
                          item.percent +
                          '%'
                      }"
                    ></div>

                  </div>

                </div>

              </div>

            </div>

          </section>


          <!-- ====================================
               答题记录
               ==================================== -->

          <section
            v-if="activePanel === 'history'"
            class="panel"
          >

            <div class="panel-header">

              <div>

                <h2>
                  答题记录
                </h2>

                <p>
                  查看你的历史答题情况
                </p>

              </div>

              <div class="history-count">
                {{ answerHistory.length }} 条记录
              </div>

            </div>


            <div
              v-if="
                answerHistory.length === 0
              "
              class="empty-state"
            >

              <div class="empty-icon">
                📝
              </div>

              <div class="empty-title">
                暂无答题记录
              </div>

              <div class="empty-text">
                完成题目后，这里会自动记录你的答题情况。
              </div>

            </div>


            <div
              v-else
              class="history-list"
            >

              <div
                v-for="
                  (record, index)
                  in reversedHistory
                "
                :key="
                  record.id ||
                  index
                "
                class="history-item"
              >

                <div
                  class="history-status"
                  :class="{
                    correct:
                      record.correct,
                    wrong:
                      !record.correct
                  }"
                >

                  <span
                    v-if="record.correct"
                  >
                    ✓
                  </span>

                  <span v-else>
                    ×
                  </span>

                </div>


                <div class="history-content">

                  <div class="history-question">
                    {{ record.question }}
                  </div>

                  <div class="history-meta">

                    <span>
                      你的答案：
                      {{
                        record.userAnswer ||
                        "未作答"
                      }}
                    </span>

                    <span>
                      正确答案：
                      {{
                        record.correctAnswer
                      }}
                    </span>

                    <span
                      v-if="
                        record.knowledgePoint
                      "
                    >
                      {{ record.knowledgePoint }}
                    </span>

                  </div>

                </div>

              </div>

            </div>

          </section>

        </main>

      </div>

    </div>

  </div>
</template>


<script setup>

import {
  ref,
  computed,
  watch,
  onMounted
} from "vue"


/* =====================================================
   Props
   ===================================================== */

const props = defineProps({

  questionData: {

    type: Object,

    default: () => null

  },


  answerHistory: {

    type: Array,

    default: () => []

  },


  autoHelp: {

    type: Object,

    default: () => ({

      visible:
        false,

      question:
        null

    })

  }

})


/* =====================================================
   Emits
   ===================================================== */

const emit = defineEmits([

  "close",

  "clear-auto-help"

])


/* =====================================================
   页面状态
   ===================================================== */

const activePanel =
  ref("analysis")


const analysisLoading =
  ref(false)


const analysisResult =
  ref(null)


/* =====================================================
   AI学习总结
   ===================================================== */

const summaryLoading =
  ref(false)


const summaryResult =
  ref("")


const startSummary =
  async () => {

    if (
      !props.answerHistory ||
      props.answerHistory.length === 0
    ) {

      summaryResult.value =
        "目前还没有足够的答题数据，请先完成一些题目。"

      return

    }


    summaryLoading.value = true


    try {

      const wrongData =
        props.answerHistory.filter(
          item =>
            item.correct === false ||
            item.isCorrect === false ||
            item.correctAnswer !== item.userAnswer
        )

      // 统计各知识点的答题情况
      const knowledgeMap = {}
      props.answerHistory.forEach(item => {
        const kp = item.knowledgePoint || item.category || '未分类'
        if (!knowledgeMap[kp]) {
          knowledgeMap[kp] = { name: kp, total: 0, correct: 0, wrong: 0 }
        }
        knowledgeMap[kp].total++
        const isCorrect = item.correct === true || item.isCorrect === true
        if (isCorrect) {
          knowledgeMap[kp].correct++
        } else {
          knowledgeMap[kp].wrong++
        }
      })
      const knowledgePoints = Object.values(knowledgeMap)

      // 给错题加上 wrongCount 字段（按知识点统计错误次数）
      const wrongCountMap = {}
      wrongData.forEach(item => {
        const kp = item.knowledgePoint || item.category || '未分类'
        wrongCountMap[kp] = (wrongCountMap[kp] || 0) + 1
      })
      const wrongQuestionsWithCount = wrongData.map(item => ({
        ...item,
        wrongCount: wrongCountMap[item.knowledgePoint || item.category || '未分类'] || 1
      }))

      const prompt = `
你是一名专业的党员学习分析助手。

请根据用户的答题数据，生成一份条理清晰的学习分析报告。

【输出结构要求】严格按照以下两大部分输出，使用清晰的小标题和分点：

━━━━━━━━━━━━━━━━━━━━
第一部分：近期学习成绩与进度总结
━━━━━━━━━━━━━━━━━━━━
1. 整体成绩概览
   - 总答题数、正确数、正确率
   - 与之前相比的进步或退步情况

2. 各知识点掌握情况
   - 按正确率从高到低列出各知识点
   - 标注掌握较好的知识点和薄弱知识点

3. 错题分布分析
   - 错误最多的知识点排名
   - 高频错误原因分析

━━━━━━━━━━━━━━━━━━━━
第二部分：学习建议与提升计划
━━━━━━━━━━━━━━━━━━━━
1. 近期重点学习建议
   - 针对最薄弱的2-3个知识点，给出具体学习方向
   - 建议优先学习的内容顺序

2. 针对性提升方法
   - 针对错题类型，给出具体的学习方法和记忆技巧
   - 容易混淆的知识点对比提醒

3. 下一阶段学习计划
   - 短期（1周内）学习目标
   - 中期（1个月）提升计划
   - 每日建议学习时长和内容分配

【数据说明】
- 总答题数：${props.answerHistory.length}
- 正确题数：${props.answerHistory.filter(item => item.correct === true || item.isCorrect === true).length}
- 知识点统计：${JSON.stringify(knowledgePoints)}
- 错题详情：${JSON.stringify(wrongData)}

请用简洁明了的语言，避免空话套话，建议要具体可执行。
`


      const response =
        await fetch(
          "http://localhost:3000/api/ai/analyze-learning",
          {
            method:"POST",
            headers:{
              "Content-Type":"application/json"
            },
            body:JSON.stringify({
              totalQuestions: props.answerHistory.length,
              correctQuestions: props.answerHistory.filter(item => item.correct === true || item.isCorrect === true).length,
              knowledgePoints,
              wrongQuestions: wrongQuestionsWithCount
            })
          }
        )


      const data =
        await response.json()


      console.log("AI学习总结返回数据：", data)

      summaryResult.value =
        data?.data?.content ||
        data?.reply ||
        data?.message ||
        "AI暂时无法生成总结。"


    } catch(error) {

      summaryResult.value =
        "生成学习总结失败，请稍后重试。"

    } finally {

      summaryLoading.value = false

    }

  }


const chatInput =
  ref("")


const chatMessages =
  ref(JSON.parse(localStorage.getItem("ai_chat_messages") || "[]"))


if (chatMessages.value.length === 0) {
  chatMessages.value.push({
    role: "assistant",
    content: "你好，我是AI智能助手，有什么问题都可以直接问我。"
  })
}


const selectedWrongId =
  ref(null)

watch(
  chatMessages,
  (value) => {
    localStorage.setItem("ai_chat_messages", JSON.stringify(value))
  },
  { deep: true }
)



/* =====================================================
   当前题目
   ===================================================== */

const questionData =
  computed(() => {
    return props.questionData
  })


/* =====================================================
   是否存在当前题目
   ===================================================== */

const hasQuestion =
  computed(() => {
    return !!(
      questionData.value &&
      questionData.value.question
    )
  })


/* =====================================================
   题号
   ===================================================== */

const questionNumber =
  computed(() => {
    if (!questionData.value) return 0
    if (questionData.value.questionNumber) return questionData.value.questionNumber
    if (typeof questionData.value.questionIndex === "number") {
      return questionData.value.questionIndex + 1
    }
    return 1
  })


/* =====================================================
   选项标准化
   ===================================================== */

const normalizedOptions =
  computed(() => {
    if (!questionData.value || !Array.isArray(questionData.value.options)) {
      return []
    }
    return questionData.value.options.map((option, index) => {
      if (typeof option === "object" && option !== null) {
        return {
          key: option.key || String.fromCharCode(65 + index),
          text: option.text || ""
        }
      }
      const text = String(option)
      const match = text.match(/^([A-Z])[\.\、\s:：]+(.+)$/)
      if (match) {
        return { key: match[1], text: match[2] }
      }
      return {
        key: String.fromCharCode(65 + index),
        text
      }
    })
  })


/* =====================================================
   提取答案字母
   ===================================================== */

const normalizeAnswer =
  (answer) => {
    if (answer === null || answer === undefined) return ""
    const value = String(answer).trim().toUpperCase()
    if (/^[A-Z]$/.test(value)) return value
    const match = value.match(/^([A-Z])[\.\、\s:：]/)
    if (match) return match[1]
    return value
  }


/* =====================================================
   判断选项是否为用户选择
   ===================================================== */

const isSelectedOption =
  (option) => {
    if (!questionData.value) return false
    const userAnswer = normalizeAnswer(questionData.value.userAnswer)
    return userAnswer !== "" && userAnswer === normalizeAnswer(option.key)
  }


/* =====================================================
   判断是否正确答案
   ===================================================== */

const isCorrectOption =
  (option) => {
    if (!questionData.value) return false
    const correctAnswer = normalizeAnswer(questionData.value.correctAnswer)
    return correctAnswer !== "" && correctAnswer === normalizeAnswer(option.key)
  }


/* =====================================================
   正确答案文字
   ===================================================== */

const correctOptionText =
  computed(() => {
    const option = normalizedOptions.value.find(item => isCorrectOption(item))
    return option ? option.text : "请参考题目正确选项"
  })


/* =====================================================
   答题历史
   ===================================================== */

const answerHistory =
  computed(() => {
    if (!Array.isArray(props.answerHistory)) return []
    return props.answerHistory
  })


/* =====================================================
   错题本
   ===================================================== */

const wrongAnswers =
  computed(() => {
    return answerHistory.value.filter(
      record => record && record.correct === false
    )
  })


/* =====================================================
   总答题数
   ===================================================== */

const totalAnswers =
  computed(() => {
    return answerHistory.value.length
  })


/* =====================================================
   正确题数
   ===================================================== */

const correctAnswers =
  computed(() => {
    return answerHistory.value.filter(
      record =>
        record &&
        (record.correct === true || record.isCorrect === true)
    ).length
  })


/* =====================================================
   正确率
   ===================================================== */

const accuracy =
  computed(() => {
    if (totalAnswers.value === 0) return 0
    return Math.round((correctAnswers.value / totalAnswers.value) * 100)
  })


/* =====================================================
   学习评价
   ===================================================== */

const studyEvaluation =
  ref("完成答题后，AI将根据你的答题情况生成学习评价与改进建议。")


/* =====================================================
   错题知识点统计
   ===================================================== */

const knowledgeStatistics =
  computed(() => {
    const wrongList = answerHistory.value.filter(
      r => r && (r.correct === false || r.isCorrect === false)
    )
    if (wrongList.length === 0) return []
    const map = {}
    wrongList.forEach(r => {
      const kp = r.knowledgePoint || r.category || "未分类"
      map[kp] = (map[kp] || 0) + 1
    })
    const total = wrongList.length
    return Object.entries(map)
      .map(([name, count]) => ({
        name,
        count,
        percent: Math.round((count / total) * 100)
      }))
      .sort((a, b) => b.count - a.count)
  })


/* =====================================================
   答题记录倒序
   ===================================================== */

const reversedHistory =
  computed(() => {
    return [...answerHistory.value].reverse()
  })


/* =====================================================
   选中的错题
   ===================================================== */

const selectedWrong =
  computed(() => {
    if (selectedWrongId.value === null) return null
    return wrongAnswers.value.find(
      (r, i) => (r.id || i) === selectedWrongId.value
    ) || null
  })


/* =====================================================
   选择错题
   ===================================================== */

const selectWrongAnswer =
  (record) => {
    const idx = wrongAnswers.value.indexOf(record)
    selectedWrongId.value = record.id || idx
  }


/* =====================================================
   关闭AI面板
   ===================================================== */

const closeAI =
  () => {
    emit("close")
    if (props.autoHelp?.visible) {
      emit("clear-auto-help")
    }
  }


/* =====================================================
   发送消息（普通对话）
   ===================================================== */

const sendMessage =
  async () => {
    const text = chatInput.value.trim()
    if (!text) return

    chatMessages.value.push({ role: "user", content: text })
    chatInput.value = ""

    try {
      const response = await fetch(
        "http://localhost:3000/api/ai/chat",
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ message: text })
        }
      )
      const data = await response.json()
      chatMessages.value.push({
        role: "assistant",
        content: data?.data?.content || "AI暂时无法回答，请稍后重试。"
      })
    } catch (error) {
      chatMessages.value.push({
        role: "assistant",
        content: "网络错误，请检查AI后端是否启动（端口3000）。"
      })
    }
  }


/* =====================================================
   AI分析当前题目
   ===================================================== */

const startAnalysis =
  async () => {
    if (!hasQuestion.value) return

    analysisLoading.value = true
    analysisResult.value = null

    try {
      const q = questionData.value
      const optionsText = (normalizedOptions.value || [])
        .map(o => `${o.key}. ${o.text}`)
        .join("\n")

      const response = await fetch(
        "http://localhost:3000/api/ai/question-help",
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            question: q.question,
            options: optionsText ? [optionsText] : [],
            userAnswer: q.userAnswer || "未作答",
            correctAnswer: q.correctAnswer || "未提供",
            knowledgePoint: q.knowledgePoint || q.category || ""
          })
        }
      )
      const data = await response.json()
      const content = data?.data?.content || "请参考题目解析"

      analysisResult.value = {
        judgement: q.submitted
          ? (q.correct ? "回答正确！你对这道题的知识点掌握得不错。" : "回答错误，让我们一起来分析这道题。")
          : "题目尚未提交，以下是知识点讲解。",
        knowledge: q.knowledgePoint || q.category || "本题考查的核心知识点",
        mistake: q.submitted && !q.correct ? content : "",
        steps: content ? [content] : ["请参考题目解析"],
        suggestion: "建议复习相关知识点，多做类似题目巩固。"
      }
    } catch (error) {
      analysisResult.value = {
        judgement: "AI分析失败，请检查网络连接和AI后端状态。",
        knowledge: "",
        mistake: "",
        steps: [],
        suggestion: ""
      }
    } finally {
      analysisLoading.value = false
    }
  }


/* =====================================================
   AI分析错题（从错题本进入）
   ===================================================== */

const analyzeWrongQuestion =
  async (record) => {
    if (!record) return

    analysisLoading.value = true
    activePanel.value = "analysis"

    try {
      const response = await fetch(
        "http://localhost:3000/api/ai/question-help",
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            question: record.question,
            userAnswer: record.userAnswer || "未作答",
            correctAnswer: record.correctAnswer || "未提供",
            knowledgePoint: record.knowledgePoint || ""
          })
        }
      )
      const data = await response.json()
      const content = data?.data?.content || "暂无详细解析"

      analysisResult.value = {
        judgement: "这是一道错题，让我们一起来分析。",
        knowledge: record.knowledgePoint || "本题考查的核心知识点",
        mistake: content,
        steps: ["请仔细阅读上方解析，理解错误原因。"],
        suggestion: "建议将此题加入重点复习清单，定期回顾。"
      }
    } catch (error) {
      analysisResult.value = {
        judgement: "AI分析失败，请稍后重试。",
        knowledge: "",
        mistake: "",
        steps: [],
        suggestion: ""
      }
    } finally {
      analysisLoading.value = false
    }
  }


onMounted(() => {
  // 面板初始化完成
})
</script>


<style scoped>
.ai-chat-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
}

.ai-chat {
  width: 900px;
  max-width: 95vw;
  height: 640px;
  max-height: 90vh;
  background: #fff;
  border-radius: 16px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

/* 顶部 */
.ai-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px;
  background: linear-gradient(135deg, #C8161D 0%, #a01218 100%);
  color: #fff;
  flex-shrink: 0;
}

.ai-header-left {
  display: flex;
  align-items: center;
  gap: 14px;
}

.ai-logo {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background: rgba(255,255,255,0.2);
  display: flex;
  align-items: center;
  justify-content: center;
}

.ai-logo-core {
  font-weight: 700;
  font-size: 16px;
}

.ai-title {
  font-size: 18px;
  font-weight: 600;
}

.ai-subtitle {
  font-size: 12px;
  opacity: 0.85;
  margin-top: 2px;
}

.ai-header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.ai-status {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
}

.ai-status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #4ade80;
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.close-button {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: none;
  background: rgba(255,255,255,0.15);
  color: #fff;
  font-size: 20px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s;
}

.close-button:hover {
  background: rgba(255,255,255,0.3);
}

/* 主体 */
.ai-body {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* 左侧导航 */
.ai-sidebar {
  width: 180px;
  background: #f8f9fa;
  border-right: 1px solid #e9ecef;
  display: flex;
  flex-direction: column;
  padding: 12px 0;
  flex-shrink: 0;
  overflow-y: auto;
}

.sidebar-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 18px;
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 14px;
  color: #495057;
  text-align: left;
  transition: all 0.15s;
  position: relative;
}

.sidebar-item:hover {
  background: #e9ecef;
}

.sidebar-item.active {
  background: #fff;
  color: #C8161D;
  font-weight: 600;
  border-left: 3px solid #C8161D;
}

.sidebar-icon {
  font-size: 16px;
  width: 20px;
  text-align: center;
}

.sidebar-count {
  margin-left: auto;
  background: #C8161D;
  color: #fff;
  font-size: 11px;
  padding: 2px 7px;
  border-radius: 10px;
  font-weight: 600;
}

.sidebar-divider {
  height: 1px;
  background: #e9ecef;
  margin: 8px 16px;
}

.sidebar-question-card {
  margin: 12px;
  padding: 12px;
  background: #fff;
  border-radius: 8px;
  border: 1px solid #e9ecef;
}

.sidebar-question-label {
  font-size: 11px;
  color: #868e96;
  margin-bottom: 6px;
  font-weight: 600;
}

.sidebar-question-text {
  font-size: 12px;
  color: #495057;
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* 右侧内容 */
.ai-content {
  flex: 1;
  overflow-y: auto;
  padding: 20px 24px;
}

.panel-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: 20px;
}

.panel-header h2 {
  font-size: 18px;
  font-weight: 600;
  color: #212529;
  margin: 0 0 4px 0;
}

.panel-header p {
  font-size: 13px;
  color: #868e96;
  margin: 0;
}

.analyze-button {
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  background: linear-gradient(135deg, #C8161D, #a01218);
  color: #fff;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: opacity 0.2s;
  white-space: nowrap;
}

.analyze-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.loading-spinner {
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* 聊天区域 */
.chat-messages {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 14px;
  margin-bottom: 16px;
  min-height: 200px;
}

.chat-message {
  display: flex;
  gap: 10px;
  max-width: 85%;
}

.chat-message.user {
  align-self: flex-end;
  flex-direction: row-reverse;
}

.chat-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: linear-gradient(135deg, #C8161D, #a01218);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  flex-shrink: 0;
}

.chat-bubble {
  padding: 10px 14px;
  border-radius: 12px;
  font-size: 14px;
  line-height: 1.6;
  background: #f1f3f5;
  color: #212529;
}

.chat-message.user .chat-bubble {
  background: #C8161D;
  color: #fff;
}

.chat-input-area {
  display: flex;
  gap: 10px;
  padding-top: 12px;
  border-top: 1px solid #e9ecef;
}

.chat-input-area input {
  flex: 1;
  padding: 10px 14px;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
}

.chat-input-area input:focus {
  border-color: #C8161D;
}

.send-button {
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  background: #C8161D;
  color: #fff;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.2s;
}

.send-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.chat-panel {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.normal-chat-messages {
  flex: 1;
}

/* 空状态 */
.empty-state {
  text-align: center;
  padding: 48px 20px;
  color: #868e96;
}

.empty-icon {
  font-size: 40px;
  margin-bottom: 12px;
  opacity: 0.5;
}

.empty-icon.success {
  color: #28a745;
  opacity: 1;
}

.empty-title {
  font-size: 16px;
  font-weight: 600;
  color: #495057;
  margin-bottom: 8px;
}

.empty-text {
  font-size: 13px;
  line-height: 1.6;
}

/* 题目分析卡片 */
.question-analysis-card {
  background: #f8f9fa;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 20px;
}

.question-card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.question-number {
  font-size: 13px;
  font-weight: 600;
  color: #C8161D;
}

.answer-status {
  font-size: 13px;
  font-weight: 600;
  padding: 4px 12px;
  border-radius: 6px;
}

.answer-status.correct {
  background: #d4edda;
  color: #155724;
}

.answer-status.wrong {
  background: #f8d7da;
  color: #721c24;
}

.question-title {
  font-size: 15px;
  font-weight: 500;
  line-height: 1.7;
  margin-bottom: 16px;
  color: #212529;
}

.question-options {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.question-option {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  background: #fff;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  font-size: 14px;
}

.question-option.selected {
  border-color: #C8161D;
  background: #fff5f5;
}

.question-option.correct {
  border-color: #28a745;
  background: #f0fff4;
}

.question-option.wrong {
  border-color: #dc3545;
  background: #fff5f5;
}

.option-letter {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #e9ecef;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
  flex-shrink: 0;
}

.question-option.selected .option-letter {
  background: #C8161D;
  color: #fff;
}

.question-option.correct .option-letter {
  background: #28a745;
  color: #fff;
}

.option-text {
  flex: 1;
}

.option-result {
  font-weight: 700;
  color: #28a745;
}

.option-result.wrong-icon {
  color: #dc3545;
}

/* 分析结果 */
.analysis-result {
  background: #fff;
  border: 1px solid #e9ecef;
  border-radius: 12px;
  padding: 20px;
}

.analysis-result-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e9ecef;
}

.analysis-ai-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: linear-gradient(135deg, #C8161D, #a01218);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
}

.analysis-ai-name {
  font-size: 15px;
  font-weight: 600;
  color: #212529;
}

.analysis-ai-tip {
  font-size: 12px;
  color: #868e96;
  margin-top: 2px;
}

.analysis-section {
  margin-bottom: 16px;
}

.analysis-section-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  font-weight: 600;
  color: #495057;
  margin-bottom: 8px;
}

.section-icon {
  font-size: 15px;
}

.analysis-section-content {
  font-size: 14px;
  line-height: 1.7;
  color: #212529;
  padding: 10px 14px;
  background: #f8f9fa;
  border-radius: 8px;
}

.answer-box {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  background: #f0fff4;
  border-radius: 8px;
}

.answer-label {
  background: #28a745;
  color: #fff;
  padding: 4px 10px;
  border-radius: 6px;
  font-weight: 600;
  font-size: 13px;
}

.knowledge-box {
  padding: 12px 14px;
  background: #f8f9fa;
  border-radius: 8px;
}

.knowledge-name {
  font-weight: 600;
  color: #C8161D;
  margin-bottom: 6px;
  font-size: 14px;
}

.knowledge-description {
  font-size: 13px;
  line-height: 1.6;
  color: #495057;
}

.mistake-box {
  padding: 12px 14px;
  background: #fff5f5;
  border-radius: 8px;
  border-left: 3px solid #dc3545;
}

.mistake-title {
  font-weight: 600;
  color: #dc3545;
  margin-bottom: 6px;
  font-size: 14px;
}

.mistake-content {
  font-size: 13px;
  line-height: 1.6;
  color: #495057;
}

.steps {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.step-item {
  display: flex;
  gap: 10px;
  align-items: flex-start;
}

.step-number {
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: #C8161D;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
  flex-shrink: 0;
  margin-top: 1px;
}

.step-content {
  font-size: 13px;
  line-height: 1.6;
  color: #495057;
  flex: 1;
}

.suggestion-box {
  padding: 12px 14px;
  background: #fffbeb;
  border-radius: 8px;
  border-left: 3px solid #f59e0b;
  font-size: 13px;
  line-height: 1.6;
  color: #495057;
}

.chat-section {
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid #e9ecef;
}

.chat-title {
  font-size: 14px;
  font-weight: 600;
  color: #495057;
  margin-bottom: 12px;
}

/* 自动错题提示 */
.auto-help-banner {
  display: flex;
  gap: 12px;
  padding: 14px 16px;
  background: #fffbeb;
  border: 1px solid #fbbf24;
  border-radius: 10px;
  margin-bottom: 16px;
}

.auto-help-banner-icon {
  font-size: 22px;
}

.auto-help-banner-title {
  font-weight: 600;
  color: #92400e;
  font-size: 14px;
  margin-bottom: 4px;
}

.auto-help-banner-text {
  font-size: 13px;
  color: #78350f;
  line-height: 1.5;
}

/* 错题本 */
.wrong-total {
  font-size: 13px;
  color: #868e96;
  font-weight: 600;
}

.wrong-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.wrong-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 14px;
  background: #fff;
  border: 1px solid #e9ecef;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.15s;
}

.wrong-item:hover {
  border-color: #C8161D;
  box-shadow: 0 2px 8px rgba(200,22,29,0.1);
}

.wrong-item.active {
  border-color: #C8161D;
  background: #fff5f5;
}

.wrong-item-number {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  background: #f8d7da;
  color: #721c24;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
  flex-shrink: 0;
}

.wrong-item-content {
  flex: 1;
  min-width: 0;
}

.wrong-item-question {
  font-size: 14px;
  font-weight: 500;
  color: #212529;
  margin-bottom: 6px;
  line-height: 1.5;
}

.wrong-item-meta {
  display: flex;
  gap: 16px;
  font-size: 12px;
  color: #868e96;
  flex-wrap: wrap;
}

.wrong-item-knowledge {
  margin-top: 6px;
  font-size: 12px;
  color: #C8161D;
  font-weight: 500;
}

.wrong-arrow {
  color: #adb5bd;
  font-size: 16px;
  flex-shrink: 0;
}

.wrong-detail {
  margin-top: 16px;
  padding: 16px;
  background: #f8f9fa;
  border-radius: 10px;
}

.wrong-detail-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
}

.detail-label {
  font-size: 12px;
  color: #868e96;
  font-weight: 600;
  margin-bottom: 4px;
}

.detail-title {
  font-size: 15px;
  font-weight: 600;
  color: #212529;
  line-height: 1.5;
}

.detail-close {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: none;
  background: #e9ecef;
  cursor: pointer;
  font-size: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.detail-answer-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  margin-bottom: 12px;
}

.detail-answer {
  padding: 10px 12px;
  border-radius: 8px;
  font-size: 13px;
}

.detail-answer span {
  display: block;
  font-size: 11px;
  color: #868e96;
  margin-bottom: 4px;
}

.detail-answer strong {
  font-size: 14px;
}

.wrong-answer {
  background: #fff5f5;
  color: #dc3545;
}

.correct-answer {
  background: #f0fff4;
  color: #28a745;
}

.detail-knowledge {
  padding: 10px 12px;
  background: #fff;
  border-radius: 8px;
  margin-bottom: 12px;
}

.detail-knowledge-title {
  font-size: 12px;
  color: #868e96;
  font-weight: 600;
  margin-bottom: 4px;
}

.detail-analyze-button {
  width: 100%;
  padding: 10px;
  border: none;
  border-radius: 8px;
  background: linear-gradient(135deg, #C8161D, #a01218);
  color: #fff;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
}

/* 学习情况 */
.statistics-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  margin-bottom: 20px;
}

.stat-card {
  background: #f8f9fa;
  border-radius: 10px;
  padding: 16px;
  text-align: center;
}

.stat-icon {
  font-size: 22px;
  margin-bottom: 8px;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #212529;
}

.stat-value.success-text {
  color: #28a745;
}

.stat-value.danger-text {
  color: #dc3545;
}

.stat-label {
  font-size: 12px;
  color: #868e96;
  margin-top: 4px;
}

.study-evaluation {
  background: #fff;
  border: 1px solid #e9ecef;
  border-radius: 10px;
  padding: 16px;
  margin-bottom: 20px;
}

.evaluation-title {
  font-size: 14px;
  font-weight: 600;
  color: #495057;
  margin-bottom: 8px;
}

.evaluation-content {
  font-size: 13px;
  line-height: 1.7;
  color: #495057;
}

.knowledge-stat-card {
  background: #fff;
  border: 1px solid #e9ecef;
  border-radius: 10px;
  padding: 16px;
}

.knowledge-stat-title {
  font-size: 14px;
  font-weight: 600;
  color: #495057;
  margin-bottom: 12px;
}

.knowledge-empty {
  text-align: center;
  padding: 20px;
  color: #adb5bd;
  font-size: 13px;
}

.knowledge-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.knowledge-stat-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.knowledge-stat-info {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
  color: #495057;
}

.progress-bar {
  height: 8px;
  background: #e9ecef;
  border-radius: 4px;
  overflow: hidden;
}

.progress-bar-inner {
  height: 100%;
  background: linear-gradient(90deg, #C8161D, #e74c3c);
  border-radius: 4px;
  transition: width 0.3s;
}

/* 答题记录 */
.history-count {
  font-size: 13px;
  color: #868e96;
  font-weight: 600;
}

.history-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.history-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 14px;
  background: #fff;
  border: 1px solid #e9ecef;
  border-radius: 10px;
}

.history-status {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 700;
  flex-shrink: 0;
}

.history-status.correct {
  background: #d4edda;
  color: #155724;
}

.history-status.wrong {
  background: #f8d7da;
  color: #721c24;
}

.history-content {
  flex: 1;
  min-width: 0;
}

.history-question {
  font-size: 14px;
  font-weight: 500;
  color: #212529;
  margin-bottom: 6px;
  line-height: 1.5;
}

.history-meta {
  display: flex;
  gap: 16px;
  font-size: 12px;
  color: #868e96;
  flex-wrap: wrap;
}
</style>