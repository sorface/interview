using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomQuestions.Records;
using Interview.Domain.Rooms.RoomQuestions.Records.Response;
using Interview.Domain.Rooms.RoomQuestions.Services;

namespace Interview.Domain.Rooms.RoomQuestions.Permissions;

public class RoomQuestionServicePermissionAccessor : IRoomQuestionService, IServiceDecorator
{
    private readonly IRoomQuestionService _roomQuestionService;
    private readonly ISecurityService _securityService;

    public RoomQuestionServicePermissionAccessor(
        IRoomQuestionService roomQuestionService,
        ISecurityService securityService)
    {
        _roomQuestionService = roomQuestionService;
        _securityService = securityService;
    }

    public async Task<RoomQuestionDetail> ChangeActiveQuestionAsync(
        RoomQuestionChangeActiveRequest request,
        CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomQuestionChangeActiveQuestion, cancellationToken);
        return await _roomQuestionService.ChangeActiveQuestionAsync(request, cancellationToken);
    }

    public async Task<RoomQuestionDetail> CreateAsync(
        RoomQuestionCreateRequest request,
        CancellationToken cancellationToken)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomQuestionCreate, cancellationToken);
        return await _roomQuestionService.CreateAsync(request, cancellationToken);
    }

    public async Task<List<Guid>> FindGuidsAsync(RoomQuestionsRequest request, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomQuestionFindGuids, cancellationToken);
        return await _roomQuestionService.FindGuidsAsync(request, cancellationToken);
    }
}
