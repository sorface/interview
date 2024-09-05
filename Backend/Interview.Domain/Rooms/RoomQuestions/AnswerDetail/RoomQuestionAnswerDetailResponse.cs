using Interview.Domain.Rooms.Records.Response.Detail;

namespace Interview.Domain.Rooms.RoomQuestions.AnswerDetail;

public class RoomQuestionAnswerDetailResponse
{
    public required QuestionCodeEditorResponse? CodeEditor { get; set; }

    public required List<Detail> Details { get; set; }

    public class Detail
    {
        public required string? AnswerCodeEditorContent { get; set; }

        public required List<QuestionDetailTranscriptionResponse> Transcription { get; set; }

        public required DateTime StartActiveDate { get; set; }

        public required DateTime EndActiveDate { get; set; }
    }
}
