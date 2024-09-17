namespace Interview.Domain.Rooms.RoomQuestions.Services.AnswerDetail;

public class RoomQuestionAnswerDetailRequest
{
    public required Guid RoomId { get; set; }

    public required Guid QuestionId { get; set; }
}
