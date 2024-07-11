namespace Interview.Domain.Rooms.Records.Request;

public class RoomQuestionRequest
{
    public required Guid Id { get; init; }

    public required int Order { get; init; }
}
