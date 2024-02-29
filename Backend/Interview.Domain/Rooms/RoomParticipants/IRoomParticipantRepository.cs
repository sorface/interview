using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomParticipants;

public interface IRoomParticipantRepository : IRepository<RoomParticipant>
{
    Task<RoomParticipant?> FindByRoomIdAndUserId(Guid roomId, Guid userId, CancellationToken cancellationToken = default);

    public Task<bool> IsExistsByRoomIdAndUserIdAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);
}
