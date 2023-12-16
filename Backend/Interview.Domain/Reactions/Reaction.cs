using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Interview.Domain.Users;

namespace Interview.Domain.Reactions;

public class Reaction : Entity
{
    public ReactionType Type { get; set; } = ReactionType.Unknown;
}
