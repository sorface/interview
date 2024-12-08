using Interview.Domain.Permissions;
using Interview.Domain.Reactions.Records;
using Interview.Domain.Reactions.Services;
using X.PagedList;

namespace Interview.Domain.Reactions.Permissions;

public class ReactionServicePermissionAccessor(IReactionService reactionService, ISecurityService securityService) : IReactionService, IServiceDecorator
{
    public async Task<IPagedList<ReactionDetail>> FindPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await securityService.EnsurePermissionAsync(SEPermission.ReactionFindPage, cancellationToken);
        return await reactionService.FindPageAsync(pageNumber, pageSize, cancellationToken);
    }
}
