namespace Interview.Domain.Rooms.Records.Request;

public sealed class RoomCreateRequest
{
    public string Name { get; set; } = string.Empty;

    public string TwitchChannel { get; set; } = string.Empty;

    public string AccessType { get; set; } = SeRoomAcсessType.Public.Name;

    public HashSet<Guid> Questions { get; set; } = new();

    public HashSet<Guid> Experts { get; set; } = new();

    public HashSet<Guid> Examinees { get; set; } = new();

    public HashSet<Guid> Tags { get; set; } = new();
}
