using Interview.Domain.Database;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Interview.Infrastructure.Users;

public sealed class CurrentPermissionAccessor(AppDbContext db, IMemoryCache memoryCache, ILogger<CurrentPermissionAccessor> logger) : ICurrentPermissionAccessor
{
    public bool IsProtectedResource(string resource)
    {
        var permissions = memoryCache.GetOrCreate("ALL_PERMISSIONS", entry =>
        {
            logger.LogInformation("Load permissions from db to cache");
            entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
            return db.Permissions.AsNoTracking()
                .Select(permission => permission.Type.Name)
                .ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        });
        return permissions != null && permissions.Contains(resource);
    }
}
