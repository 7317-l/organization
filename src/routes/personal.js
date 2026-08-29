// routes/personal.js
// 带成员个人上下文的 AI 对话接口（由成员端前端拉取成员数据后传入 userContext）

import express from "express"
import { chatWithQwen } from "../services/qwen.js"

const router = express.Router()

// POST /api/ai/personal-chat
// 入参（二选一）：
//   { message: "...", userContext: {...} }
//   { messages: [{ role, content }, ...], userContext: {...} }
// 返回：
//   { success: true, data: { content: "..." } }
router.post("/personal-chat", async (req, res) => {
  try {
    const { message, messages, userContext } = req.body || {}

    if (!message && !(Array.isArray(messages) && messages.length > 0)) {
      return res.status(400).json({
        success: false,
        message: "message或messages不能为空"
      })
    }

    // 将成员数据作为 system 上下文注入，让 AI 回答时能结合真实数据
    const contextText = userContext
      ? JSON.stringify(userContext)
      : ""

    const systemContent = [
      "你是数智党校AI智能学习助手。",
      "以下是当前党员在平台中的真实个人数据（userContext），回答时应结合这些数据给出个性化、具体的辅导，不要编造数据中不存在的信息：",
      contextText || "（暂无该党员的个人数据）"
    ].join("\n")

    let finalMessages

    if (Array.isArray(messages) && messages.length > 0) {
      finalMessages = [
        {
          role: "system",
          content: systemContent
        },
        ...messages
      ]
    } else {
      finalMessages = [
        {
          role: "system",
          content: systemContent
        },
        {
          role: "user",
          content: message
        }
      ]
    }

    const content = await chatWithQwen(finalMessages)

    res.json({
      success: true,
      data: {
        content
      }
    })
  } catch (error) {
    console.error("个人AI对话失败:", error)

    res.status(500).json({
      success: false,
      message: error.message || "个人AI对话失败"
    })
  }
})

export default router
