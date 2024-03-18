namespace Interview.Domain.RoomQuestions.Records
{
    public class RoomQuestionsRequest
    {
        public Guid RoomId { get; set; }

        public ISet<RoomQuestionStateType> States { get; set; } = new HashSet<RoomQuestionStateType> { RoomQuestionStateType.Open };
    }
}
