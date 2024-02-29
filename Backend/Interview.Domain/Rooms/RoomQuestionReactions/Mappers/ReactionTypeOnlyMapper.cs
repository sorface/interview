using Interview.Domain.Reactions;
using Interview.Domain.Repository;

namespace Interview.Domain.Rooms.RoomQuestionReactions.Mappers;

public class ReactionTypeOnlyMapper
{
    public static readonly Mapper<RoomQuestionReaction, ReactionType> Instance = new(reaction => reaction.Reaction!.Type);
}