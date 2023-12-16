namespace Interview.Domain.RoomReviews.Records;

public class RoomReviewUpdateRequest
{
    public string Review { get; init; } = string.Empty;

    public EVRoomReviewState State { get; init; } = EVRoomReviewState.Open;
}
