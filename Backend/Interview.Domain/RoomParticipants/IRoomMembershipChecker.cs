namespace Interview.Domain.RoomParticipants
{
    public interface IRoomMembershipChecker
    {
        Task EnsureCurrentUserMemberOfRoom(Guid roomId, CancellationToken cancellationToken);
    }
}
