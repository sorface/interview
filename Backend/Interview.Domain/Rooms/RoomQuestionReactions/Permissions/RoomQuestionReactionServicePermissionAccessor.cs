using Interview.Domain.Permissions;
using Interview.Domain.Rooms.RoomQuestionReactions.Records;
using Interview.Domain.Rooms.RoomQuestionReactions.Records.Response;
using Interview.Domain.Rooms.RoomQuestionReactions.Services;

namespace Interview.Domain.Rooms.RoomQuestionReactions.Permissions;

public class RoomQuestionReactionServicePermissionAccessor(
    IRoomQuestionReactionService roomQuestionReactionService,
    ISecurityService securityService)
    : IRoomQuestionReactionService, IServiceDecorator
{
    public async Task<RoomQuestionReactionDetail> CreateAsync(
        RoomQuestionReactionCreateRequest request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await securityService.EnsureRoomPermissionAsync(request.RoomId, SEPermission.RoomQuestionReactionCreate, cancellationToken);
        return await roomQuestionReactionService.CreateAsync(request, userId, cancellationToken);
    }
}
