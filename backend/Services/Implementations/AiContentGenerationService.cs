using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

/// <summary>AI素材生成服务（占位实现）</summary>
public class AiContentGenerationService : IAiContentGenerationService
{
    public Task<AiGenerateContentResponse> GenerateAsync(AiGenerateContentRequest request)
    {
        var questions = new List<AiGeneratedQuestionDto>();

        // 生成单选题示例
        for (int i = 0; i < request.SingleChoiceCount; i++)
        {
            questions.Add(new AiGeneratedQuestionDto
            {
                QuestionType = QuestionType.SingleChoice,
                QuestionTypeName = "单选题",
                Stem = $"（AI生成示例）中国共产党成立于哪一年？第{i + 1}题",
                Options = new List<string> { "1919年", "1921年", "1927年", "1949年" },
                CorrectAnswer = "B",
                Score = 10
            });
        }

        // 生成多选题示例
        for (int i = 0; i < request.MultiChoiceCount; i++)
        {
            questions.Add(new AiGeneratedQuestionDto
            {
                QuestionType = QuestionType.MultiChoice,
                QuestionTypeName = "多选题",
                Stem = $"（AI生成示例）以下哪些属于\"四个意识\"？第{i + 1}题",
                Options = new List<string> { "政治意识", "大局意识", "核心意识", "看齐意识" },
                CorrectAnswer = "[0,1,2,3]",
                Score = 15
            });
        }

        // 生成判断题示例
        for (int i = 0; i < request.TrueFalseCount; i++)
        {
            questions.Add(new AiGeneratedQuestionDto
            {
                QuestionType = QuestionType.TrueFalse,
                QuestionTypeName = "判断题",
                Stem = $"（AI生成示例）中国共产党的根本宗旨是全心全意为人民服务。第{i + 1}题",
                Options = new List<string> { "正确", "错误" },
                CorrectAnswer = "A",
                Score = 5
            });
        }

        // 生成学习卡片
        var flashCards = new List<AiFlashCardDto>();
        if (request.GenerateFlashCards)
        {
            flashCards.Add(new AiFlashCardDto
            {
                Front = "中国共产党的初心和使命是什么？",
                Back = "为中国人民谋幸福，为中华民族谋复兴。"
            });
            flashCards.Add(new AiFlashCardDto
            {
                Front = "什么是\"三会一课\"？",
                Back = "支部党员大会、支部委员会、党小组会和党课。"
            });
        }

        return Task.FromResult(new AiGenerateContentResponse
        {
            Questions = questions,
            FlashCards = flashCards,
            Summary = $"已基于源材料生成{questions.Count}道题目和{flashCards.Count}张学习卡片。" +
                      "建议人工审核后再发布使用。"
        });
    }
}
