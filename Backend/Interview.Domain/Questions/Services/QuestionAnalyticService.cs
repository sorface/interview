using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Questions.Services;

public sealed class QuestionAnalyticService(AppDbContext db) : ISelfScopeService
{
    public async Task<QuestionAnalytics> GetAnalyticsAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        var question = await db.Questions
            .AsNoTracking()
            .Include(q => q.RoomQuestions)
                .ThenInclude(rq => rq.Evaluations)
                    .ThenInclude(e => e.CreatedBy)
            .Include(q => q.RoomQuestions)
                .ThenInclude(rq => rq.Room)
                    .ThenInclude(r => r!.Participants)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question == null)
        {
            throw NotFoundException.Create<Question>(questionId);
        }

        var evaluations = new List<QuestionAnalytics.AnalyticsUserEvaluation>();
        var allEvaluationsSubmitted = true;

        foreach (var roomQuestion in question.RoomQuestions)
        {
            var participants = roomQuestion.Room!.Participants
                .ToDictionary(p => p.UserId, p => p.Type);

            foreach (var evaluation in roomQuestion.Evaluations)
            {
                if (evaluation.State == SERoomQuestionEvaluationState.Draft)
                {
                    allEvaluationsSubmitted = false;
                    continue;
                }

                if (evaluation.State == SERoomQuestionEvaluationState.Submitted)
                {
                    evaluations.Add(new QuestionAnalytics.AnalyticsUserEvaluation
                    {
                        UserId = evaluation.CreatedBy!.Id,
                        Nickname = evaluation.CreatedBy.Nickname,
                        Avatar = evaluation.CreatedBy.Avatar,
                        ParticipantType = participants[evaluation.CreatedBy.Id].EnumValue,
                        Mark = evaluation.Mark,
                        Review = evaluation.Review
                    });
                }
            }
        }

        var analytics = new QuestionAnalytics
        {
            Completed = allEvaluationsSubmitted,
            Evaluations = evaluations,
            AverageMark = allEvaluationsSubmitted && evaluations.Any() 
                ? evaluations
                    .Where(e => e.Mark is not null && e.Mark.Value > 0)
                    .Select(e => e.Mark!.Value)
                    .DefaultIfEmpty(0)
                    .Average()
                : null
        };

        return analytics;
    }
} 