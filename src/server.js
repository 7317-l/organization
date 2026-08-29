import dotenv from "dotenv"

dotenv.config()

const { default: express } = await import("express")
const { default: cors } = await import("cors")
const { default: chatRouter } = await import("./routes/chat.js")
const { default: learningRouter } = await import("./routes/learning.js")
const { default: knowledgeRouter } = await import("./routes/knowledge.js")
const { default: contentRouter } = await import("./routes/content.js")
const { default: personalRouter } = await import("./routes/personal.js")

const app = express()

app.use(
  cors({
    origin: true,
    credentials: true
  })
)

app.use(
  express.json({
    limit: "10mb"
  })
)

app.use(
  express.urlencoded({
    extended: true,
    limit: "10mb"
  })
)

app.get(
  "/",
  (req, res) => {
    res.json({
      success: true,
      message: "AI backend is running"
    })
  }
)

app.use(
  "/api/ai",
  chatRouter
)

app.use(
  "/api/ai",
  learningRouter
)

app.use(
  "/api/ai",
  knowledgeRouter
)

app.use(
  "/api/ai",
  contentRouter
)

app.use(
  "/api/ai",
  personalRouter
)

const PORT =
  process.env.PORT || 3000

app.listen(
  PORT,
  () => {
    console.log(
      `AI backend running at http://localhost:${PORT}`
    )
  }
)