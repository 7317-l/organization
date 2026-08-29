// frontend/src/services/learningData.js

const STORAGE_KEY = "ai_learning_records";

/**
 * 获取当前党员的答题记录
 */
export function getLearningRecords() {
  try {
    const data = localStorage.getItem(STORAGE_KEY);

    if (!data) {
      return [];
    }

    const records = JSON.parse(data);

    if (!Array.isArray(records)) {
      return [];
    }

    return records;
  } catch (error) {
    console.error("读取学习记录失败：", error);
    return [];
  }
}

/**
 * 保存所有答题记录
 */
export function saveLearningRecords(records) {
  try {
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify(records)
    );
  } catch (error) {
    console.error("保存学习记录失败：", error);
  }
}

/**
 * 添加一条答题记录
 */
export function addLearningRecord(record) {
  const records = getLearningRecords();

  const newRecord = {
    id:
      Date.now() +
      "_" +
      Math.random()
        .toString(36)
        .substring(2, 9),

    questionId: record.questionId || "",
    question: record.question || "",
    userAnswer: record.userAnswer || "",
    correctAnswer: record.correctAnswer || "",

    isCorrect:
      Boolean(record.isCorrect),

    knowledgePoint:
      record.knowledgePoint ||
      "未分类知识点",

    timestamp:
      record.timestamp ||
      new Date().toISOString()
  };

  records.push(newRecord);

  saveLearningRecords(records);

  return newRecord;
}

/**
 * 清空学习记录
 */
export function clearLearningRecords() {
  localStorage.removeItem(STORAGE_KEY);
}

/**
 * 获取某一个知识点的数据
 */
export function getKnowledgePointStats() {
  const records = getLearningRecords();

  const map = {};

  records.forEach((record) => {
    const point =
      record.knowledgePoint ||
      "未分类知识点";

    if (!map[point]) {
      map[point] = {
        knowledgePoint: point,
        total: 0,
        correct: 0,
        wrong: 0,
        accuracy: 0
      };
    }

    map[point].total += 1;

    if (record.isCorrect) {
      map[point].correct += 1;
    } else {
      map[point].wrong += 1;
    }
  });

  Object.values(map).forEach((item) => {
    item.accuracy =
      item.total === 0
        ? 0
        : Number(
            (
              (item.correct / item.total) *
              100
            ).toFixed(1)
          );
  });

  return Object.values(map);
}

/**
 * 获取总体学习情况
 */
export function getLearningSummary() {
  const records = getLearningRecords();

  const total = records.length;

  const correct = records.filter(
    (item) => item.isCorrect
  ).length;

  const wrong = total - correct;

  const accuracy =
    total === 0
      ? 0
      : Number(
          ((correct / total) * 100).toFixed(1)
        );

  const knowledgePoints =
    getKnowledgePointStats();

  const weakPoints =
    knowledgePoints
      .filter(
        (item) =>
          item.total >= 1 &&
          item.accuracy < 70
      )
      .sort(
        (a, b) =>
          a.accuracy - b.accuracy
      );

  const strongPoints =
    knowledgePoints
      .filter(
        (item) =>
          item.total >= 1 &&
          item.accuracy >= 80
      )
      .sort(
        (a, b) =>
          b.accuracy - a.accuracy
      );

  return {
    total,
    correct,
    wrong,
    accuracy,
    knowledgePoints,
    weakPoints,
    strongPoints
  };
}