using Interview.Domain.Permissions;
using Interview.Domain.RoomQuestions.Records;
using Interview.Domain.RoomQuestions.Records.Response;
using Interview.Domain.RoomQuestions.Services;

namespace Interview.Domain.RoomQuestions.Permissions;

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

    public Task<RoomQuestionDetail> ChangeActiveQuestionAsync(
        RoomQuestionChangeActiveRequest request,
        CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.RoomQuestionChangeActiveQuestion);

        return _roomQuestionService.ChangeActiveQuestionAsync(request, cancellationToken);
    }

    public Task<RoomQuestionDetail> CreateAsync(
        RoomQuestionCreateRequest request,
        CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.RoomQuestionCreate);

        return _roomQuestionService.CreateAsync(request, cancellationToken);
    }

    public Task<List<Guid>> FindGuidsAsync(RoomQuestionsRequest request, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.RoomQuestionFindGuids);

        return _roomQuestionService.FindGuidsAsync(request, cancellationToken);
    }
}
