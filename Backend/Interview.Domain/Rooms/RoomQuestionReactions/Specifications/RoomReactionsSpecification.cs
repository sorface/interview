using NSpecifications;

namespace Interview.Domain.Rooms.RoomQuestionReactions.Specifications;

public class RoomReactionsSpecification : Spec<RoomQuestionReaction>
{
    public RoomReactionsSpecification(Guid roomId)
        : base(reaction => reaction.RoomQuestion!.Room!.Id == roomId)
    {
    }
}
