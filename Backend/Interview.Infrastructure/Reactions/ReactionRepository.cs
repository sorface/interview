using Interview.Domain.Database;
using Interview.Domain.Reactions;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.Reactions;

public class ReactionRepository : EfRepository<Reaction>, IReactionRepository
{
    public ReactionRepository(AppDbContext db)
        : base(db)
    {
    }

    public Task<Reaction?> FindByReactionTypeAsync(ReactionType reactionType, CancellationToken cancellationToken = default)
    {
        return Set.FirstOrDefaultAsync(e => e.Type == reactionType, cancellationToken);
    }

    protected override IQueryable<Reaction> ApplyIncludes(DbSet<Reaction> set) => Set;
}
