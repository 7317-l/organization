import express from "express";
import cors from "cors";
import aiRouter from "./routes/ai.js";

const app = express();

app.use(cors());
app.use(express.json());

app.get("/api/health", (req, res) => {
  res.json({
    success: true,
    message: "AI backend is running"
  });
});

app.use("/api/ai", aiRouter);

export default app;