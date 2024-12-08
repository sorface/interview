using Interview.Domain.Reactions.Records;
using Interview.Domain.Repository;
using X.PagedList;

namespace Interview.Domain.Reactions.Services;

public class ReactionService(IReactionRepository reactionRepository) : IReactionService
{
    public async Task<IPagedList<ReactionDetail>> FindPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var mapper = new Mapper<Reaction, ReactionDetail>(
            reaction => new ReactionDetail { Id = reaction.Id, Type = reaction.Type, });

        return await reactionRepository.GetPageDetailedAsync(mapper, pageNumber, pageSize, cancellationToken);
    }
}
