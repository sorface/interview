using Interview.Domain.Questions;
using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomQuestions;

public class RoomQuestion : Entity
{
    public RoomQuestion()
    {
    }

    public required Guid RoomId { get; set; }

    public required Guid QuestionId { get; set; }

    public required Room? Room { get; set; }

    public required Question? Question { get; set; }

    public required RoomQuestionState State { get; set; }
}
