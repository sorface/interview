using Interview.Domain.Repository;
using Interview.Domain.Rooms.Service.Records.Response.Detail;

namespace Interview.Domain.RoomInvites;

public interface IRoomInviteRepository : IRepository<RoomInvite>
{
    Task<RoomInviteDetail> ApplyInvite(Guid inviteId, Guid userId, CancellationToken cancellationToken = default);
}
