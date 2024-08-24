using Interview.Domain.Questions.QuestionAnswers;

namespace Interview.Domain.Rooms.Records.Response.Detail;

public class RoomQuestionDetail
{
    public Guid Id { get; set; }

    public string? Value { get; set; }

    public required int Order { get; set; }

    public required List<QuestionAnswerResponse>? Answers { get; set; }

    public required QuestionCodeEditorResponse? CodeEditor { get; set; }
}
