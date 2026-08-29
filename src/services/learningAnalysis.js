// frontend/src/services/learningAnalysis.js

import {
  getLearningRecords,
  getLearningSummary
} from "./learningData.js";

/**
 * 请求后端 AI
 */
async function requestAI(messages) {
  const response = await fetch(
    "http://localhost:3000/api/ai/chat",
    {
      method: "POST",

      headers: {
        "Content-Type": "application/json"
      },

      body: JSON.stringify({
        messages
      })
    }
  );

  if (!response.ok) {
    throw new Error(
      `AI服务请求失败：${response.status}`
    );
  }

  const result =
    await response.json();

  if (
    !result.success ||
    !result.data
  ) {
    throw new Error(
      result.message ||
      "AI服务返回异常"
    );
  }

  return result.data.content;
}

/**
 * 生成 AI 学习分析
 */
export async function generateLearningAnalysis() {
  const records =
    getLearningRecords();

  const summary =
    getLearningSummary();

  if (records.length === 0) {
    return {
      success: false,
      message:
        "目前还没有足够的答题数据，请先完成一些题目。"
    };
  }

  const knowledgeData =
    summary.knowledgePoints
      .map((item) => {
        return {
          knowledgePoint:
            item.knowledgePoint,

          total: item.total,

          correct: item.correct,

          wrong: item.wrong,

          accuracy:
            item.accuracy
        };
      });

  const prompt = `
你现在是“数智党校AI学习分析助手”。

请根据党员的历史答题数据，对党员的学习情况进行分析。

【总体数据】

总答题数：
${summary.total}

正确题数：
${summary.correct}

错误题数：
${summary.wrong}

总体正确率：
${summary.accuracy}%

【知识点数据】

${JSON.stringify(
  knowledgeData,
  null,
  2
)}

请完成以下任务：

1. 判断党员目前整体学习情况。

2. 找出最薄弱的3个知识点。

3. 找出掌握较好的知识点。

4. 分析党员可能存在的学习问题。

5. 给出具体的学习建议。

6. 给出下一阶段建议重点学习的知识点。

要求：

- 必须严格根据提供的数据分析。
- 不要编造不存在的答题数据。
- 不要虚构党员没有出现过的知识点。
- 语言简洁、清晰、适合党员学习。
- 不需要输出复杂表格。
- 使用中文。
- 分成“总体情况”“薄弱知识点”“掌握较好”“学习问题”“AI学习建议”五部分。
`;

  try {
    const answer =
      await requestAI([
        {
          role: "system",

          content:
            "你是一个严谨、专业、负责的党员学习分析助手。"
        },

        {
          role: "user",

          content: prompt
        }
      ]);

    return {
      success: true,

      answer,

      summary,

      records
    };
  } catch (error) {
    console.error(
      "AI学习分析失败：",
      error
    );

    return {
      success: false,

      message:
        error.message ||
        "AI学习分析失败"
    };
  }
}

/**
 * 题目答题情况分析（供 /api/ai/question-help 使用）
 */
export function analyzeQuestion({ question, options, userAnswer, correctAnswer, knowledgePoint, knowledgePointId } = {}) {
  const answered = !!userAnswer && String(userAnswer).trim() !== ""
  const isCorrect = answered && correctAnswer
    ? String(userAnswer).trim() === String(correctAnswer).trim()
    : null
  return {
    question: question || "",
    options: Array.isArray(options) ? options : [],
    knowledgePoint: knowledgePoint || knowledgePointId || "未标注知识点",
    answered,
    isCorrect,
    hasCorrectAnswer: !!correctAnswer,
    needsHint: answered && isCorrect === false
  }
}

/**
 * 学习数据统计与分析（供 /api/ai/analyze-learning 使用）
 */
export function analyzeLearningData({ totalQuestions, correctQuestions, knowledgePoints = [], wrongQuestions = [] } = {}) {
  const total = Number(totalQuestions) || 0
  const correct = Number(correctQuestions) || 0
  const wrong = Math.max(0, total - correct)
  const accuracy = total > 0 ? Math.round((correct / total) * 1000) / 10 : 0

  const kpStats = (Array.isArray(knowledgePoints) ? knowledgePoints : []).map(kp => {
    const kpTotal = Number(kp && kp.total) || 0
    const kpCorrect = Number(kp && kp.correct) || 0
    const kpWrong = Number(kp && kp.wrong) || (kpTotal - kpCorrect)
    const kpAccuracy = kpTotal > 0 ? Math.round((kpCorrect / kpTotal) * 1000) / 10 : 0
    return { ...kp, total: kpTotal, correct: kpCorrect, wrong: kpWrong, accuracy: kpAccuracy }
  })

  const weakPoints = (Array.isArray(wrongQuestions) ? wrongQuestions : []).slice(0, 5)

  return { total, correct, wrong, accuracy, knowledgePoints: kpStats, wrongQuestions: weakPoints }
}

/**
 * 生成 AI 学习分析提示词（供 /api/ai/analyze-learning 使用）
 */
export function buildLearningPrompt(analysis) {
  if (!analysis) return ""
  return `
你是"数智党校AI学习分析助手"。

请根据党员的历史答题数据进行学习情况分析。

【总体数据】
总答题数：${analysis.total}
正确题数：${analysis.correct}
错误题数：${analysis.wrong}
总体正确率：${analysis.accuracy}%

【知识点数据】
${JSON.stringify(analysis.knowledgePoints, null, 2)}

【薄弱知识点】
${analysis.wrongQuestions && analysis.wrongQuestions.length ? analysis.wrongQuestions.join("\n") : "暂无"}

请完成以下任务：
1. 判断党员目前整体学习情况。
2. 找出最薄弱的3个知识点。
3. 找出掌握较好的知识点。
4. 分析党员可能存在的学习问题。
5. 给出具体的学习建议。
6. 给出下一阶段建议重点学习的知识点。

要求：
- 必须严格根据提供的数据分析，不要编造数据。
- 语言简洁、清晰，使用中文。
- 分成"总体情况""薄弱知识点""掌握较好""学习问题""AI学习建议"五部分。
`
}
