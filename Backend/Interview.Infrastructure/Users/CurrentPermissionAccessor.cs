using Interview.Domain.Database;
using Interview.Domain.Users;
using Interview.Domain.Users.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.Users;

public sealed class CurrentPermissionAccessor : ICurrentPermissionAccessor
{
    private readonly Lazy<HashSet<string>> _lazyPermission;

    public CurrentPermissionAccessor(AppDbContext appDbContext)
    {
        // roomQuestionReaction -> RoomQuestionReaction [write, read]
        _lazyPermission = new Lazy<HashSet<string>>(() => appDbContext.Permissions.AsNoTracking()
            .Select(permission => permission.Type.Name)
            .ToHashSet(StringComparer.InvariantCultureIgnoreCase));
    }

    public bool IsProtectedResource(string resource)
    {
        return _lazyPermission.Value.Contains(resource);
    }
}
