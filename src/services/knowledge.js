import fs from "fs"
import path from "path"
import { fileURLToPath } from "url"

const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

const knowledgePath = path.join(
  __dirname,
  "../../knowledge/documents"
)

const normalizeText = text => {
  return text
    .replace(/\r\n/g, "\n")
    .replace(/\r/g, "\n")
    .replace(/[ \t]+/g, " ")
    .trim()
}

const splitText = text => {
  const normalized = normalizeText(text)

  if (!normalized) {
    return []
  }

  const paragraphs = normalized
    .split(/\n{2,}/)
    .map(item => item.trim())
    .filter(Boolean)

  const chunks = []

  for (const paragraph of paragraphs) {
    if (paragraph.length <= 1000) {
      chunks.push(paragraph)
      continue
    }

    for (
      let start = 0;
      start < paragraph.length;
      start += 800
    ) {
      const chunk = paragraph.slice(
        start,
        start + 1000
      )

      if (chunk.trim()) {
        chunks.push(chunk.trim())
      }
    }
  }

  return chunks
}

const calculateScore = (
  query,
  content
) => {
  const q = query.toLowerCase()
  const target = content.toLowerCase()

  // 1. 整词匹配（按标点切分后的词）
  const queryWords =
    q.split(/[\s，。！？、；：,.!?;:]+/)
      .filter(word => word.length > 1)

  let score =
    queryWords
      .filter(word => target.includes(word))
      .length * 2

  // 2. 连续4字子串匹配（弥补中文整句切分过粗的问题）
  const n = 4

  for (
    let i = 0;
    i <= q.length - n;
    i++
  ) {
    const sub = q.slice(i, i + n)

    if (target.includes(sub)) {
      score += 1
    }
  }

  return score
}

export const loadKnowledgeBase = () => {
  if (!fs.existsSync(knowledgePath)) {
    return []
  }

  const files =
    fs.readdirSync(knowledgePath)

  const documents = []

  for (const file of files) {
    const filePath =
      path.join(
        knowledgePath,
        file
      )

    if (!fs.statSync(filePath).isFile()) {
      continue
    }

    if (
      !file.endsWith(".txt") &&
      !file.endsWith(".md")
    ) {
      continue
    }

    const content =
      fs.readFileSync(
        filePath,
        "utf-8"
      )

    const chunks =
      splitText(content)

    chunks.forEach(
      (chunk, index) => {
        documents.push({
          id:
            `${file}-${index}`,

          file,

          content: chunk
        })
      }
    )
  }

  return documents
}

export const searchKnowledge = (
  query,
  limit = 5
) => {
  const documents =
    loadKnowledgeBase()

  if (
    !query ||
    !documents.length
  ) {
    return []
  }

  return documents
    .map(document => ({
      ...document,
      score: calculateScore(
        query,
        document.content
      )
    }))
    .filter(
      document =>
        document.score > 0
    )
    .sort(
      (a, b) =>
        b.score - a.score
    )
    .slice(0, limit)
}

export const buildKnowledgeContext =
  results => {
    if (!results.length) {
      return ""
    }

    return results
      .map(
        (item, index) =>
          `资料${index + 1}
来源：${item.file}
内容：
${item.content}`
      )
      .join("\n\n")
  }