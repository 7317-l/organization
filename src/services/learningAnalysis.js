const calculateRate = (correct, total) => {
  if (!total || total <= 0) {
    return 0
  }

  return Number(
    ((correct / total) * 100).toFixed(2)
  )
}

export const analyzeQuestion = ({
  question,
  options = [],
  userAnswer,
  correctAnswer,
  knowledgePoint,
  knowledgePointId
}) => {
  const isCorrect =
    userAnswer === correctAnswer

  return {
    question,
    options,
    userAnswer,
    correctAnswer,
    knowledgePoint,
    knowledgePointId,
    isCorrect,
    needHelp: !isCorrect
  }
}

export const analyzeLearningData = ({
  totalQuestions = 0,
  correctQuestions = 0,
  knowledgePoints = [],
  wrongQuestions = []
}) => {
  const wrongQuestionsCount =
    Math.max(
      totalQuestions -
        correctQuestions,
      0
    )

  const overallAccuracy =
    calculateRate(
      correctQuestions,
      totalQuestions
    )

  const knowledgeAnalysis =
    knowledgePoints
      .map(item => {
        const total =
          Number(item.total) || 0

        const correct =
          Number(item.correct) || 0

        const wrong =
          Number(item.wrong) ||
          Math.max(
            total - correct,
            0
          )

        const accuracy =
          calculateRate(
            correct,
            total
          )

        return {
          name: item.name,
          total,
          correct,
          wrong,
          accuracy,
          level:
            accuracy >= 85
              ? "优秀"
              : accuracy >= 70
              ? "良好"
              : accuracy >= 60
              ? "一般"
              : "薄弱"
        }
      })
      .sort(
        (a, b) =>
          a.accuracy -
          b.accuracy
      )

  const weakKnowledgePoints =
    knowledgeAnalysis
      .filter(
        item =>
          item.accuracy < 70
      )
      .slice(0, 5)

  const repeatedWrongQuestions =
    [...wrongQuestions]
      .sort(
        (a, b) =>
          (Number(b.wrongCount) || 0) -
          (Number(a.wrongCount) || 0)
      )
      .slice(0, 10)

  return {
    totalQuestions,
    correctQuestions,
    wrongQuestions:
      wrongQuestionsCount,
    overallAccuracy,
    knowledgeAnalysis,
    weakKnowledgePoints,
    repeatedWrongQuestions
  }
}

export const buildLearningPrompt = (
  analysis
) => {
  const weakPoints =
    analysis.weakKnowledgePoints
      .map(
        item =>
          `${item.name}：正确率${item.accuracy}%`
      )
      .join("\n")

  const repeatedWrong =
    analysis.repeatedWrongQuestions
      .map(
        item =>
          `${item.knowledgePoint || "未知知识点"}：错误${item.wrongCount}次`
      )
      .join("\n")

  return `
请根据以下党员学习数据进行学习情况分析。

总答题数：
${analysis.totalQuestions}

正确题数：
${analysis.correctQuestions}

错误题数：
${analysis.wrongQuestions}

综合正确率：
${analysis.overallAccuracy}%

薄弱知识点：
${weakPoints || "暂无"}

高频错误知识点：
${repeatedWrong || "暂无"}

请输出以下内容：

1. 总体学习情况
2. 当前最需要强化的知识点
3. 高频错误原因分析
4. 建议优先学习的内容
5. 下一阶段学习建议

回答应当清晰、具体、具有针对性，不要编造不存在的数据。
`
}