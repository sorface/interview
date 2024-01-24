using Interview.Domain.Invites;
using Interview.Domain.Repository;
using Interview.Domain.RoomParticipants;
using Interview.Domain.Rooms;

namespace Interview.Domain.RoomInvites;

public class RoomInvite : Entity
{
    public RoomInvite(Invite invite, Room room, RoomParticipantType participantType)
    {
        Invite = invite;
        Room = room;
        ParticipantType = participantType;
    }

    private RoomInvite()
    {
    }

    public Guid? InviteById { get; set; }

    public Guid? RoomById { get; set; }

    public Invite? Invite { get; set; }

    public Room? Room { get; set; }

    public RoomParticipantType? ParticipantType { get; set; }
}
