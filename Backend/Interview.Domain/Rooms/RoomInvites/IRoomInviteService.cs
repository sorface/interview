using Interview.Domain.Rooms.Records.Response;
using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Rooms.RoomInvites;

public interface IRoomInviteService : IService
{
    Task<RoomInviteResponse> ApplyInvite(Guid inviteId, Guid userId, CancellationToken cancellationToken = default);

    public Task<RoomInviteResponse> GenerateAsync(
        Guid roomId,
        SERoomParticipantType participantType,
        int inviteMaxCount,
        CancellationToken cancellationToken);
}
