<template>
  <div class="analysis-page">

    <!-- 页面标题 -->
    <div class="page-header">

      <div>
        <h1>AI学习分析</h1>

        <p>
          根据你的历史答题数据，
          AI为你分析学习情况
        </p>
      </div>

      <button
        class="refresh-button"
        @click="handleAnalysis"
        :disabled="loading"
      >
        {{ loading ? "分析中..." : "重新分析" }}
      </button>

    </div>


    <!-- 总体数据 -->
    <div class="summary-grid">

      <div class="summary-card">

        <div class="summary-icon">
          📚
        </div>

        <div>
          <div class="summary-label">
            总答题数
          </div>

          <div class="summary-value">
            {{ summary.total }}
          </div>
        </div>

      </div>


      <div class="summary-card">

        <div class="summary-icon">
          ✓
        </div>

        <div>
          <div class="summary-label">
            正确题数
          </div>

          <div class="summary-value">
            {{ summary.correct }}
          </div>
        </div>

      </div>


      <div class="summary-card">

        <div class="summary-icon">
          📊
        </div>

        <div>
          <div class="summary-label">
            正确率
          </div>

          <div class="summary-value">
            {{ summary.accuracy }}%
          </div>
        </div>

      </div>


      <div class="summary-card">

        <div class="summary-icon">
          🎯
        </div>

        <div>
          <div class="summary-label">
            薄弱知识点
          </div>

          <div class="summary-value">
            {{ summary.weakPoints.length }}
          </div>
        </div>

      </div>

    </div>


    <!-- 没有数据 -->
    <div
      v-if="summary.total === 0"
      class="empty-card"
    >

      <div class="empty-icon">
        📚
      </div>

      <h2>
        暂时还没有学习数据
      </h2>

      <p>
        完成一些题目后，
        AI就可以根据你的答题情况进行分析。
      </p>

    </div>


    <!-- 有数据 -->
    <template v-else>

      <!-- 知识点 -->
      <div class="section">

        <div class="section-title">
          <span>📊</span>
          知识点掌握情况
        </div>

        <div class="knowledge-list">

          <div
            v-for="item in summary.knowledgePoints"
            :key="item.knowledgePoint"
            class="knowledge-item"
          >

            <div class="knowledge-top">

              <span class="knowledge-name">
                {{ item.knowledgePoint }}
              </span>

              <span
                class="accuracy"
                :class="getAccuracyClass(item.accuracy)"
              >
                {{ item.accuracy }}%
              </span>

            </div>

            <div class="progress">

              <div
                class="progress-bar"
                :class="getAccuracyClass(item.accuracy)"
                :style="{
                  width: item.accuracy + '%'
                }"
              ></div>

            </div>

            <div class="knowledge-detail">

              共 {{ item.total }} 题

              ·

              正确 {{ item.correct }} 题

              ·

              错误 {{ item.wrong }} 题

            </div>

          </div>

        </div>

      </div>


      <!-- 薄弱知识点 -->
      <div class="section">

        <div class="section-title weak-title">
          <span>🔴</span>
          重点薄弱知识点
        </div>

        <div
          v-if="summary.weakPoints.length === 0"
          class="success-box"
        >
          暂未发现明显薄弱知识点，继续保持！
        </div>

        <div
          v-else
          class="weak-list"
        >

          <div
            v-for="(item, index) in summary.weakPoints.slice(0, 3)"
            :key="item.knowledgePoint"
            class="weak-item"
          >

            <div class="rank">
              {{ index + 1 }}
            </div>

            <div class="weak-content">

              <div class="weak-name">
                {{ item.knowledgePoint }}
              </div>

              <div class="weak-info">

                正确率

                <strong>
                  {{ item.accuracy }}%
                </strong>

                ·

                错误 {{ item.wrong }} 次

              </div>

            </div>

          </div>

        </div>

      </div>


      <!-- AI分析 -->
      <div class="section ai-analysis">

        <div class="section-title">

          <span>🤖</span>

          AI学习分析

        </div>

        <div
          v-if="loading"
          class="loading"
        >

          <div class="loading-icon">
            🤖
          </div>

          <p>
            AI正在分析你的学习数据……
          </p>

        </div>

        <div
          v-else-if="analysis"
          class="analysis-content"
        >

          <div class="ai-answer">
            {{ analysis }}
          </div>

        </div>

        <div
          v-else
          class="analysis-placeholder"
        >

          <p>
            点击右上角“重新分析”，
            让AI根据你的答题情况生成学习建议。
          </p>

        </div>

      </div>

    </template>

  </div>
</template>


<script setup>

import {
  ref,
  onMounted
} from "vue";

import {
  getLearningSummary
} from "../services/learningData.js";

import {
  generateLearningAnalysis
} from "../services/learningAnalysis.js";


const summary = ref({
  total: 0,
  correct: 0,
  wrong: 0,
  accuracy: 0,
  knowledgePoints: [],
  weakPoints: [],
  strongPoints: []
});


const analysis = ref("");

const loading = ref(false);


/**
 * 加载统计数据
 */
function loadSummary() {
  summary.value =
    getLearningSummary();
}


/**
 * AI分析
 */
async function handleAnalysis() {

  if (summary.value.total === 0) {

    analysis.value =
      "目前还没有足够的答题数据，请先完成一些题目。";

    return;
  }

  loading.value = true;

  analysis.value = "";

  try {

    const result =
      await generateLearningAnalysis();

    if (result.success) {

      analysis.value =
        result.answer;

    } else {

      analysis.value =
        result.message ||
        "AI分析失败，请稍后重试。";

    }

  } catch (error) {

    console.error(error);

    analysis.value =
      "AI分析失败，请检查AI服务是否正常运行。";

  } finally {

    loading.value = false;

  }

}


/**
 * 根据正确率设置样式
 */
function getAccuracyClass(
  accuracy
) {

  if (accuracy < 60) {
    return "danger";
  }

  if (accuracy < 80) {
    return "warning";
  }

  return "success";

}


onMounted(() => {

  loadSummary();

});

</script>


<style scoped>

.analysis-page {

  min-height: 100vh;

  padding: 40px;

  box-sizing: border-box;

  background:
    linear-gradient(
      135deg,
      #f5f8fc,
      #eef4fb
    );

}


/* 页面标题 */

.page-header {

  display: flex;

  align-items: center;

  justify-content: space-between;

  margin-bottom: 30px;

}


.page-header h1 {

  margin: 0;

  font-size: 32px;

  color: #172b4d;

}


.page-header p {

  margin: 10px 0 0;

  color: #718096;

  font-size: 16px;

}


.refresh-button {

  border: none;

  background: #1677ff;

  color: white;

  padding: 12px 24px;

  border-radius: 10px;

  font-size: 15px;

  cursor: pointer;

}


.refresh-button:hover {

  background: #409eff;

}


.refresh-button:disabled {

  opacity: 0.6;

  cursor: not-allowed;

}


/* 数据卡片 */

.summary-grid {

  display: grid;

  grid-template-columns:
    repeat(4, 1fr);

  gap: 20px;

  margin-bottom: 30px;

}


.summary-card {

  background: white;

  border-radius: 16px;

  padding: 24px;

  display: flex;

  align-items: center;

  gap: 16px;

  box-shadow:
    0 5px 20px
    rgba(0, 0, 0, 0.05);

}


.summary-icon {

  width: 50px;

  height: 50px;

  border-radius: 14px;

  background: #eaf3ff;

  display: flex;

  align-items: center;

  justify-content: center;

  font-size: 24px;

}


.summary-label {

  color: #718096;

  font-size: 14px;

}


.summary-value {

  margin-top: 5px;

  font-size: 28px;

  font-weight: 700;

  color: #172b4d;

}


/* 空状态 */

.empty-card {

  background: white;

  border-radius: 18px;

  padding: 70px;

  text-align: center;

  box-shadow:
    0 5px 20px
    rgba(0, 0, 0, 0.05);

}


.empty-icon {

  font-size: 60px;

}


.empty-card h2 {

  margin-top: 20px;

  color: #172b4d;

}


.empty-card p {

  color: #718096;

}


/* section */

.section {

  background: white;

  border-radius: 18px;

  padding: 28px;

  margin-bottom: 25px;

  box-shadow:
    0 5px 20px
    rgba(0, 0, 0, 0.05);

}


.section-title {

  font-size: 21px;

  font-weight: 700;

  color: #172b4d;

  margin-bottom: 25px;

  display: flex;

  align-items: center;

  gap: 10px;

}


.weak-title {

  color: #d4380d;

}


/* 知识点 */

.knowledge-list {

  display: flex;

  flex-direction: column;

  gap: 22px;

}


.knowledge-top {

  display: flex;

  justify-content: space-between;

  align-items: center;

}


.knowledge-name {

  font-size: 16px;

  font-weight: 600;

  color: #333;

}


.accuracy {

  font-weight: 700;

}


.progress {

  height: 10px;

  background: #edf1f5;

  border-radius: 10px;

  overflow: hidden;

  margin-top: 10px;

}


.progress-bar {

  height: 100%;

  border-radius: 10px;

}


.knowledge-detail {

  margin-top: 7px;

  color: #9aa5b1;

  font-size: 13px;

}


/* 状态 */

.danger {

  color: #ff4d4f;

  background-color: #ff4d4f;

}


.warning {

  color: #faad14;

  background-color: #faad14;

}


.success {

  color: #52c41a;

  background-color: #52c41a;

}


/* 薄弱知识点 */

.weak-list {

  display: flex;

  flex-direction: column;

  gap: 14px;

}


.weak-item {

  display: flex;

  align-items: center;

  padding: 18px;

  border-radius: 12px;

  background: #fff5f5;

}


.rank {

  width: 36px;

  height: 36px;

  border-radius: 50%;

  background: #ff4d4f;

  color: white;

  display: flex;

  align-items: center;

  justify-content: center;

  font-weight: 700;

  margin-right: 15px;

}


.weak-name {

  font-size: 16px;

  font-weight: 700;

}


.weak-info {

  margin-top: 6px;

  color: #8c8c8c;

  font-size: 13px;

}


/* 成功 */

.success-box {

  padding: 18px;

  background: #f6ffed;

  border-radius: 10px;

  color: #389e0d;

}


/* AI */

.ai-analysis {

  background:
    linear-gradient(
      135deg,
      #ffffff,
      #f5f9ff
    );

}


.ai-answer {

  white-space: pre-wrap;

  line-height: 2;

  font-size: 16px;

  color: #333;

}


.analysis-placeholder {

  padding: 30px;

  text-align: center;

  color: #8c8c8c;

}


.loading {

  text-align: center;

  padding: 40px;

}


.loading-icon {

  font-size: 45px;

  animation:
    loading 1.2s infinite;

}


@keyframes loading {

  0% {
    transform: translateY(0);
  }

  50% {
    transform: translateY(-10px);
  }

  100% {
    transform: translateY(0);
  }

}


/* 响应式 */

@media (max-width: 900px) {

  .summary-grid {

    grid-template-columns:
      repeat(2, 1fr);

  }

}


@media (max-width: 600px) {

  .analysis-page {

    padding: 20px;

  }

  .page-header {

    align-items: flex-start;

    gap: 15px;

    flex-direction: column;

  }

  .summary-grid {

    grid-template-columns: 1fr;

  }

}

</style>