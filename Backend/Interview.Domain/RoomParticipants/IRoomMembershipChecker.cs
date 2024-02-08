namespace Interview.Domain.RoomParticipants
{
    public interface IRoomMembershipChecker
    {
        Task EnsureCurrentUserMemberOfRoomAsync(Guid roomId, CancellationToken cancellationToken);
    }
}
