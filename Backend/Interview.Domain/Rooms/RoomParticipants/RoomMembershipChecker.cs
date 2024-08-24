using Interview.Domain.Users;

namespace Interview.Domain.Rooms.RoomParticipants;

public class RoomMembershipChecker : IService, IRoomMembershipChecker
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IRoomParticipantRepository _participantRepository;

    public RoomMembershipChecker(ICurrentUserAccessor currentUserAccessor, IRoomParticipantRepository participantRepository)
    {
        _currentUserAccessor = currentUserAccessor;
        _participantRepository = participantRepository;
    }

    public Task EnsureCurrentUserMemberOfRoomAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.GetUserIdOrThrow();
        return EnsureUserMemberOfRoomAsync(userId, roomId, cancellationToken);
    }

    public async Task EnsureUserMemberOfRoomAsync(Guid userId, Guid roomId, CancellationToken cancellationToken)
    {
        var hasParticipant = await _participantRepository.IsExistsByRoomIdAndUserIdAsync(roomId, userId, cancellationToken);
        if (!hasParticipant)
        {
            throw new AccessDeniedException("You are not a member of the room.");
        }
    }
}
