using Interview.Domain.Users;
using Entity = Interview.Domain.Repository.Entity;

namespace Interview.Domain.Rooms.RoomParticipants;

public class RoomParticipant : Entity
{
    public RoomParticipant(User user, Room room, RoomParticipantType type)
    {
        User = user;
        Room = room;
        Type = type;
    }

    private RoomParticipant()
    {
    }

    public User User { get; set; } = null!;

    public Room Room { get; set; } = null!;

    public RoomParticipantType Type { get; set; } = null!;
}
