using System.Text.Json.Serialization;

namespace Interview.Domain.Rooms.RoomReviews.Records;

public class UpsertReviewResponse
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required Guid RoomId { get; init; }

    public required string? Review { get; init; }

    public required string State { get; init; }

    [JsonIgnore]
    public bool Created { get; init; }
}
