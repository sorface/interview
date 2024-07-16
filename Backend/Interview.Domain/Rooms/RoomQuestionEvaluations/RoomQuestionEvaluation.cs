using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;

namespace Interview.Domain.Rooms.RoomQuestionEvaluations;

public class RoomQuestionEvaluation : Entity
{
    public required Guid RoomQuestionId { get; set; }

    public RoomQuestion? RoomQuestion { get; set; }

    public string? Review { get; set; }

    public int? Mark { get; set; }

    public SERoomQuestionEvaluationState State { get; set; } = SERoomQuestionEvaluationState.Draft;
}
