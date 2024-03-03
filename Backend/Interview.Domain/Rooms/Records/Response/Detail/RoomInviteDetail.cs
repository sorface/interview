using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Rooms.Records.Response.Detail;

public class RoomInviteDetail
{
    public Guid ParticipantId { get; set; }

    public Guid RoomId { get; set; }

    public SERoomParticipantType? ParticipantType { get; set; }
}
