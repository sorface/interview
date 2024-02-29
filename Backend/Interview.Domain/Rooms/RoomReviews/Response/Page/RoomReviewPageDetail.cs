using Interview.Domain.Rooms.Records.Response.Detail;

namespace Interview.Domain.Rooms.RoomReviews.Response.Page;

public class RoomReviewPageDetail
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public RoomUserDetail? User { get; set; }

    public string? Review { get; set; }

    public required EVRoomReviewState State { get; init; }
}
