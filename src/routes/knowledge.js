// routes/knowledge.js
// 党建知识库问答接口（知识库检索 + 通义千问生成）

import express from "express"
import {
  searchKnowledge,
  buildKnowledgeContext
} from "../services/knowledge.js"
import { chatWithQwen } from "../services/qwen.js"

const router = express.Router()

// POST /api/ai/knowledge-query
// 入参：{ question, sessionId? }
// 返回：
//   { success: true, data: { content, references, sessionId } }
router.post("/knowledge-query", async (req, res) => {
  try {
    const { question, sessionId } = req.body || {}

    if (!question) {
      return res.status(400).json({
        success: false,
        message: "question不能为空"
      })
    }

    // 1. 从知识库检索相关资料
    const results = searchKnowledge(question, 5)
    const context = buildKnowledgeContext(results)

    // 2. 组装 prompt 交给千问
    const prompt = `
你是数智党校AI党建知识助手。

用户提问：
${question}

以下是从党建知识库中检索到的相关资料：
${context || "（知识库中没有检索到相关资料）"}

请遵循以下要求：
1. 优先根据检索到的知识库资料回答，回答要准确、完整。
2. 如果知识库资料充分，请直接给出答案，并简要说明依据。
3. 如果知识库资料不足，请基于党建常识回答，但不要编造具体的法条、政策或文号。
4. 回答使用中文，条理清晰。
`

    const answer = await chatWithQwen([
      {
        role: "system",
        content:
          "你是数智党校AI党建知识助手，回答要准确、严谨、规范。"
      },
      {
        role: "user",
        content: prompt
      }
    ])

    res.json({
      success: true,
      data: {
        content: answer,
        references: results.map(r => ({
          file: r.file,
          id: r.id
        })),
        sessionId: sessionId || ""
      }
    })
  } catch (error) {
    console.error("知识库问答失败:", error)

    res.status(500).json({
      success: false,
      message:
        error.message || "知识库问答失败"
    })
  }
})

export default router
