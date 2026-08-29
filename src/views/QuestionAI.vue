<template>

  <div class="question-ai">

    <div class="question-card">

      <!-- ============================== -->
      <!-- 做题进度 -->
      <!-- ============================== -->

      <div class="progress">
        第 {{ currentIndex + 1 }} / {{ questions.length }} 题
      </div>


      <!-- ============================== -->
      <!-- 当前题目 -->
      <!-- ============================== -->

      <div class="title">
        当前题目
      </div>


      <div class="question">
        {{ currentQuestion.question }}
      </div>


      <!-- ============================== -->
      <!-- 重新练习提示 -->
      <!-- ============================== -->

      <div
        v-if="isRetryQuestion"
        class="retry-notice"
      >

        📕 正在重新练习错题

      </div>


      <!-- ============================== -->
      <!-- 选项 -->
      <!-- ============================== -->

      <div class="options">

        <label
          v-for="option in currentQuestion.options"
          :key="option.key"
          class="option"
          :class="{
            selected:
              userAnswer === option.key
          }"
        >

          <input
            v-model="userAnswer"
            type="radio"
            :value="option.key"
            :disabled="submitted"
          />


          <span>

            {{ option.key }}.
            {{ option.text }}

          </span>

        </label>

      </div>


      <!-- ============================== -->
      <!-- 提交答案 -->
      <!-- ============================== -->

      <button
        v-if="!submitted"
        class="submit-button"
        @click="submitAnswer"
      >
        提交答案
      </button>


      <!-- ============================== -->
      <!-- 答题结果 -->
      <!-- ============================== -->

      <div
        v-if="submitted"
        class="result"
      >

        <div
          v-if="isCorrect"
          class="correct"
        >
          回答正确
        </div>


        <div
          v-else
          class="wrong"
        >
          回答错误
        </div>


        <!-- ============================== -->
        <!-- 知识点 -->
        <!-- ============================== -->

        <div class="knowledge">

          <div class="knowledge-title">
            本题知识点
          </div>

          <div class="knowledge-content">
            {{ currentQuestion.knowledgePoint }}
          </div>

        </div>


        <!-- ============================== -->
        <!-- 下一题 -->
        <!-- ============================== -->

        <button
          v-if="
            currentIndex <
            questions.length - 1
          "
          class="next-button"
          @click="nextQuestion"
        >
          下一题
        </button>


        <!-- ============================== -->
        <!-- 重新开始 -->
        <!-- ============================== -->

        <button
          v-else
          class="next-button"
          @click="restartQuestions"
        >
          重新开始
        </button>

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

  /*
   * AI错题本点击“重新练习”
   * 会把题目传到这里。
   */

  retryQuestion: {

    type: Object,

    default:
      null

  }

})


/* =====================================================
   事件
   ===================================================== */

const emit =
  defineEmits([

    "update-question",

    "answer-record",

    "update-records",

    "wrong-answer"

  ])


/* =====================================================
   答题记录存储Key
   ===================================================== */

const STORAGE_KEY =
  "ai_answer_records"


/* =====================================================
   原始题目
   ===================================================== */

const questions =
  ref([

    {

      question:
        "下列哪一项属于党支部的基本任务？",

      options: [

        {
          key:
            "A",

          text:
            "负责所有党员的行政管理工作"

        },

        {
          key:
            "B",

          text:
            "负责党员个人经济收入管理"

        },

        {
          key:
            "C",

          text:
            "组织党员认真学习党的理论和路线方针政策"

        },

        {
          key:
            "D",

          text:
            "负责所有群众的日常生活"

        }

      ],

      correctAnswer:
        "C",

      knowledgePoint:
        "党支部的基本任务",

      knowledgePointId:
        "KP0018"

    },


    {

      question:
        "党员必须履行的第一项义务是什么？",

      options: [

        {

          key:
            "A",

          text:
            "认真学习党的理论和路线方针政策"

        },

        {

          key:
            "B",

          text:
            "参加所有社会活动"

        },

        {

          key:
            "C",

          text:
            "负责党组织全部行政工作"

        },

        {

          key:
            "D",

          text:
            "管理其他党员"

        }

      ],

      correctAnswer:
        "A",

      knowledgePoint:
        "党员义务",

      knowledgePointId:
        "KP0021"

    },


    {

      question:
        "党的根本组织原则是什么？",

      options: [

        {

          key:
            "A",

          text:
            "民主集中制"

        },

        {

          key:
            "B",

          text:
            "个人负责制"

        },

        {

          key:
            "C",

          text:
            "多数服从少数"

        },

        {

          key:
            "D",

          text:
            "自由组织原则"

        }

      ],

      correctAnswer:
        "A",

      knowledgePoint:
        "民主集中制",

      knowledgePointId:
        "KP0035"

    },


    {

      question:
        "党的最高理想和最终目标是什么？",

      options: [

        {

          key:
            "A",

          text:
            "全面建成社会主义现代化强国"

        },

        {

          key:
            "B",

          text:
            "实现共产主义"

        },

        {

          key:
            "C",

          text:
            "实现共同富裕"

        },

        {

          key:
            "D",

          text:
            "推进中国式现代化"

        }

      ],

      correctAnswer:
        "B",

      knowledgePoint:
        "党的最高理想和最终目标",

      knowledgePointId:
        "KP0042"

    }

  ])


/* =====================================================
   当前题目状态
   ===================================================== */

const currentIndex =
  ref(0)


const userAnswer =
  ref("")


const submitted =
  ref(false)


const isCorrect =
  ref(false)


/* =====================================================
   是否为重新练习
   ===================================================== */

const isRetryQuestion =
  ref(false)


/* =====================================================
   全部答题记录
   ===================================================== */

const answerRecords =
  ref([])


/* =====================================================
   当前题目
   ===================================================== */

const currentQuestion =
  computed(() => {

    return (

      questions.value[
        currentIndex.value
      ] ||

      {

        question:
          "",

        options:
          [],

        correctAnswer:
          "",

        knowledgePoint:
          "",

        knowledgePointId:
          ""

      }

    )

  })


/* =====================================================
   读取记录
   ===================================================== */

const loadAnswerRecords =
  () => {

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
          records

      } else {

        answerRecords.value =
          []

      }

    } catch (error) {

      console.error(
        "读取答题记录失败：",
        error
      )

      answerRecords.value =
        []

    }

  }


/* =====================================================
   保存记录
   ===================================================== */

const saveAnswerRecords =
  () => {

    try {

      localStorage.setItem(

        STORAGE_KEY,

        JSON.stringify(
          answerRecords.value
        )

      )


      emit(

        "update-records",

        [
          ...answerRecords.value
        ]

      )

    } catch (error) {

      console.error(
        "保存答题记录失败：",
        error
      )

    }

  }


/* =====================================================
   创建唯一记录ID
   ===================================================== */

const createRecordId =
  () => {

    return (

      Date.now() +

      "_" +

      currentIndex.value +

      "_" +

      Math.random()

        .toString(36)

        .substring(
          2,
          9
        )

    )

  }


/* =====================================================
   保存当前答题
   ===================================================== */

const saveCurrentAnswerRecord =
  () => {

    const record = {

      id:
        createRecordId(),

      questionIndex:
        currentIndex.value,

      questionNumber:
        currentIndex.value + 1,

      question:
        currentQuestion.value.question,

      options:
        currentQuestion.value.options,

      userAnswer:
        userAnswer.value,

      correctAnswer:
        currentQuestion.value
          .correctAnswer,

      correct:
        isCorrect.value,

      knowledgePoint:
        currentQuestion.value
          .knowledgePoint,

      knowledgePointId:
        currentQuestion.value
          .knowledgePointId,

      answerTime:
        new Date()
          .toISOString(),

      isRetry:
        isRetryQuestion.value

    }


    answerRecords.value.push(
      record
    )


    saveAnswerRecords()


    emit(
      "answer-record",
      record
    )


    console.log(
      "================================"
    )

    console.log(
      "新增答题记录：",
      record
    )

    console.log(
      "累计答题数量：",
      answerRecords.value.length
    )

    console.log(
      "================================"
    )

  }


/* =====================================================
   更新当前题目
   ===================================================== */

const updateCurrentQuestion =
  () => {

    emit(

      "update-question",

      {

        question:
          currentQuestion.value
            .question,

        options:
          currentQuestion.value
            .options
            .map(

              item =>
                `${item.key}. ${item.text}`

            ),

        userAnswer:
          userAnswer.value,

        correctAnswer:
          currentQuestion.value
            .correctAnswer,

        knowledgePoint:
          currentQuestion.value
            .knowledgePoint,

        knowledgePointId:
          currentQuestion.value
            .knowledgePointId,

        submitted:
          submitted.value,

        correct:
          isCorrect.value,

        questionIndex:
          currentIndex.value,

        questionNumber:
          currentIndex.value + 1,

        totalQuestions:
          questions.value.length,

        completedCount:
          answerRecords.value.length,

        answerRecords:
          [
            ...answerRecords.value
          ],

        isRetry:
          isRetryQuestion.value

      }

    )

  }


/* =====================================================
   提交答案
   ===================================================== */

const submitAnswer =
  () => {

    if (
      !userAnswer.value
    ) {

      alert(
        "请选择一个答案"
      )

      return

    }


    if (
      submitted.value
    ) {

      return

    }


    submitted.value =
      true


    isCorrect.value =

      userAnswer.value ===

      currentQuestion.value
        .correctAnswer


    saveCurrentAnswerRecord()


    updateCurrentQuestion()


    /*
     * 如果答错，
     * 通知App弹出AI帮助。
     */

    if (
      !isCorrect.value
    ) {

      emit(

        "wrong-answer",

        {

          question:
            currentQuestion.value.question,

          options:
            currentQuestion.value.options,

          userAnswer:
            userAnswer.value,

          correctAnswer:
            currentQuestion.value
              .correctAnswer,

          knowledgePoint:
            currentQuestion.value
              .knowledgePoint,

          knowledgePointId:
            currentQuestion.value
              .knowledgePointId,

          submitted:
            true,

          correct:
            false

        }

      )

    }

  }


/* =====================================================
   下一题
   ===================================================== */

const nextQuestion =
  () => {

    if (

      currentIndex.value <

      questions.value.length - 1

    ) {

      currentIndex.value++

      userAnswer.value =
        ""

      submitted.value =
        false

      isCorrect.value =
        false

      isRetryQuestion.value =
        false

      updateCurrentQuestion()

    }

  }


/* =====================================================
   重新开始
   ===================================================== */

const restartQuestions =
  () => {

    currentIndex.value =
      0

    userAnswer.value =
      ""

    submitted.value =
      false

    isCorrect.value =
      false

    isRetryQuestion.value =
      false

    updateCurrentQuestion()

  }


/* =====================================================
   查找题目
   ===================================================== */

const findQuestionIndex =
  (question) => {

    if (
      !question ||
      !question.question
    ) {

      return -1

    }


    return questions.value.findIndex(

      item =>

        item.question ===
        question.question &&

        item.correctAnswer ===
        question.correctAnswer

    )

  }


/* =====================================================
   加载重新练习题目
   ===================================================== */

const loadRetryQuestion =
  (question) => {

    if (
      !question ||
      !question.question
    ) {

      return

    }


    console.log(
      "QuestionAI加载重新练习题目：",
      question
    )


    /*
     * 先查找原题。
     */

    let index =
      findQuestionIndex(
        question
      )


    /*
     * 如果原题不在当前题库，
     * 就把它临时加入题库。
     */

    if (
      index === -1
    ) {

      const normalizedOptions =

        Array.isArray(
          question.options
        )

          ?

          question.options.map(

            option => {

              if (
                typeof option ===
                "string"
              ) {

                const match =
                  option.match(
                    /^([A-Z])\.\s*(.*)$/
                  )


                if (match) {

                  return {

                    key:
                      match[1],

                    text:
                      match[2]

                  }

                }


                return {

                  key:
                    "",

                  text:
                    option

                }

              }


              return {

                key:
                  option.key,

                text:
                  option.text

              }

            }

          )

          :

          []


      questions.value.push({

        question:
          question.question,

        options:
          normalizedOptions,

        correctAnswer:
          question.correctAnswer,

        knowledgePoint:
          question.knowledgePoint,

        knowledgePointId:
          question.knowledgePointId

      })


      index =
        questions.value.length - 1

    }


    /*
     * 切换到这道题。
     */

    currentIndex.value =
      index


    /*
     * 重新练习必须清空用户之前的答案。
     */

    userAnswer.value =
      ""

    submitted.value =
      false

    isCorrect.value =
      false

    isRetryQuestion.value =
      true


    updateCurrentQuestion()


    /*
     * 滚动回顶部。
     */

    window.scrollTo({

      top:
        0,

      behavior:
        "smooth"

    })

  }


/* =====================================================
   监听AI重新练习请求
   ===================================================== */

watch(

  () =>
    props.retryQuestion,

  question => {

    if (
      question &&
      question.question
    ) {

      loadRetryQuestion(
        question
      )

    }

  },

  {
    deep:
      true

  }

)


/* =====================================================
   初始化
   ===================================================== */

onMounted(

  () => {

    loadAnswerRecords()


    emit(

      "update-records",

      [
        ...answerRecords.value
      ]

    )


    updateCurrentQuestion()


    console.log(
      "历史答题记录：",
      answerRecords.value
    )


    console.log(
      "历史答题数量：",
      answerRecords.value.length
    )

  }

)


/* =====================================================
   状态监听
   ===================================================== */

watch(

  [

    userAnswer,

    submitted,

    isCorrect,

    currentIndex

  ],

  () => {

    updateCurrentQuestion()

  }

)

</script>


<style scoped>

.question-ai {

  min-height:
    100vh;

  padding:
    40px;

  background:
    #f5f7fa;

}


.question-card {

  max-width:
    800px;

  margin:
    0 auto;

  padding:
    30px;

  background:
    white;

  border-radius:
    16px;

  box-shadow:
    0 10px 30px
    rgba(
      0,
      0,
      0,
      0.08
    );

}


.progress {

  margin-bottom:
    15px;

  color:
    #409eff;

  font-size:
    14px;

  font-weight:
    600;

}


.title {

  font-size:
    24px;

  font-weight:
    700;

  margin-bottom:
    25px;

}


.question {

  font-size:
    20px;

  line-height:
    1.7;

  margin-bottom:
    25px;

}


/* ==================================================
   重新练习提示
   ================================================== */

.retry-notice {

  margin-bottom:
    20px;

  padding:
    10px 14px;

  border-radius:
    8px;

  background:
    #fff8e8;

  border:
    1px solid #f5d78e;

  color:
    #b88230;

  font-size:
    13px;

  font-weight:
    600;

}


.options {

  display:
    flex;

  flex-direction:
    column;

  gap:
    14px;

}


.option {

  display:
    flex;

  align-items:
    center;

  gap:
    12px;

  padding:
    16px;

  border:
    1px solid #ddd;

  border-radius:
    10px;

  cursor:
    pointer;

  transition:
    border-color 0.2s,
    background 0.2s;

}


.option:hover {

  border-color:
    #409eff;

}


.option.selected {

  border-color:
    #409eff;

  background:
    #ecf5ff;

}


.option input {

  width:
    18px;

  height:
    18px;

}


.submit-button,
.next-button {

  margin-top:
    25px;

  width:
    100%;

  padding:
    14px;

  border:
    none;

  border-radius:
    10px;

  background:
    #409eff;

  color:
    white;

  font-size:
    16px;

  cursor:
    pointer;

}


.submit-button:hover,
.next-button:hover {

  background:
    #337ecc;

}


.result {

  margin-top:
    25px;

}


.correct {

  padding:
    15px;

  border-radius:
    10px;

  background:
    #f0f9eb;

  color:
    #67c23a;

}


.wrong {

  padding:
    15px;

  border-radius:
    10px;

  background:
    #fef0f0;

  color:
    #f56c6c;

}


.knowledge {

  margin-top:
    15px;

  padding:
    15px;

  border-radius:
    10px;

  background:
    #f5f7fa;

}


.knowledge-title {

  margin-bottom:
    8px;

  font-weight:
    700;

}


.knowledge-content {

  color:
    #606266;

  line-height:
    1.6;

}

</style>