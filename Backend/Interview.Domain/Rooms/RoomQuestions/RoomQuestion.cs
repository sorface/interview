using Interview.Domain.Questions;
using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomQuestionEvaluations;

namespace Interview.Domain.Rooms.RoomQuestions;

public class RoomQuestion : Entity
{
    public required Guid RoomId { get; set; }

    public required Guid QuestionId { get; set; }

    public required Room? Room { get; set; }

    public required Question? Question { get; set; }

    public required RoomQuestionState State { get; set; }

    public List<RoomQuestionEvaluation> Evaluations { get; set; } = new();

    public required int Order { get; set; }

}
