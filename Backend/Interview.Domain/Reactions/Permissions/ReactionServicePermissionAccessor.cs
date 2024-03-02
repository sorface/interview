using Interview.Domain.Permissions;
using Interview.Domain.Questions.Permissions;
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

    public Task<IPagedList<ReactionDetail>> FindPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        _securityService.EnsurePermission(SEPermission.ReactionFindPage);
        return _reactionService.FindPageAsync(pageNumber, pageSize, cancellationToken);
    }
}
