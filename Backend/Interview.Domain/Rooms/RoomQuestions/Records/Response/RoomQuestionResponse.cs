using Interview.Domain.Questions.QuestionAnswers;

namespace Interview.Domain.Rooms.RoomQuestions.Records.Response;

public class RoomQuestionResponse
{
    public required Guid Id { get; set; }

    public required string? Value { get; set; }

    public required int Order { get; set; }

    public required RoomQuestionStateType? State { get; set; }

    public required List<QuestionAnswerResponse> Answers { get; set; }
}
