using Interview.Domain.RoomQuestions.Records;

namespace Interview.Domain.Rooms.Service.Records.Response.Detail;

public class RoomDetail
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? TwitchChannel { get; set; }

    public List<RoomUserDetail>? Users { get; set; }

    public required EVRoomStatus RoomStatus { get; init; }
}
