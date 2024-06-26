using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomQuestionEvaluations;

public interface IRoomQuestionEvaluationRepository : IRepository<RoomQuestionEvaluation>
{
    Task<RoomQuestionEvaluation?> FindByActiveQuestionRoomAsync(Guid roomId, Guid userId, CancellationToken cancellationToken);

    Task<RoomQuestionEvaluation?> FindByQuestionIdAndRoomAsync(Guid roomId, Guid questionId, Guid userId, CancellationToken cancellationToken);
}
