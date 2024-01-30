using Interview.Domain.Questions;

namespace Interview.Domain.RoomQuestions.Records
{
    public class RoomQuestionCreateRequest
    {
        public Guid RoomId { get; set; }

        public Guid? QuestionId { get; set; }

        public QuestionCreateRequest? Question { get; set; }
    }
}
