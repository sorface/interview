using Interview.Domain.Rooms.Records.Response.Detail;
using Interview.Domain.Tags.Records.Response;

namespace Interview.Domain.Rooms.Records.Response.Page;

public class RoomPageDetail
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public List<RoomQuestionDetail>? Questions { get; set; }

    public List<RoomUserDetail>? Users { get; set; }

    public List<TagItem>? Tags { get; set; }

    public required EVRoomStatus RoomStatus { get; init; }

    public RoomTimerDetail? Timer { get; set; }
}
