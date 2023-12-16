using Interview.Domain.Questions;
using Interview.Domain.Repository;
using Interview.Domain.RoomQuestionReactions;
using Interview.Domain.Rooms;

namespace Interview.Domain.RoomQuestions;

public class RoomQuestion : Entity
{
    public RoomQuestion()
    {
    }

    public Room? Room { get; set; }

    public Question? Question { get; set; }

    public RoomQuestionState? State { get; set; }
}
