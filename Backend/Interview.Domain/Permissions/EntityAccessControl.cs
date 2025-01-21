using Interview.Domain.Database;
using Interview.Domain.Repository;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Permissions;

/// <summary>
/// Entity access control.
/// </summary>
public class EntityAccessControl(ICurrentUserAccessor currentUserAccessor, AppDbContext appDbContext) : IService, IEntityAccessControl
{
    public async Task EnsureEditPermissionAsync<T>(Guid entityId, CancellationToken cancellationToken)
        where T : Entity
    {
        if (currentUserAccessor.IsAdmin())
        {
            return;
        }

        var currentUserId = currentUserAccessor.GetUserIdOrThrow();
        var canEdit = await appDbContext.Set<T>()
            .AsNoTracking()
            .AnyAsync(e => e.Id == entityId && (e.CreatedById == null || e.CreatedById == currentUserId), cancellationToken);
        if (!canEdit)
        {
            throw new AccessDeniedException("You do not have permission to modify the entity.");
        }
    }
}
