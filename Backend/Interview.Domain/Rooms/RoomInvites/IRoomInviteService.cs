using Interview.Domain.Repository;
using Interview.Domain.Rooms.Records.Response.Detail;

namespace Interview.Domain.Rooms.RoomInvites;

public interface IRoomInviteService : IService
{
    Task<RoomInviteDetail> ApplyInvite(Guid inviteId, Guid userId, CancellationToken cancellationToken = default);
}
