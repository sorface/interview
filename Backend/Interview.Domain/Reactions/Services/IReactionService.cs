using Interview.Domain.Reactions.Records;
using X.PagedList;

namespace Interview.Domain.Reactions.Services;

public interface IReactionService : IService
{
    Task<IPagedList<ReactionDetail>> FindPageAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
