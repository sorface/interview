namespace Interview.Domain.Rooms.Records.Request;

public sealed class RoomCreateRequest
{
    public string Name { get; set; } = string.Empty;

    public SERoomAccessType AccessType { get; set; } = SERoomAccessType.Public;

    public required HashSet<Guid> Questions { get; init; }

    public required HashSet<Guid> Experts { get; init; }

    public required HashSet<Guid> Examinees { get; init; }

    public required HashSet<Guid> Tags { get; init; }

    public required Guid? CategoryId { get; init; }
}
