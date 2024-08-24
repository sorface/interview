namespace Interview.Domain.Rooms.RoomReviews.Services.UserRoomReview;

public class UserRoomReviewResponse
{
    public Guid Id { get; set; }

    public string? Review { get; set; }

    public required EVRoomReviewState State { get; init; }
}
