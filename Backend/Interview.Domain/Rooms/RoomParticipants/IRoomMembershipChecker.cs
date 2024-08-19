namespace Interview.Domain.Rooms.RoomParticipants;

public interface IRoomMembershipChecker
{
    Task EnsureCurrentUserMemberOfRoomAsync(Guid roomId, CancellationToken cancellationToken);

    Task EnsureUserMemberOfRoomAsync(Guid userId, Guid roomId, CancellationToken cancellationToken);
}
