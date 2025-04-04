using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Tags.Records.Response;

namespace Interview.Domain.Rooms.Records.Response.Page;

public class RoomPageDetail
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public List<RoomUserDetail>? Participants { get; set; }

    public List<TagItem>? Tags { get; set; }

    public required EVRoomStatus Status { get; init; }

    public RoomTimerDetail? Timer { get; set; }

    public DateTime? ScheduledStartTime { get; init; }

    public required EVRoomType Type { get; init; }
}
