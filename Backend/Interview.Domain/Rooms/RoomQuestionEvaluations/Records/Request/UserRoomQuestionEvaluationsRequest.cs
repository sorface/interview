namespace Interview.Domain.Rooms.RoomQuestionEvaluations.Records.Request;

public class UserRoomQuestionEvaluationsRequest
{
    public required Guid UserId { get; set; }

    public required Guid RoomId { get; set; }
}
