using Interview.Domain.Users;
using Entity = Interview.Domain.Repository.Entity;

namespace Interview.Domain.Rooms.RoomParticipants;

public class RoomParticipant : Entity
{
    public RoomParticipant(User user, Room room, SERoomParticipantType type)
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

    public SERoomParticipantType Type { get; set; } = null!;

    public List<AvailableRoomPermission> Permissions { get; set; } = new();
}
