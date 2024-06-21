namespace Interview.Domain.Rooms.Records.Response.Detail;

public class RoomDetail
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public RoomUserDetail Owner { get; set; }

    public List<RoomUserDetail>? Participants { get; set; }

    public required EVRoomStatus Status { get; init; }

    public List<RoomInviteResponse> Invites { get; init; }

    public EVRoomAccessType Type { get; init; }

    public RoomTimerDetail? Timer { get; init; }
}
