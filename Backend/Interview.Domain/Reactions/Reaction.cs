using Interview.Domain.Repository;

namespace Interview.Domain.Reactions;

public class Reaction : Entity
{
    public ReactionType Type { get; set; } = ReactionType.Unknown;
}
