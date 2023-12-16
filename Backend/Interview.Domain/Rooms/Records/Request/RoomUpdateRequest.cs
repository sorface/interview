namespace Interview.Domain.Rooms.Records.Request;

public class RoomUpdateRequest
{
    public string? Name { get; set; }

    public string? TwitchChannel { get; set; }

    public HashSet<Guid> Tags { get; set; } = new();
}
