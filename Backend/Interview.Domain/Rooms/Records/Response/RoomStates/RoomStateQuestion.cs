using Interview.Domain.RoomQuestions;

namespace Interview.Domain.Rooms.Service.Records.Response.RoomStates;

public class RoomStateQuestion
{
    public Guid Id { get; set; }

    public string Value { get; set; } = string.Empty;

    public RoomQuestionState State { get; set; } = RoomQuestionState.Open;
}
