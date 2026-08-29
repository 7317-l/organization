const API_BASE_URL = "http://localhost:3000/api"

export async function sendToAI(messages) {
  const response = await fetch(`${API_BASE_URL}/ai/chat`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      messages
    })
  })

  if (!response.ok) {
    throw new Error(`AI请求失败：${response.status}`)
  }

  const result = await response.json()

  if (!result.success) {
    throw new Error(result.message || "AI服务返回失败")
  }

  return result.data.content
}