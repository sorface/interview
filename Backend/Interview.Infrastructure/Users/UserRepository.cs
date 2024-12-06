using Interview.Domain.Database;
using Interview.Domain.Repository;
using Interview.Domain.Users;
using Interview.Domain.Users.Permissions;
using Interview.Domain.Users.Records;
using Interview.Domain.Users.Roles;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Interview.Infrastructure.Users;

public class UserRepository(AppDbContext db) : EfRepository<User>(db), IUserRepository
{
    public Task<User?> FindByNicknameAsync(string nickname, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set)
            .FirstOrDefaultAsync(user => user.Nickname == nickname, cancellationToken);
    }

    public Task<List<User>> GetByRoleAsync(RoleName roleName, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set)
            .Where(e => e.Roles.Any(r => r.Name == roleName))
            .ToListAsync(cancellationToken);
    }

    public Task<IPagedList<User>> FindPageByRoleAsync<TRes>(
        IMapper<User, TRes> mapper,
        int pageNumber,
        int pageSize,
        RoleName roleName,
        CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set)
            .Where(e => e.Roles.Any(r => r.Name == roleName))
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<User?> FindByExternalIdAsync(string twitchIdentity, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set)
            .FirstOrDefaultAsync(user => user.ExternalId == twitchIdentity, cancellationToken);
    }

    public async Task<Dictionary<string, List<PermissionItem>>> FindPermissionByUserId(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await ApplyIncludes(Set)
            .Where(user => user.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        var userPermissions = (user?.Permissions ?? []).Select(it => it.Id).ToHashSet();

        var dictionary = Db.Permissions.ToList()
            .Aggregate(
                new Dictionary<string, List<PermissionItem>>(),
                (dict, item) =>
                {
                    var permissionItem = new PermissionItem(item.Type, userPermissions.Contains(item.Id));

                    if (dict.ContainsKey(item.Type.Name))
                    {
                        dict.GetValueOrDefault(item.Type.Name)?.Add(permissionItem);
                    }
                    else
                    {
                        dict.Add(item.Type.Name, [..new[] { permissionItem }]);
                    }

                    return dict;
                });

        return dictionary;
    }

    protected override IQueryable<User> ApplyIncludes(DbSet<User> set) =>
        set.Include(user => user.Roles)
            .Include(user => user.Permissions);
}
