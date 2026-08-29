// routes/content.js
// AI 素材生成接口（出题 + 学习卡片），供主系统 AiContentGenerationService 转发使用

import express from "express"
import { chatWithQwen } from "../services/qwen.js"

const router = express.Router()

// POST /api/ai/generate-content
// 入参：{ sourceText?, singleChoiceCount?, multiChoiceCount?, trueFalseCount?, generateFlashCards? }
// 返回：
//   { success: true, data: { questions: [...], flashCards: [...], summary } }
router.post("/generate-content", async (req, res) => {
  try {
    const {
      sourceText = "",
      singleChoiceCount = 5,
      multiChoiceCount = 3,
      trueFalseCount = 2,
      generateFlashCards = true
    } = req.body || {}

    const prompt = `
你是数智党校AI出题助手，请根据以下要求生成党建知识题目。

${sourceText
  ? `参考材料（源文本）：
${sourceText}
`
  : "（未提供源文本，请基于党建基础知识规范出题）"}

请生成：
- 单选题 ${singleChoiceCount} 道
- 多选题 ${multiChoiceCount} 道
- 判断题 ${trueFalseCount} 道
${generateFlashCards ? "- 学习卡片 3 张" : ""}

严格按以下 JSON 格式输出（不要输出 JSON 之外的任何内容）：
{
  "questions": [
    { "questionType": "single", "stem": "题目", "options": ["A选项","B选项","C选项","D选项"], "correctAnswer": "B", "score": 10 },
    { "questionType": "multi", "stem": "题目", "options": ["A选项","B选项","C选项","D选项"], "correctAnswer": "[0,1,2]", "score": 15 },
    { "questionType": "trueFalse", "stem": "题目", "options": ["正确","错误"], "correctAnswer": "A", "score": 5 }
  ],
  "flashCards": [ { "front": "正面内容", "back": "背面内容" } ]
}

要求：
1. 题目内容准确、符合党建知识规范，不要编造不存在的法规条款。
2. 每道题选项 2～5 个。
3. 多选题 correctAnswer 用正确选项下标的数组字符串表示，例如 "[0,2]"。
4. 判断题 correctAnswer 用 "A"（正确）或 "B"（错误）。
5. 学习卡片仅当 generateFlashCards 为 true 时生成。
`

    const answer = await chatWithQwen([
      {
        role: "system",
        content:
          "你是数智党校AI出题助手，只输出指定的JSON，不输出其他任何内容。"
      },
      {
        role: "user",
        content: prompt
      }
    ])

    // 解析千问返回的 JSON（容错处理）
    let parsed = null
    const cleaned = answer
      .replace(/```json|```/g, "")
      .trim()

    try {
      parsed = JSON.parse(cleaned)
    } catch (e) {
      const start = cleaned.indexOf("{")
      const end = cleaned.lastIndexOf("}")
      if (start >= 0 && end > start) {
        try {
          parsed = JSON.parse(
            cleaned.slice(start, end + 1)
          )
        } catch (e2) {
          parsed = null
        }
      }
    }

    if (
      !parsed ||
      !Array.isArray(parsed.questions)
    ) {
      return res.status(502).json({
        success: false,
        message: "AI返回内容解析失败，请重试"
      })
    }

    const flashCards =
      Array.isArray(parsed.flashCards)
        ? parsed.flashCards
        : []

    res.json({
      success: true,
      data: {
        questions: parsed.questions,
        flashCards,
        summary:
          `已基于${sourceText ? "源材料" : "党建基础知识"}生成${parsed.questions.length}道题目和${flashCards.length}张学习卡片，建议人工审核后再发布使用。`
      }
    })
  } catch (error) {
    console.error("AI出题失败:", error)

    res.status(500).json({
      success: false,
      message:
        error.message || "AI出题失败"
    })
  }
})

export default router
