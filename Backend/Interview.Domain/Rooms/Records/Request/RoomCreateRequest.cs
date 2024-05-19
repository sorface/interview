namespace Interview.Domain.Rooms.Records.Request;

public sealed class RoomCreateRequest
{
    public required string Name { get; init; }

    public required string TwitchChannel { get; init; }

    public required SERoomAccessType AccessType { get; init; }

    public required HashSet<Guid> Questions { get; init; }

    public required HashSet<Guid> Experts { get; init; }

    public required HashSet<Guid> Examinees { get; init; }

    public required HashSet<Guid> Tags { get; init; }
}
