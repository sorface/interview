using Interview.Domain.Database;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomQuestions;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.RoomQuestionEvaluations;

public class RoomQuestionEvaluationRepository(AppDbContext db) : EfRepository<RoomQuestionEvaluation>(db), IRoomQuestionEvaluationRepository
{
    public async Task<RoomQuestionEvaluation?> FindByActiveQuestionRoomAsync(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        var question = await Db.RoomQuestions
            .Include(roomQuestion => roomQuestion.Room)
            .Where(roomQuestion => roomQuestion.State == RoomQuestionState.Active && roomQuestion.RoomId == roomId)
            .FirstAsync(cancellationToken);

        return await ApplyIncludes(Set)
            .Include(e => e.RoomQuestion)
            .Include(e => e.CreatedBy)
            .Select(e => e)
            .Where(e => e.RoomQuestionId == question.Id && e.CreatedById == userId)
            .FirstAsync(cancellationToken);
    }

    public async Task SubmitAsync(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        var roomQuestionEvaluations = ApplyIncludes(Set)
            .Include(evaluation => evaluation.RoomQuestion)
            .Include(evaluation => evaluation.CreatedBy)
            .Where(evaluation => evaluation.RoomQuestion!.RoomId == roomId && evaluation.CreatedById == userId);

        await roomQuestionEvaluations.ForEachAsync(evaluation =>
            {
                evaluation.State = SERoomQuestionEvaluationState.Submitted;
            },
            cancellationToken);

        // todo: not work batch update
        // await roomQuestionEvaluations.ExecuteUpdateAsync(property => property.SetProperty(e => e.State, evaluation => SERoomQuestionEvaluationState.Submitted), cancellationToken);
        await Db.SaveChangesAsync(cancellationToken);
    }
}
