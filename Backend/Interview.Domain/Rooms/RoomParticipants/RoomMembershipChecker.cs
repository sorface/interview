using Interview.Domain.Users;

namespace Interview.Domain.Rooms.RoomParticipants;

public class RoomMembershipChecker(ICurrentUserAccessor currentUserAccessor, IRoomParticipantRepository participantRepository)
    : IService, IRoomMembershipChecker
{
    public Task EnsureCurrentUserMemberOfRoomAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetUserIdOrThrow();
        return EnsureUserMemberOfRoomAsync(userId, roomId, cancellationToken);
    }

    public async Task EnsureUserMemberOfRoomAsync(Guid userId, Guid roomId, CancellationToken cancellationToken)
    {
        var hasParticipant = await participantRepository.IsExistsByRoomIdAndUserIdAsync(roomId, userId, cancellationToken);
        if (!hasParticipant)
        {
            throw new AccessDeniedException("You are not a member of the room.");
        }
    }
}
