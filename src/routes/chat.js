// routes/chat.js
// 通用 AI 对话接口（统一走通义千问）

import express from "express"
import { chatWithQwen } from "../services/qwen.js"

const router = express.Router()

// POST /api/ai/chat
// 入参（二选一）：
//   { message: "..." } 或 { messages: [{ role, content }, ...] }
// 返回：
//   { success: true, data: { content: "..." } }
router.post("/chat", async (req, res) => {
  try {
    const { message, messages } = req.body || {}

    let finalMessages

    if (Array.isArray(messages) && messages.length > 0) {
      finalMessages = messages
    } else if (message) {
      finalMessages = [
        {
          role: "system",
          content:
            "你是一个专业的学习AI助手。回答要求：1.简洁准确 2.使用中文 3.如果涉及学习问题，优先给出步骤化分析。"
        },
        {
          role: "user",
          content: message
        }
      ]
    } else {
      return res.status(400).json({
        success: false,
        message: "message或messages不能为空"
      })
    }

    const content = await chatWithQwen(finalMessages)

    res.json({
      success: true,
      data: {
        content
      }
    })
  } catch (error) {
    console.error("AI对话失败:", error)

    res.status(500).json({
      success: false,
      message:
        error.message || "AI对话失败"
    })
  }
})

export default router
