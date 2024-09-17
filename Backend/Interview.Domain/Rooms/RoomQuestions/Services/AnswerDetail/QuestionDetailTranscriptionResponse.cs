namespace Interview.Domain.Rooms.RoomQuestions.Services.AnswerDetail;

public class QuestionDetailTranscriptionResponse
{
    public required Guid Id { get; set; }

    public required string? Payload { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required QuestionDetailTranscriptionUserResponse User { get; set; }
}
