using Interview.Domain.Permissions;
using Interview.Domain.RoomQuestionReactions.Records;
using Interview.Domain.RoomQuestionReactions.Records.Response;

namespace Interview.Domain.RoomQuestionReactions.Permissions;

public class RoomQuestionReactionServicePermissionAccessor : IRoomQuestionReactionService, IServiceDecorator
{
    private readonly IRoomQuestionReactionService _roomQuestionReactionService;
    private readonly ISecurityService _securityService;

    public RoomQuestionReactionServicePermissionAccessor(
        IRoomQuestionReactionService roomQuestionReactionService,
        ISecurityService securityService)
    {
        _roomQuestionReactionService = roomQuestionReactionService;
        _securityService = securityService;
    }

    public Task<RoomQuestionReactionDetail> CreateAsync(
        RoomQuestionReactionCreateRequest request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        _securityService.EnsurePermission(SEPermission.RoomQuestionReactionCreate);

        return _roomQuestionReactionService.CreateAsync(request, userId, cancellationToken);
    }
}
