using NSpecifications;

namespace Interview.Domain.Rooms.RoomQuestionReactions.Specifications;

public class RoomReactionsSpecification(Guid roomId) : Spec<RoomQuestionReaction>(reaction => reaction.RoomQuestion!.Room!.Id == roomId);
