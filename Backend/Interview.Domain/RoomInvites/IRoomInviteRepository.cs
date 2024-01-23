using Interview.Domain.Repository;

namespace Interview.Domain.RoomInvites;

public interface IRoomInviteRepository : IRepository<RoomInvite>
{
    Task<RoomInvite> FindFirstByInviteId(Guid inviteId, CancellationToken cancellationToken = default);
}
