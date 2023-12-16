using Interview.Domain.Repository;

namespace Interview.Domain.Reactions;

public interface IReactionRepository : IRepository<Reaction>
{
    Task<Reaction?> FindByReactionTypeAsync(ReactionType reactionType, CancellationToken cancellationToken = default);
}
