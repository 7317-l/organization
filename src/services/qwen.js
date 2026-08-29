import OpenAI from "openai";

const client = new OpenAI({
  apiKey: process.env.DASHSCOPE_API_KEY,
  baseURL: process.env.QWEN_BASE_URL
});

export async function chatWithQwen(messages) {
  const completion = await client.chat.completions.create({
    model: process.env.QWEN_MODEL || "qwen-plus",
    messages
  });

  return completion.choices[0].message.content;
}