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
    public async Task EnsureEditPermissionAsync<T>(Guid entityId, bool skipNotExistEntity = false, CancellationToken cancellationToken = default)
        where T : Entity
    {
        if (currentUserAccessor.IsAdmin())
        {
            return;
        }

        var currentUserId = currentUserAccessor.GetUserIdOrThrow();
        var res = await appDbContext.Set<T>()
            .AsNoTracking()
            .Where(e => e.Id == entityId)
            .Select(e => new { e.CreatedById })
            .FirstOrDefaultAsync(cancellationToken);
        if (skipNotExistEntity && res is null)
        {
            return;
        }

        var canEdit = res?.CreatedById != null && res.CreatedById == currentUserId;
        if (!canEdit)
        {
            throw new AccessDeniedException("You do not have permission to modify the entity.");
        }
    }
}
