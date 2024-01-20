using Interview.Domain.Invites;
using Interview.Domain.RoomParticipants;
using Interview.Domain.Rooms;
using Entity = Interview.Domain.Repository.Entity;

namespace Interview.Domain.RoomInvites;

public class RoomInvite : Entity
{
    public Invite Invite { get; set; }

    public Room Room { get; set; }

    public RoomParticipantType ParticipantType { get; set; }
}
