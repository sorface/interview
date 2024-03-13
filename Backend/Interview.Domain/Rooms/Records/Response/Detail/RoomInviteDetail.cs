using Interview.Domain.RoomParticipants;

namespace Interview.Domain.Rooms.Service.Records.Response.Detail;

public class RoomInviteDetail
{
    public Guid ParticipantId { get; set; }

    public Guid RoomId { get; set; }

    public RoomParticipantType ParticipantType { get; set; }
}
