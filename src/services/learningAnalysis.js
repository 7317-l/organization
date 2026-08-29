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