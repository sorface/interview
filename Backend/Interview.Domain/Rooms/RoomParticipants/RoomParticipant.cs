using Interview.Domain.Rooms.RoomReviews;
using Interview.Domain.Users;
using Interview.Domain.Users.Permissions;
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

    public Guid UserId { get; set; }

    public Room Room { get; set; } = null!;

    public Guid RoomId { get; set; }

    public SERoomParticipantType Type { get; set; } = null!;

    public RoomReview? Review { get; set; }

    public List<Permission> Permissions { get; set; } = new();
}
