using Interview.Domain.Repository;
using Interview.Domain.Users;

namespace Interview.Domain.Rooms.RoomReviews;

public class RoomReview : Entity
{
    public RoomReview(User user, Room room, SERoomReviewState state)
    {
        User = user;
        Room = room;
        SeRoomReviewState = state;
    }

    private RoomReview()
    {
    }

    public User? User { get; set; }

    public Room? Room { get; set; }

    public string Review { get; set; } = string.Empty;

    public SERoomReviewState SeRoomReviewState { get; set; } = null!;
}
