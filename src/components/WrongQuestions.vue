<template>
  <div class="wrong-question-page">

    <!-- =====================================================
         页面顶部
         ===================================================== -->

    <div class="page-header">

      <div>
        <div class="page-title">
          错题本
        </div>

        <div class="page-subtitle">
          自动整理你的错题，针对薄弱知识点进行强化学习
        </div>
      </div>

      <button
        class="refresh-button"
        @click="loadWrongQuestions"
      >
        ↻ 刷新错题
      </button>

    </div>


    <!-- =====================================================
         数据统计
         ===================================================== -->

    <div class="statistics">

      <div class="stat-card">

        <div class="stat-number">
          {{ wrongQuestions.length }}
        </div>

        <div class="stat-label">
          错题总数
        </div>

      </div>


      <div class="stat-card">

        <div class="stat-number danger">
          {{ unansweredCount }}
        </div>

        <div class="stat-label">
          未掌握
        </div>

      </div>


      <div class="stat-card">

        <div class="stat-number success">
          {{ masteredCount }}
        </div>

        <div class="stat-label">
          已掌握
        </div>

      </div>


      <div class="stat-card">

        <div class="stat-number">
          {{ knowledgePointCount }}
        </div>

        <div class="stat-label">
          涉及知识点
        </div>

      </div>

    </div>


    <!-- =====================================================
         筛选区域
         ===================================================== -->

    <div class="filter-bar">

      <button
        class="filter-button"
        :class="{
          active: currentFilter === 'all'
        }"
        @click="currentFilter = 'all'"
      >
        全部错题
      </button>


      <button
        class="filter-button"
        :class="{
          active: currentFilter === 'unmastered'
        }"
        @click="currentFilter = 'unmastered'"
      >
        未掌握
      </button>


      <button
        class="filter-button"
        :class="{
          active: currentFilter === 'mastered'
        }"
        @click="currentFilter = 'mastered'"
      >
        已掌握
      </button>


      <div class="filter-info">
        当前显示 {{ filteredQuestions.length }} 道
      </div>

    </div>


    <!-- =====================================================
         空状态
         ===================================================== -->

    <div
      v-if="filteredQuestions.length === 0"
      class="empty-state"
    >

      <div class="empty-icon">
        📚
      </div>

      <div class="empty-title">
        暂时没有错题
      </div>

      <div class="empty-description">

        {{
          wrongQuestions.length === 0
            ? "完成题目后，答错的题目会自动进入错题本。"
            : "当前筛选条件下没有符合条件的错题。"
        }}

      </div>

    </div>


    <!-- =====================================================
         错题列表
         ===================================================== -->

    <div
      v-else
      class="wrong-question-list"
    >

      <div
        v-for="(item, index) in filteredQuestions"
        :key="item.id"
        class="wrong-question-card"
      >

        <!-- =================================================
             卡片顶部
             ================================================= -->

        <div class="question-card-header">

          <div class="question-number">
            错题 {{ index + 1 }}
          </div>


          <div
            class="master-status"
            :class="{
              mastered: isMastered(item.id)
            }"
          >

            {{
              isMastered(item.id)
                ? "✓ 已掌握"
                : "● 未掌握"
            }}

          </div>

        </div>


        <!-- =================================================
             题目
             ================================================= -->

        <div class="question-content">

          <div class="question-text">
            {{ item.question }}
          </div>


          <!-- =================================================
               选项
               ================================================= -->

          <div
            v-if="
              item.options &&
              item.options.length > 0
            "
            class="options"
          >

            <div
              v-for="(option, optionIndex) in item.options"
              :key="optionIndex"
              class="option"
              :class="{
                'correct-option':
                  getOptionKey(option) === item.correctAnswer,

                'wrong-option':
                  getOptionKey(option) === item.userAnswer
              }"
            >

              {{ option }}

            </div>

          </div>


          <!-- =================================================
               答题信息
               ================================================= -->

          <div class="answer-info">

            <div class="answer-row">

              <span class="info-label">
                我的答案
              </span>

              <span class="wrong-answer">
                {{ item.userAnswer || "未作答" }}
              </span>

            </div>


            <div class="answer-row">

              <span class="info-label">
                正确答案
              </span>

              <span class="correct-answer">
                {{ item.correctAnswer }}
              </span>

            </div>


            <div class="answer-row">

              <span class="info-label">
                知识点
              </span>

              <span class="knowledge-point">
                {{ item.knowledgePoint || "暂无知识点" }}
              </span>

            </div>


            <div
              v-if="item.knowledgePointId"
              class="answer-row"
            >

              <span class="info-label">
                知识点编号
              </span>

              <span class="knowledge-id">
                {{ item.knowledgePointId }}
              </span>

            </div>


            <div
              v-if="item.answerTime"
              class="answer-row"
            >

              <span class="info-label">
                答题时间
              </span>

              <span class="answer-time">
                {{ formatTime(item.answerTime) }}
              </span>

            </div>

          </div>

        </div>


        <!-- =================================================
             操作按钮
             ================================================= -->

        <div class="question-actions">

          <!-- AI解析 -->

          <button
            class="ai-button"
            :disabled="
              loading &&
              currentAIQuestionId === item.id
            "
            @click="analyzeQuestion(item)"
          >

            <span
              v-if="
                loading &&
                currentAIQuestionId === item.id
            >
              AI分析中...
            </span>

            <span v-else>
              🤖 AI解析
            </span>

          </button>


          <!-- 标记掌握 -->

          <button
            v-if="!isMastered(item.id)"
            class="master-button"
            @click="markMastered(item)"
          >
            ✓ 标记已掌握
          </button>


          <!-- 取消掌握 -->

          <button
            v-else
            class="unmaster-button"
            @click="markUnmastered(item)"
          >
            ↺ 重新练习
          </button>

        </div>


        <!-- =================================================
             AI解析结果
             ================================================= -->

        <div
          v-if="
            aiAnswer &&
            currentAIQuestionId === item.id
          "
          class="ai-answer"
        >

          <div class="ai-answer-header">

            <div class="ai-answer-title">
              🤖 AI错题解析
            </div>

            <button
              class="close-ai-button"
              @click="closeAIAnswer"
            >
              ×
            </button>

          </div>


          <div class="ai-answer-content">
            {{ aiAnswer }}
          </div>

        </div>

      </div>

    </div>


    <!-- =====================================================
         页面底部提示
         ===================================================== -->

    <div
      v-if="wrongQuestions.length > 0"
      class="bottom-tip"
    >

      💡 建议优先复习“未掌握”错题，并结合 AI 解析强化对应知识点。

    </div>

  </div>
</template>


<script setup>

import {
  ref,
  computed,
  onMounted
} from "vue"


/* =========================================================
   答题记录存储 Key

   与 QuestionAI.vue 保持完全一致
   ========================================================= */

const STORAGE_KEY =
  "ai_answer_records"


/* =========================================================
   错题掌握状态存储 Key
   ========================================================= */

const MASTERED_KEY =
  "ai_mastered_questions"


/* =========================================================
   错题数据
   ========================================================= */

const wrongQuestions =
  ref([])


/* =========================================================
   掌握状态

   保存的是已经掌握的错题 id
   ========================================================= */

const masteredQuestions =
  ref([])


/* =========================================================
   当前筛选
   ========================================================= */

const currentFilter =
  ref("all")


/* =========================================================
   AI回答
   ========================================================= */

const aiAnswer =
  ref("")


/* =========================================================
   当前正在AI分析的题目
   ========================================================= */

const currentAIQuestionId =
  ref("")


/* =========================================================
   AI加载状态
   ========================================================= */

const loading =
  ref(false)


/* =========================================================
   读取答题记录
   ========================================================= */

const loadWrongQuestions =
  () => {

    try {

      const data =
        localStorage.getItem(
          STORAGE_KEY
        )


      if (!data) {

        wrongQuestions.value =
          []

        return

      }


      const records =
        JSON.parse(data)


      if (
        !Array.isArray(records)
      ) {

        wrongQuestions.value =
          []

        return

      }


      /*
       * 只保留答错的题目
       */

      wrongQuestions.value =
        records.filter(
          item =>
            item &&
            item.correct === false
        )

    } catch (error) {

      console.error(
        "读取错题失败：",
        error
      )

      wrongQuestions.value =
        []

    }

  }


/* =========================================================
   读取掌握状态
   ========================================================= */

const loadMasteredQuestions =
  () => {

    try {

      const data =
        localStorage.getItem(
          MASTERED_KEY
        )


      if (!data) {

        masteredQuestions.value =
          []

        return

      }


      const records =
        JSON.parse(data)


      if (
        Array.isArray(records)
      ) {

        masteredQuestions.value =
          records

      } else {

        masteredQuestions.value =
          []

      }

    } catch (error) {

      console.error(
        "读取错题掌握状态失败：",
        error
      )

      masteredQuestions.value =
        []

    }

  }


/* =========================================================
   保存掌握状态
   ========================================================= */

const saveMasteredQuestions =
  () => {

    try {

      localStorage.setItem(
        MASTERED_KEY,
        JSON.stringify(
          masteredQuestions.value
        )
      )

    } catch (error) {

      console.error(
        "保存错题掌握状态失败：",
        error
      )

    }

  }


/* =========================================================
   判断题目是否已经掌握
   ========================================================= */

const isMastered =
  (id) => {

    return masteredQuestions.value
      .includes(id)

  }


/* =========================================================
   标记已掌握
   ========================================================= */

const markMastered =
  (item) => {

    if (!item || !item.id) {

      return

    }


    if (
      !masteredQuestions.value.includes(
        item.id
      )
    ) {

      masteredQuestions.value.push(
        item.id
      )

    }


    saveMasteredQuestions()

  }


/* =========================================================
   标记重新练习
   ========================================================= */

const markUnmastered =
  (item) => {

    if (!item || !item.id) {

      return

    }


    masteredQuestions.value =
      masteredQuestions.value.filter(
        id =>
          id !== item.id
      )


    saveMasteredQuestions()

  }


/* =========================================================
   错题总数
   ========================================================= */

const wrongCount =
  computed(() => {

    return wrongQuestions.value.length

  })


/* =========================================================
   已掌握数量
   ========================================================= */

const masteredCount =
  computed(() => {

    return wrongQuestions.value.filter(
      item =>
        isMastered(item.id)
    ).length

  })


/* =========================================================
   未掌握数量
   ========================================================= */

const unansweredCount =
  computed(() => {

    return wrongQuestions.value.filter(
      item =>
        !isMastered(item.id)
    ).length

  })


/* =========================================================
   知识点数量
   ========================================================= */

const knowledgePointCount =
  computed(() => {

    const set =
      new Set()


    wrongQuestions.value.forEach(
      item => {

        if (
          item.knowledgePoint
        ) {

          set.add(
            item.knowledgePoint
          )

        }

      }
    )


    return set.size

  })


/* =========================================================
   筛选后的错题
   ========================================================= */

const filteredQuestions =
  computed(() => {

    if (
      currentFilter.value ===
      "unmastered"
    ) {

      return wrongQuestions.value
        .filter(
          item =>
            !isMastered(item.id)
        )

    }


    if (
      currentFilter.value ===
      "mastered"
    ) {

      return wrongQuestions.value
        .filter(
          item =>
            isMastered(item.id)
        )

    }


    return wrongQuestions.value

  })


/* =========================================================
   获取选项 Key

   兼容：

   A. xxx

   以及

   { key: "A", text: "xxx" }

   两种格式
   ========================================================= */

const getOptionKey =
  (option) => {

    if (
      option &&
      typeof option === "object"
    ) {

      return option.key

    }


    if (
      typeof option !== "string"
    ) {

      return ""

    }


    const match =
      option.match(
        /^([A-Z])[.．、\s]/
      )


    if (match) {

      return match[1]

    }


    return option.trim()

  }


/* =========================================================
   格式化时间
   ========================================================= */

const formatTime =
  (time) => {

    if (!time) {

      return ""

    }


    try {

      const date =
        new Date(time)


      if (
        Number.isNaN(
          date.getTime()
        )
      ) {

        return time

      }


      return date.toLocaleString(
        "zh-CN",
        {
          year: "numeric",
          month: "2-digit",
          day: "2-digit",
          hour: "2-digit",
          minute: "2-digit"
        }
      )

    } catch {

      return time

    }

  }


/* =========================================================
   AI解析错题
   ========================================================= */

const analyzeQuestion =
  async (item) => {

    if (!item) {

      return

    }


    loading.value =
      true


    currentAIQuestionId.value =
      item.id


    aiAnswer.value =
      ""


    try {

      /*
       * 整理选项
       */

      let optionsText =
        ""


      if (
        Array.isArray(
          item.options
        )
      ) {

        optionsText =
          item.options
            .map(
              option => {

                if (
                  typeof option ===
                  "object"
                ) {

                  return `${option.key}. ${option.text}`

                }

                return option

              }
            )
            .join("\n")

      }


      /* =================================================
         System Prompt
         ================================================= */

      const systemPrompt = `
你是一名数智党校AI学习助手。

你的任务是帮助用户分析错题，而不是简单告诉用户答案。

请做到：

1. 明确指出这道题考查的核心知识点。
2. 明确说明正确答案。
3. 解释为什么正确答案正确。
4. 分析用户为什么会答错。
5. 对错误选项进行必要的辨析。
6. 对相关知识点进行简洁但完整的讲解。
7. 最后给出记忆方法或者学习建议。
8. 不要编造不存在的党内法规、文件名称或者具体条文。
9. 如果题目信息不足，应明确说明。
10. 语言要适合党校学习，清晰、准确、易理解。
`


      /* =================================================
         User Prompt
         ================================================= */

      const userPrompt = `
请帮我分析下面这道错题。

【题目】
${item.question}

【选项】
${optionsText || "暂无选项"}

【我的答案】
${item.userAnswer || "未作答"}

【正确答案】
${item.correctAnswer || "暂无"}

【知识点】
${item.knowledgePoint || "暂无"}

【知识点编号】
${item.knowledgePointId || "暂无"}

请严格按照下面的结构回答：

一、题目考查什么

二、正确答案及解析

三、我的答案为什么错误

四、选项辨析

五、相关知识点重点讲解

六、记忆方法

七、学习建议
`


      /* =================================================
         请求后端
         ================================================= */

      const response =
        await fetch(
          "http://localhost:3000/api/ai/chat",
          {
            method: "POST",

            headers: {
              "Content-Type":
                "application/json"
            },

            body:
              JSON.stringify({

                messages: [

                  {
                    role: "system",
                    content:
                      systemPrompt
                  },

                  {
                    role: "user",
                    content:
                      userPrompt
                  }

                ]

              })

          }
        )


      const result =
        await response.json()


      if (
        !result.success
      ) {

        throw new Error(
          result.message ||
          "AI请求失败"
        )

      }


      aiAnswer.value =
        result.data.content


    } catch (error) {

      console.error(
        "AI错题解析失败：",
        error
      )


      aiAnswer.value =
        "AI解析失败，请检查后端 AI 服务是否正常运行。"

    } finally {

      loading.value =
        false

    }

  }


/* =========================================================
   关闭AI解析
   ========================================================= */

const closeAIAnswer =
  () => {

    aiAnswer.value =
      ""

    currentAIQuestionId.value =
      ""

  }


/* =========================================================
   初始化
   ========================================================= */

onMounted(
  () => {

    loadWrongQuestions()

    loadMasteredQuestions()

  }
)

</script>


<style scoped>

/* =========================================================
   页面
   ========================================================= */

.wrong-question-page {

  min-height: 100%;

  padding: 30px;

  background: #f5f7fa;

  box-sizing: border-box;

}


/* =========================================================
   Header
   ========================================================= */

.page-header {

  display: flex;

  align-items: center;

  justify-content: space-between;

  margin-bottom: 25px;

}


.page-title {

  font-size: 28px;

  font-weight: 700;

  color: #303133;

}


.page-subtitle {

  margin-top: 8px;

  color: #909399;

  font-size: 14px;

}


.refresh-button {

  padding: 10px 18px;

  border: 1px solid #dcdfe6;

  border-radius: 8px;

  background: white;

  color: #606266;

  cursor: pointer;

  font-size: 14px;

}


.refresh-button:hover {

  border-color: #409eff;

  color: #409eff;

}


/* =========================================================
   Statistics
   ========================================================= */

.statistics {

  display: grid;

  grid-template-columns:
    repeat(4, 1fr);

  gap: 18px;

  margin-bottom: 25px;

}


.stat-card {

  padding: 22px;

  background: white;

  border-radius: 12px;

  border: 1px solid #ebeef5;

  text-align: center;

}


.stat-number {

  font-size: 28px;

  font-weight: 700;

  color: #409eff;

}


.stat-number.danger {

  color: #f56c6c;

}


.stat-number.success {

  color: #67c23a;

}


.stat-label {

  margin-top: 8px;

  color: #909399;

  font-size: 13px;

}


/* =========================================================
   Filter
   ========================================================= */

.filter-bar {

  display: flex;

  align-items: center;

  gap: 10px;

  margin-bottom: 20px;

}


.filter-button {

  padding: 9px 18px;

  border: 1px solid #dcdfe6;

  border-radius: 8px;

  background: white;

  color: #606266;

  cursor: pointer;

  font-size: 14px;

}


.filter-button:hover {

  border-color: #409eff;

  color: #409eff;

}


.filter-button.active {

  border-color: #409eff;

  background: #409eff;

  color: white;

}


.filter-info {

  margin-left: auto;

  color: #909399;

  font-size: 13px;

}


/* =========================================================
   Empty
   ========================================================= */

.empty-state {

  padding: 80px 20px;

  background: white;

  border-radius: 14px;

  text-align: center;

  border: 1px solid #ebeef5;

}


.empty-icon {

  font-size: 52px;

  margin-bottom: 15px;

}


.empty-title {

  font-size: 20px;

  font-weight: 700;

  color: #303133;

}


.empty-description {

  margin-top: 10px;

  color: #909399;

  font-size: 14px;

}


/* =========================================================
   List
   ========================================================= */

.wrong-question-list {

  display: flex;

  flex-direction: column;

  gap: 18px;

}


/* =========================================================
   Card
   ========================================================= */

.wrong-question-card {

  background: white;

  border-radius: 14px;

  border: 1px solid #ebeef5;

  overflow: hidden;

}


.question-card-header {

  display: flex;

  justify-content: space-between;

  align-items: center;

  padding: 15px 20px;

  background: #fafbfc;

  border-bottom: 1px solid #ebeef5;

}


.question-number {

  color: #409eff;

  font-size: 14px;

  font-weight: 700;

}


.master-status {

  padding: 5px 10px;

  border-radius: 20px;

  background: #fef0f0;

  color: #f56c6c;

  font-size: 12px;

}


.master-status.mastered {

  background: #f0f9eb;

  color: #67c23a;

}


/* =========================================================
   Question Content
   ========================================================= */

.question-content {

  padding: 20px;

}


.question-text {

  font-size: 17px;

  line-height: 1.7;

  font-weight: 600;

  color: #303133;

}


/* =========================================================
   Options
   ========================================================= */

.options {

  display: flex;

  flex-direction: column;

  gap: 8px;

  margin-top: 18px;

}


.option {

  padding: 11px 14px;

  border-radius: 8px;

  background: #f8fafc;

  border: 1px solid #ebeef5;

  color: #606266;

  font-size: 14px;

  line-height: 1.6;

}


.option.correct-option {

  background: #f0f9eb;

  border-color: #b3e19d;

  color: #67c23a;

  font-weight: 600;

}


.option.wrong-option {

  background: #fef0f0;

  border-color: #fbc4c4;

  color: #f56c6c;

}


/* =========================================================
   Answer Info
   ========================================================= */

.answer-info {

  margin-top: 20px;

  padding: 15px;

  border-radius: 10px;

  background: #f8fafc;

}


.answer-row {

  display: flex;

  align-items: flex-start;

  gap: 12px;

  padding: 7px 0;

  font-size: 13px;

  line-height: 1.5;

}


.info-label {

  width: 75px;

  flex-shrink: 0;

  color: #909399;

}


.wrong-answer {

  color: #f56c6c;

  font-weight: 600;

}


.correct-answer {

  color: #67c23a;

  font-weight: 600;

}


.knowledge-point {

  color: #409eff;

  font-weight: 600;

}


.knowledge-id {

  color: #606266;

}


.answer-time {

  color: #909399;

}


/* =========================================================
   Actions
   ========================================================= */

.question-actions {

  display: flex;

  gap: 10px;

  padding: 0 20px 20px;

}


.ai-button,
.master-button,
.unmaster-button {

  padding: 10px 16px;

  border-radius: 8px;

  cursor: pointer;

  font-size: 13px;

}


.ai-button {

  border: none;

  background: #409eff;

  color: white;

}


.ai-button:hover {

  background: #337ecc;

}


.ai-button:disabled {

  opacity: 0.6;

  cursor: not-allowed;

}


.master-button {

  border: 1px solid #67c23a;

  background: #f0f9eb;

  color: #67c23a;

}


.master-button:hover {

  background: #67c23a;

  color: white;

}


.unmaster-button {

  border: 1px solid #e6a23c;

  background: #fdf6ec;

  color: #e6a23c;

}


.unmaster-button:hover {

  background: #e6a23c;

  color: white;

}


/* =========================================================
   AI Answer
   ========================================================= */

.ai-answer {

  margin: 0 20px 20px;

  border-radius: 12px;

  border: 1px solid #b3d8ff;

  background: #f5faff;

  overflow: hidden;

}


.ai-answer-header {

  display: flex;

  align-items: center;

  justify-content: space-between;

  padding: 14px 16px;

  background: #ecf5ff;

  border-bottom: 1px solid #b3d8ff;

}


.ai-answer-title {

  color: #409eff;

  font-weight: 700;

  font-size: 14px;

}


.close-ai-button {

  width: 28px;

  height: 28px;

  border: none;

  border-radius: 6px;

  background: transparent;

  color: #909399;

  font-size: 20px;

  cursor: pointer;

}


.close-ai-button:hover {

  background: #d9ecff;

  color: #409eff;

}


.ai-answer-content {

  padding: 18px;

  white-space: pre-wrap;

  line-height: 1.8;

  color: #303133;

  font-size: 14px;

}


/* =========================================================
   Bottom Tip
   ========================================================= */

.bottom-tip {

  margin-top: 20px;

  padding: 14px 18px;

  border-radius: 10px;

  background: #fff8e8;

  border: 1px solid #f5d78e;

  color: #8c6a1d;

  font-size: 13px;

}


/* =========================================================
   Mobile
   ========================================================= */

@media (max-width: 800px) {

  .wrong-question-page {

    padding: 15px;

  }


  .page-header {

    align-items: flex-start;

    flex-direction: column;

    gap: 15px;

  }


  .statistics {

    grid-template-columns:
      repeat(2, 1fr);

  }


  .filter-bar {

    flex-wrap: wrap;

  }


  .filter-info {

    width: 100%;

    margin-left: 0;

    margin-top: 5px;

  }


  .question-actions {

    flex-wrap: wrap;

  }


  .answer-row {

    flex-direction: column;

    gap: 3px;

  }


  .info-label {

    width: auto;

  }

}

</style>