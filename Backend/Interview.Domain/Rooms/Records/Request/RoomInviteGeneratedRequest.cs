using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Rooms.Records.Request;

public sealed class RoomInviteGeneratedRequest
{
    public Guid RoomId { get; init; }

    public EVRoomParticipantType ParticipantType { get; init; }
}
