using Interview.Domain.Questions.QuestionTreeById;

namespace Interview.Domain.Rooms.Records.Response.Detail;

public class RoomDetail
{
    public required Guid Id { get; set; }

    public required string? Name { get; set; }

    public required RoomUserDetail Owner { get; set; }

    public required List<RoomUserDetail>? Participants { get; set; }

    public required EVRoomStatus Status { get; init; }

    public required List<RoomInviteResponse> Invites { get; init; }

    public required EVRoomAccessType Type { get; init; }

    public required RoomTimerDetail? Timer { get; init; }

    public required DateTime? ScheduledStartTime { get; init; }

    public required List<RoomQuestionDetail>? Questions { get; set; }

    public required QuestionTreeByIdResponse? QuestionTree { get; set; }
}
