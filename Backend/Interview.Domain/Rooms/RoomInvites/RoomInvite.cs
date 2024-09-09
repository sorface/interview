using Interview.Domain.Invites;
using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Rooms.RoomInvites;

public class RoomInvite : Entity
{
    public RoomInvite(Invite invite, Room room, SERoomParticipantType participantType)
    {
        Invite = invite;
        Room = room;
        ParticipantType = participantType;
    }

    public RoomInvite(Guid invite, Guid room, SERoomParticipantType participantType)
    {
        InviteId = invite;
        RoomId = room;
        ParticipantType = participantType;
    }

    private RoomInvite()
    {
    }

    public Guid InviteId { get; set; }

    public Guid RoomId { get; set; }

    public Invite? Invite { get; set; }

    public Room? Room { get; set; }

    public SERoomParticipantType? ParticipantType { get; set; }
}
