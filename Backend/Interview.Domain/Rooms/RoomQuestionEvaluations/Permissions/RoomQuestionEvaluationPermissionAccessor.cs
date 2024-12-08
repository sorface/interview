using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Services;

namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Permissions;

public class RoomQuestionEvaluationPermissionAccessor(IRoomQuestionEvaluationService roomQuestionEvaluationService, ISecurityService securityService)
    : IRoomQuestionEvaluationService, IServiceDecorator
{
    public Task<List<RoomQuestionEvaluationResponse>> GetUserRoomQuestionEvaluationsAsync(UserRoomQuestionEvaluationsRequest request, CancellationToken cancellationToken)
    {
        return roomQuestionEvaluationService.GetUserRoomQuestionEvaluationsAsync(request, cancellationToken);
    }

    public async Task<QuestionEvaluationDetail> FindByRoomIdAndQuestionIdAsync(QuestionEvaluationGetRequest request, CancellationToken cancellationToken)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomQuestionEvaluationFind, cancellationToken);

        return await roomQuestionEvaluationService.FindByRoomIdAndQuestionIdAsync(request, cancellationToken);
    }

    public async Task<QuestionEvaluationDetail> MergeAsync(QuestionEvaluationMergeRequest mergeRequest, CancellationToken cancellationToken)
    {
        await securityService.EnsureRoomPermissionAsync(mergeRequest.RoomId, SEPermission.RoomQuestionEvaluationMerge, cancellationToken);

        return await roomQuestionEvaluationService.MergeAsync(mergeRequest, cancellationToken);
    }
}
