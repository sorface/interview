using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;

namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Services;

public interface IRoomQuestionEvaluationService : IService
{
    public Task<QuestionEvaluationDetail> FindByRoomIdAndQuestionIdAsync(QuestionEvaluationGetRequest request, CancellationToken cancellationToken);

    public Task<QuestionEvaluationDetail> MergeAsync(QuestionEvaluationMergeRequest mergeRequest, CancellationToken cancellationToken);
}
