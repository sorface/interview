using Interview.Domain.Reactions;
using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;

namespace Interview.Domain.Rooms.RoomQuestionReactions;

public class RoomQuestionReaction : Entity
{
    public RoomQuestionReaction()
    {
    }

    public Reaction? Reaction { get; set; }

    public RoomQuestion? RoomQuestion { get; set; }

    public User? Sender { get; set; }

    public string? Payload { get; set; }
}
