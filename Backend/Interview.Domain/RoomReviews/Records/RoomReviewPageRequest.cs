using System.ComponentModel.DataAnnotations;

namespace Interview.Domain.RoomReviews.Records;

public class RoomReviewPageRequest
{
    [Required]
    public required PageRequest Page { get; init; } = new PageRequest();

    [Required]
    public required RoomReviewPageRequestFilter Filter { get; init; } = new RoomReviewPageRequestFilter
    {
        RoomId = null,
        State = null,
    };
}

public class RoomReviewPageRequestFilter
{
    public required Guid? RoomId { get; init; }

    public required EVRoomReviewState? State { get; init; }
}
