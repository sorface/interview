namespace Interview.Domain.Rooms.RoomQuestions.Services.Update;

/// <summary>
/// RoomQuestionUpdateRequest.
/// </summary>
public class RoomQuestionUpdateRequest
{
    public required Guid QuestionId { get; set; }

    public required int Order { get; set; }
}
