using Interview.Domain.Reactions;
using Interview.Domain.Repository;

namespace Interview.Domain.RoomQuestionReactions.Mappers
{
    public class ReactionTypeOnlyMapper
    {
        public static readonly Mapper<RoomQuestionReaction, ReactionType> Instance = new(reaction => reaction.Reaction!.Type);
    }
}
