namespace Interview.Domain.Rooms.Records.Response.Detail;

public class RoomUserDetail
{
    public required Guid Id { get; set; }

    public required string? Nickname { get; set; }

    public required string? Avatar { get; set; }

    public required string? Type { get; set; }
}
