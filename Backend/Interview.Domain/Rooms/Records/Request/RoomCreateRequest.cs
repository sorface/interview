namespace Interview.Domain.Rooms.Records.Request;

public sealed class RoomCreateRequest
{
    public string Name { get; set; } = string.Empty;

    public SERoomAccessType AccessType { get; set; } = SERoomAcсessType.Public;

    public required HashSet<Guid> Questions { get; init; }

    public required HashSet<Guid> Experts { get; init; }

    public required HashSet<Guid> Examinees { get; init; }

    public required HashSet<Guid> Tags { get; init; }
}
