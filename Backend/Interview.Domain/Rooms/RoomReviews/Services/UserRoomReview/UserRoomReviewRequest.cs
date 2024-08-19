namespace Interview.Domain.Rooms.RoomReviews.Services.UserRoomReview;

public class UserRoomReviewRequest
{
    public required Guid UserId { get; init; }

    public required Guid RoomId { get; init; }
}
