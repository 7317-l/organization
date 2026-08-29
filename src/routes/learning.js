import express from "express"
import {
  analyzeQuestion,
  analyzeLearningData,
  buildLearningPrompt
} from "../services/learningAnalysis.js"
import { chatWithQwen } from "../services/qwen.js"

const router = express.Router()

router.post(
  "/question-help",
  async (req, res) => {
    try {
      const {
        question,
        options,
        userAnswer,
        correctAnswer,
        knowledgePoint,
        knowledgePointId,
        knowledgeContext
      } = req.body

      if (!question) {
        return res.status(400).json({
          success: false,
          message:
            "question不能为空"
        })
      }

      const analysis =
        analyzeQuestion({
          question,
          options,
          userAnswer,
          correctAnswer,
          knowledgePoint,
          knowledgePointId
        })

      const prompt = `
你是数智党校AI智能学习助手。

现在有一名党员正在进行在线学习。

请根据当前题目和知识点，为党员提供针对性的学习辅导。

当前题目：
${question}

选项：
${(options || []).join("\n")}

党员选择：
${userAnswer || "未提供"}

正确答案：
${correctAnswer || "未提供"}

对应知识点：
${knowledgePoint || "未提供"}

知识库参考内容：
${knowledgeContext || "暂无"}

请遵循以下要求：

1. 如果党员已经答错，请指出其选择与正确答案的区别。
2. 重点解释本题考查的知识点。
3. 用容易理解的语言进行讲解。
4. 给出本题的核心记忆点。
5. 给出一个容易出现的错误理解。
6. 如果知识库没有足够资料，不要编造法规条文。
7. 不要扩展与当前题目无关的大量内容。
8. 如果党员还没有提交答案，不要直接透露正确答案。
9. 最终目的是帮助党员理解知识点，而不是单纯告诉答案。

请直接给出学习辅导内容。
`

      const answer =
        await chatWithQwen([
          {
            role: "system",
            content:
              "你是数智党校AI智能学习助手。"
          },
          {
            role: "user",
            content: prompt
          }
        ])

      res.json({
        success: true,
        data: {
          analysis,
          content: answer
        }
      })
    } catch (error) {
      console.error(error)

      res.status(500).json({
        success: false,
        message:
          error.message ||
          "AI题目辅导失败"
      })
    }
  }
)

router.post(
  "/analyze-learning",
  async (req, res) => {
    try {
      const {
        totalQuestions,
        correctQuestions,
        knowledgePoints,
        wrongQuestions
      } = req.body

      const analysis =
        analyzeLearningData({
          totalQuestions,
          correctQuestions,
          knowledgePoints,
          wrongQuestions
        })

      const prompt =
        buildLearningPrompt(
          analysis
        )

      const answer =
        await chatWithQwen([
          {
            role: "system",
            content:
              "你是数智党校AI学习数据分析助手。"
          },
          {
            role: "user",
            content: prompt
          }
        ])

      res.json({
        success: true,
        data: {
          analysis,
          content: answer
        }
      })
    } catch (error) {
      console.error(error)

      res.status(500).json({
        success: false,
        message:
          error.message ||
          "AI学习分析失败"
      })
    }
  }
)

export default router