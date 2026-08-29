<template>

  <div class="app">

    <!-- ==========================================
         做题模块
         ========================================== -->

    <QuestionAI
      :retry-question="retryQuestion"
      @update-question="handleQuestionUpdate"
      @answer-record="handleAnswerRecord"
      @update-records="handleUpdateRecords"
      @wrong-answer="handleWrongAnswer"
    />


    <!-- ==========================================
         AI悬浮按钮
         ========================================== -->

    <AIFloatingButton
      v-if="!showAI && !autoHelp.visible"
      @click="openAI"
    />


    <!-- ==========================================
         AI自动错题小提示
         ========================================== -->

    <div
      v-if="
        autoHelp.visible &&
        !showAI
      "
      class="auto-help-popup"
    >

      <div class="auto-help-popup-icon">
        💡
      </div>


      <div class="auto-help-popup-content">

        <div class="auto-help-popup-title">
          发现你刚刚答错了一道题
        </div>

        <div class="auto-help-popup-text">
          需要 AI 帮你分析这道题吗？
        </div>

      </div>


      <div class="auto-help-popup-actions">

        <button
          class="auto-help-analyze"
          @click="acceptAutoHelp"
        >
          分析题目
        </button>


        <button
          class="auto-help-ignore"
          @click="ignoreAutoHelp"
        >
          暂时不用
        </button>

      </div>

    </div>


    <!-- ==========================================
         AI主界面
         ========================================== -->

    <AIChat
      v-if="showAI"
      :question-data="currentQuestion"
      :answer-history="answerRecords"
      :auto-help="autoHelp"
      @close="closeAI"
      @clear-auto-help="clearAutoHelp"
      @retry-question="handleRetryQuestion"
    />

  </div>

</template>


<script setup>

import {
  ref,
  onMounted
} from "vue"


import AIFloatingButton
  from "./components/AIFloatingButton.vue"


import AIChat
  from "./views/AIChat.vue"


import QuestionAI
  from "./views/QuestionAI.vue"


/* =====================================================
   localStorage
   ===================================================== */

const STORAGE_KEY =
  "ai_answer_records"


/* =====================================================
   AI是否打开
   ===================================================== */

const showAI =
  ref(false)


/* =====================================================
   当前题目
   ===================================================== */

const currentQuestion =
  ref(null)


/* =====================================================
   所有答题记录
   ===================================================== */

const answerRecords =
  ref([])


/* =====================================================
   AI自动错题提示
   ===================================================== */

const autoHelp =
  ref({

    visible:
      false,

    question:
      null

  })


/* =====================================================
   AI错题重新练习
   ===================================================== */

const retryQuestion =
  ref(null)


/* =====================================================
   读取历史答题记录
   ===================================================== */

const loadRecords = () => {

  try {

    const data =
      localStorage.getItem(
        STORAGE_KEY
      )


    if (!data) {

      answerRecords.value =
        []

      return

    }


    const records =
      JSON.parse(data)


    if (
      Array.isArray(records)
    ) {

      answerRecords.value =
        [
          ...records
        ]

    } else {

      answerRecords.value =
        []

    }

  } catch (error) {

    console.error(
      "App读取答题记录失败：",
      error
    )

    answerRecords.value =
      []

  }

}


/* =====================================================
   当前题目更新
   ===================================================== */

const handleQuestionUpdate =
  (data) => {

    if (!data) {

      return

    }


    currentQuestion.value =
      {
        ...data
      }


    /*
     * QuestionAI同时带来了完整记录
     */

    if (
      Array.isArray(
        data.answerRecords
      )
    ) {

      answerRecords.value =
        [
          ...data.answerRecords
        ]

    }

  }


/* =====================================================
   收到一条新的答题记录
   ===================================================== */

const handleAnswerRecord =
  (record) => {

    if (!record) {

      return

    }


    /*
     * 防止同一条记录重复添加
     */

    const exists =
      answerRecords.value.some(

        item =>
          item.id &&
          record.id &&
          item.id === record.id

      )


    if (!exists) {

      answerRecords.value =
        [

          ...answerRecords.value,

          record

        ]

    }


    console.log(
      "App收到新的答题记录：",
      record
    )


    console.log(
      "App当前累计答题：",
      answerRecords.value.length
    )

  }


/* =====================================================
   QuestionAI主动发送完整记录
   ===================================================== */

const handleUpdateRecords =
  (records) => {

    if (
      !Array.isArray(records)
    ) {

      return

    }


    answerRecords.value =
      [
        ...records
      ]


    console.log(
      "App同步完整答题记录：",
      answerRecords.value
    )


    console.log(
      "App累计答题数量：",
      answerRecords.value.length
    )

  }


/* =====================================================
   答错 → 触发AI自动提示
   ===================================================== */

const handleWrongAnswer =
  (question) => {

    if (!question) {

      return

    }


    console.log(
      "================================"
    )

    console.log(
      "检测到错题"
    )

    console.log(
      "准备显示AI分析提示：",
      question
    )

    console.log(
      "================================"
    )


    autoHelp.value =
      {

        visible:
          true,

        question:
          {
            ...question
          }

      }


    currentQuestion.value =
      {
        ...question
      }

  }


/* =====================================================
   AI打开
   ===================================================== */

const openAI = () => {

  loadRecords()


  showAI.value =
    true

}


/* =====================================================
   AI关闭
   ===================================================== */

const closeAI = () => {

  showAI.value =
    false

}


/* =====================================================
   用户点击“分析题目”
   ===================================================== */

const acceptAutoHelp = () => {

  const question =
    autoHelp.value.question


  if (question) {

    currentQuestion.value =
      {
        ...question
      }

  }


  autoHelp.value =
    {

      visible:
        false,

      question:
        null

    }


  showAI.value =
    true

}


/* =====================================================
   用户点击“暂时不用”
   ===================================================== */

const ignoreAutoHelp = () => {

  autoHelp.value =
    {

      visible:
        false,

      question:
        null

    }

}


/* =====================================================
   AI内部清除自动提示
   ===================================================== */

const clearAutoHelp = () => {

  autoHelp.value =
    {

      visible:
        false,

      question:
        null

    }

}


/* =====================================================
   AI错题本 → 重新练习
   ===================================================== */

const handleRetryQuestion =
  (question) => {

    if (!question) {

      return

    }


    console.log(
      "AI请求重新练习：",
      question
    )


    /*
     * 设置当前题目
     */

    currentQuestion.value =
      {
        ...question
      }


    /*
     * 通知QuestionAI加载这道题
     *
     * 这里使用新的对象，
     * 确保Vue watch能够检测到变化。
     */

    retryQuestion.value =
      {

        ...question,

        retryId:
          Date.now()

      }


    /*
     * 关闭AI窗口
     */

    showAI.value =
      false


    /*
     * 清除自动帮助提示
     */

    autoHelp.value =
      {

        visible:
          false,

        question:
          null

      }

  }


/* =====================================================
   初始化
   ===================================================== */

onMounted(() => {

  loadRecords()


  console.log(
    "================================"
  )

  console.log(
    "App初始化"
  )

  console.log(
    "历史答题记录：",
    answerRecords.value
  )

  console.log(
    "历史答题数量：",
    answerRecords.value.length
  )

  console.log(
    "================================"
  )

})

</script>


<style>

* {

  box-sizing:
    border-box;

}


html,
body,
#app {

  margin:
    0;

  width:
    100%;

  min-height:
    100%;

}


body {

  font-family:
    Arial,
    "Microsoft YaHei",
    sans-serif;

}


.app {

  min-height:
    100vh;

}


/* =====================================================
   AI自动错题小提示
   ===================================================== */

.auto-help-popup {

  position:
    fixed;

  right:
    30px;

  bottom:
    30px;

  z-index:
    3000;

  width:
    420px;

  padding:
    20px;

  display:
    flex;

  align-items:
    center;

  gap:
    14px;

  background:
    white;

  border:
    1px solid #f5d78e;

  border-radius:
    14px;

  box-shadow:
    0 12px 40px
    rgba(
      0,
      0,
      0,
      0.18
    );

}


.auto-help-popup-icon {

  width:
    45px;

  height:
    45px;

  flex-shrink:
    0;

  display:
    flex;

  align-items:
    center;

  justify-content:
    center;

  border-radius:
    50%;

  background:
    #fff8e8;

  font-size:
    24px;

}


.auto-help-popup-content {

  flex:
    1;

}


.auto-help-popup-title {

  font-size:
    15px;

  font-weight:
    700;

  color:
    #303133;

}


.auto-help-popup-text {

  margin-top:
    5px;

  font-size:
    13px;

  color:
    #909399;

}


.auto-help-popup-actions {

  display:
    flex;

  flex-direction:
    column;

  gap:
    7px;

}


.auto-help-analyze {

  padding:
    8px 13px;

  border:
    none;

  border-radius:
    7px;

  background:
    #409eff;

  color:
    white;

  cursor:
    pointer;

}


.auto-help-ignore {

  padding:
    7px 13px;

  border:
    1px solid #dcdfe6;

  border-radius:
    7px;

  background:
    white;

  color:
    #606266;

  cursor:
    pointer;

}


.auto-help-analyze:hover {

  background:
    #337ecc;

}


.auto-help-ignore:hover {

  background:
    #f5f7fa;

}


@media (
  max-width: 600px
) {

  .auto-help-popup {

    left:
      15px;

    right:
      15px;

    bottom:
      15px;

    width:
      auto;

  }

}

</style>