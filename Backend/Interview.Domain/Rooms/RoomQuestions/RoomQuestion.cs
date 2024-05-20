using Interview.Domain.Questions;
using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomQuestions;

public class RoomQuestion : Entity
{
    public RoomQuestion()
    {
    }

    public Room? Room { get; set; }

    public Question? Question { get; set; }

    public RoomQuestionState? State { get; set; }
}
