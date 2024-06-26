using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Response;
using Interview.Domain.Rooms.RoomQuestionEvaluations.Services;

namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Permissions;

public class RoomQuestionEvaluationPermissionAccessor : IRoomQuestionEvaluationService, IServiceDecorator
{
    private readonly ISecurityService _securityService;
    private readonly IRoomQuestionEvaluationService _roomQuestionEvaluationService;

    public RoomQuestionEvaluationPermissionAccessor(IRoomQuestionEvaluationService roomQuestionEvaluationService, ISecurityService securityService)
    {
        _securityService = securityService;
        _roomQuestionEvaluationService = roomQuestionEvaluationService;
    }

    public async Task<QuestionEvaluationDetail> FindByRoomIdAndQuestionId(QuestionEvaluationGetRequest request, CancellationToken cancellationToken)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomQuestionEvaluationFind, cancellationToken);

        return await _roomQuestionEvaluationService.FindByRoomIdAndQuestionId(request, cancellationToken);
    }

    public async Task<QuestionEvaluationDetail> MergeAsync(QuestionEvaluationMergeRequest mergeRequest, CancellationToken cancellationToken)
    {
        await _securityService.EnsureRoomPermissionAsync(mergeRequest.RoomId, SEPermission.RoomQuestionEvaluationMerge, cancellationToken);

        return await _roomQuestionEvaluationService.MergeAsync(mergeRequest, cancellationToken);
    }
}
