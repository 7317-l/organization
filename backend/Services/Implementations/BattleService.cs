using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PartySchoolApi.Data;
using PartySchoolApi.Middleware;
using PartySchoolApi.Models.Common;
using PartySchoolApi.Models.DTOs;
using PartySchoolApi.Models.Entities;
using PartySchoolApi.Services.Interfaces;

namespace PartySchoolApi.Services.Implementations;

public class BattleService : IBattleService
{
    private readonly AppDbContext _db;
    private readonly IPointService _pointService;
    private readonly INotificationService _notificationService;

    public BattleService(AppDbContext db, IPointService pointService, INotificationService notificationService)
    {
        _db = db;
        _pointService = pointService;
        _notificationService = notificationService;
    }

    public async Task<CreateBattleResponse> CreateBattleAsync(int challengerId, CreateBattleRequest request)
    {
        if (challengerId == request.OpponentId)
            throw new BusinessException("不能与自己对战");

        var opponent = await _db.PartyMembers.FindAsync(request.OpponentId);
        if (opponent == null)
            throw new BusinessException("对手不存在");

        var count = Math.Clamp(request.QuestionCount, 3, 10);
        var query = _db.Questions.Where(q => q.QuestionType == QuestionType.SingleChoice || q.QuestionType == QuestionType.TrueFalse);
        if (request.Difficulty.HasValue)
            query = query.Where(q => q.Score == (request.Difficulty.Value == 1 ? 5 : request.Difficulty.Value == 2 ? 10 : 15));

        var allQids = await query.Select(q => q.Id).ToListAsync();
        if (allQids.Count < count)
            throw new BusinessException("题库题目不足，无法创建对战");

        var shuffled = allQids.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        var game = new BattleGame
        {
            ChallengerId = challengerId,
            OpponentId = request.OpponentId,
            Status = 0,
            QuestionIds = JsonSerializer.Serialize(shuffled),
            TimeoutMinutes = 10,
            CreatedAt = DateTime.Now
        };
        _db.BattleGames.Add(game);
        await _db.SaveChangesAsync();

        try
        {
            await _notificationService.SendAsync(new SendNotificationRequest
            {
                PartyMemberId = request.OpponentId,
                Type = NotificationType.SystemNotice,
                Title = "党史PK对战邀请",
                Content = $"您收到了一场党史PK对战邀请，请及时应战！"
            });
        }
        catch { }

        return new CreateBattleResponse
        {
            GameId = game.Id,
            QuestionCount = count,
            ExpiresAt = game.CreatedAt.AddMinutes(10)
        };
    }

    public async Task<List<BattlePendingDto>> GetPendingBattlesAsync(int memberId)
    {
        var games = await _db.BattleGames
            .Where(g => g.OpponentId == memberId && g.Status == 0)
            .OrderByDescending(g => g.CreatedAt)
            .Include(g => g.Challenger)
            .ToListAsync();

        return games.Select(g => new BattlePendingDto
        {
            GameId = g.Id,
            ChallengerId = g.ChallengerId,
            ChallengerName = g.Challenger != null ? g.Challenger.Name : "",
            QuestionCount = JsonSerializer.Deserialize<List<int>>(g.QuestionIds)?.Count ?? 0,
            CreatedAt = g.CreatedAt
        }).ToList();
    }

    public async Task AcceptBattleAsync(int gameId, int memberId)
    {
        var game = await _db.BattleGames.FindAsync(gameId);
        if (game == null) throw new BusinessException("对局不存在");
        if (game.OpponentId != memberId) throw new BusinessException("无权操作此对局");
        if (game.Status != 0) throw new BusinessException("对局状态不允许应战");

        game.Status = 1;
        game.StartedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task CancelBattleAsync(int gameId, int memberId)
    {
        var game = await _db.BattleGames.FindAsync(gameId);
        if (game == null) throw new BusinessException("对局不存在");
        if (game.ChallengerId != memberId) throw new BusinessException("只有创建者可以取消");
        if (game.Status != 0) throw new BusinessException("对局已开始，无法取消");

        game.Status = 3;
        await _db.SaveChangesAsync();
    }

    public async Task<BattleCurrentQuestionResponse> GetCurrentQuestionAsync(int gameId, int memberId)
    {
        var game = await _db.BattleGames.FindAsync(gameId);
        if (game == null) throw new BusinessException("对局不存在");
        if (game.ChallengerId != memberId && game.OpponentId != memberId)
            throw new BusinessException("无权查看此对局");

        var qids = JsonSerializer.Deserialize<List<int>>(game.QuestionIds) ?? new();
        var isChallenger = game.ChallengerId == memberId;
        var opponentName = isChallenger
            ? (await _db.PartyMembers.FindAsync(game.OpponentId))?.Name ?? ""
            : (await _db.PartyMembers.FindAsync(game.ChallengerId))?.Name ?? "";

        // status=0：等待应战，不返回题目
        if (game.Status == 0)
        {
            return new BattleCurrentQuestionResponse
            {
                GameId = gameId,
                Index = 0,
                Total = qids.Count,
                Question = null,
                MyScore = 0,
                OpponentScore = 0,
                OpponentName = opponentName,
                WaitingForOpponent = true
            };
        }

        if (game.Status == 2 || game.CurrentQuestionIndex >= qids.Count)
        {
            return new BattleCurrentQuestionResponse
            {
                GameId = gameId,
                Index = game.CurrentQuestionIndex,
                Total = qids.Count,
                Question = null,
                MyScore = isChallenger ? game.ChallengerScore : game.OpponentScore,
                OpponentScore = isChallenger ? game.OpponentScore : game.ChallengerScore,
                OpponentName = opponentName
            };
        }

        var qid = qids[game.CurrentQuestionIndex];
        var question = await _db.Questions.FindAsync(qid);
        if (question == null)
        {
            return new BattleCurrentQuestionResponse
            {
                GameId = gameId, Index = game.CurrentQuestionIndex, Total = qids.Count, Question = null,
                MyScore = isChallenger ? game.ChallengerScore : game.OpponentScore,
                OpponentScore = isChallenger ? game.OpponentScore : game.ChallengerScore,
                OpponentName = opponentName
            };
        }

        var options = JsonSerializer.Deserialize<List<string>>(question.Options) ?? new();
        return new BattleCurrentQuestionResponse
        {
            GameId = gameId,
            Index = game.CurrentQuestionIndex,
            Total = qids.Count,
            Question = new BattleQuestionDto
            {
                QuestionId = question.Id,
                Stem = question.Stem,
                Options = options,
                Score = question.Score
            },
            MyScore = isChallenger ? game.ChallengerScore : game.OpponentScore,
            OpponentScore = isChallenger ? game.OpponentScore : game.ChallengerScore,
            OpponentName = opponentName
        };
    }

    public async Task<BattleAnswerResponse> SubmitAnswerAsync(int gameId, int memberId, BattleAnswerRequest request)
    {
        var game = await _db.BattleGames.FindAsync(gameId);
        if (game == null) throw new BusinessException("对局不存在");
        // 只有status=1（进行中）才能答题，status=0时等待对方应战
        if (game.Status == 0)
            throw new BusinessException("等待对方应战，暂不能答题");
        if (game.Status != 1)
            throw new BusinessException("对局状态不允许答题");

        var isChallenger = game.ChallengerId == memberId;

        var qids = JsonSerializer.Deserialize<List<int>>(game.QuestionIds) ?? new();
        if (game.CurrentQuestionIndex >= qids.Count)
            throw new BusinessException("所有题目已答完");

        var currentQid = qids[game.CurrentQuestionIndex];
        if (request.QuestionId != currentQid)
            throw new BusinessException("题目不匹配");

        var question = await _db.Questions.FindAsync(request.QuestionId);
        if (question == null) throw new BusinessException("题目不存在");

        var correct = CheckAnswer(question, request.Answer);

        if (correct)
        {
            if (isChallenger) game.ChallengerScore += question.Score;
            else game.OpponentScore += question.Score;
        }

        var nextIndex = game.CurrentQuestionIndex + 1;
        if (nextIndex >= qids.Count)
        {
            game.Status = 2;
            game.FinishedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            await WriteBattleRecordAsync(game);
        }
        else
        {
            game.CurrentQuestionIndex = nextIndex;
            await _db.SaveChangesAsync();
        }

        return new BattleAnswerResponse
        {
            Correct = correct,
            CorrectAnswer = question.CorrectAnswer,
            MyScore = isChallenger ? game.ChallengerScore : game.OpponentScore,
            OpponentScore = isChallenger ? game.OpponentScore : game.ChallengerScore,
            NextIndex = game.Status == 2 ? qids.Count : nextIndex
        };
    }

    public async Task<BattleResultResponse> FinishBattleAsync(int gameId, int memberId)
    {
        var game = await _db.BattleGames.FindAsync(gameId);
        if (game == null) throw new BusinessException("对局不存在");
        if (game.ChallengerId != memberId && game.OpponentId != memberId)
            throw new BusinessException("无权操作此对局");

        if (game.Status != 2)
        {
            game.Status = 2;
            game.FinishedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            await WriteBattleRecordAsync(game);
        }

        return await BuildResultAsync(game, memberId);
    }

    public async Task<BattleResultResponse> GetBattleResultAsync(int gameId, int memberId)
    {
        var game = await _db.BattleGames.FindAsync(gameId);
        if (game == null) throw new BusinessException("对局不存在");
        return await BuildResultAsync(game, memberId);
    }

    public async Task ForfeitBattleAsync(int gameId, int memberId)
    {
        var game = await _db.BattleGames.FindAsync(gameId);
        if (game == null) throw new BusinessException("对局不存在");
        if (game.ChallengerId != memberId && game.OpponentId != memberId)
            throw new BusinessException("无权操作此对局");
        if (game.Status >= 2) return;

        game.Status = 2;
        game.FinishedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        await WriteBattleRecordAsync(game);
    }

    private async Task WriteBattleRecordAsync(BattleGame game)
    {
        var existing = await _db.BattleRecords.AnyAsync(r => r.ChallengerId == game.ChallengerId && r.OpponentId == game.OpponentId && r.BattleTime == game.CreatedAt);
        if (existing) return;

        var qids = JsonSerializer.Deserialize<List<int>>(game.QuestionIds) ?? new();
        var resultJson = JsonSerializer.Serialize(new
        {
            winnerId = game.ChallengerScore > game.OpponentScore ? game.ChallengerId : game.OpponentScore > game.ChallengerScore ? game.OpponentId : (int?)null,
            challengerScore = game.ChallengerScore,
            opponentScore = game.OpponentScore,
            questions = qids
        });

        _db.BattleRecords.Add(new BattleRecord
        {
            ChallengerId = game.ChallengerId,
            OpponentId = game.OpponentId,
            ResultJson = resultJson,
            BattleTime = game.FinishedAt ?? DateTime.Now
        });

        if (game.ChallengerScore > game.OpponentScore)
        {
            await _pointService.AddPointsAsync(game.ChallengerId, 10, PointSourceType.BattleVictory, game.Id);
        }
        else if (game.OpponentScore > game.ChallengerScore)
        {
            await _pointService.AddPointsAsync(game.OpponentId, 10, PointSourceType.BattleVictory, game.Id);
        }
        else
        {
            await _pointService.AddPointsAsync(game.ChallengerId, 3, PointSourceType.BattleVictory, game.Id);
            await _pointService.AddPointsAsync(game.OpponentId, 3, PointSourceType.BattleVictory, game.Id);
        }

        await _db.SaveChangesAsync();
    }

    private async Task<BattleResultResponse> BuildResultAsync(BattleGame game, int memberId)
    {
        var isChallenger = game.ChallengerId == memberId;
        var myScore = isChallenger ? game.ChallengerScore : game.OpponentScore;
        var oppScore = isChallenger ? game.OpponentScore : game.ChallengerScore;
        var isDraw = myScore == oppScore;
        var winnerId = isDraw ? (int?)null : (myScore > oppScore ? memberId : (isChallenger ? game.OpponentId : game.ChallengerId));
        var winnerName = "";
        if (winnerId.HasValue)
        {
            var w = await _db.PartyMembers.FindAsync(winnerId.Value);
            winnerName = w?.Name ?? "";
        }

        return new BattleResultResponse
        {
            WinnerId = winnerId,
            WinnerName = winnerName,
            MyScore = myScore,
            OpponentScore = oppScore,
            IsDraw = isDraw,
            Result = isDraw ? "draw" : (myScore > oppScore ? "win" : "lose")
        };
    }

    private static bool CheckAnswer(Question question, string answer)
    {
        if (string.IsNullOrEmpty(answer)) return false;
        var correct = question.CorrectAnswer.Trim();
        var userAnswer = answer.Trim();

        if (question.QuestionType == QuestionType.TrueFalse)
        {
            var correctBool = correct.ToLower() switch { "true" or "对" or "正确" or "√" or "t" => true, _ => false };
            var userBool = userAnswer.ToLower() switch { "true" or "对" or "正确" or "√" or "t" or "1" => true, _ => false };
            return correctBool == userBool;
        }

        if (correct.Equals(userAnswer, StringComparison.OrdinalIgnoreCase)) return true;
        var options = JsonSerializer.Deserialize<List<string>>(question.Options) ?? new();
        if (int.TryParse(userAnswer, out var idx) && idx >= 0 && idx < options.Count)
            return options[idx].Equals(correct, StringComparison.OrdinalIgnoreCase);
        if (userAnswer.Length == 1 && char.IsLetter(userAnswer[0]))
        {
            var letterIdx = char.ToUpper(userAnswer[0]) - 'A';
            if (letterIdx >= 0 && letterIdx < options.Count)
                return options[letterIdx].Equals(correct, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
