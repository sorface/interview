using System.ComponentModel.DataAnnotations;

namespace Interview.Domain.Rooms.RoomReviews.Records;

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

#pragma warning disable SA1402
public class RoomReviewPageRequestFilter
#pragma warning restore SA1402
{
    public required Guid? RoomId { get; init; }

    public required EVRoomReviewState? State { get; init; }
}
