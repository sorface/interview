namespace Interview.Domain.Rooms.Records.Request;

public class RoomUpdateRequest
{
    public string? Name { get; set; }

    public HashSet<Guid> Tags { get; set; } = new();

    public required Guid? CategoryId { get; init; }
}
