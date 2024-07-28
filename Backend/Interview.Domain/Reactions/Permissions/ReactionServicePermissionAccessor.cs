using Interview.Domain.Permissions;
using Interview.Domain.Reactions.Records;
using Interview.Domain.Reactions.Services;
using X.PagedList;

namespace Interview.Domain.Reactions.Permissions;

public class ReactionServicePermissionAccessor : IReactionService, IServiceDecorator
{
    private readonly IReactionService _reactionService;
    private readonly ISecurityService _securityService;

    public ReactionServicePermissionAccessor(IReactionService reactionService, ISecurityService securityService)
    {
        _reactionService = reactionService;
        _securityService = securityService;
    }

    public async Task<IPagedList<ReactionDetail>> FindPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        await _securityService.EnsurePermissionAsync(SEPermission.ReactionFindPage, cancellationToken);
        return await _reactionService.FindPageAsync(pageNumber, pageSize, cancellationToken);
    }
}
