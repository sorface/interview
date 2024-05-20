namespace Interview.Domain.Rooms.RoomReviews.Records;

public class RoomReviewCreateRequest
{
    public Guid RoomId { get; init; } = Guid.Empty;

    public string Review { get; init; } = string.Empty;
}
