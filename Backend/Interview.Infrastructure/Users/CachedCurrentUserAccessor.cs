using Interview.Domain.Database;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.Users;

public sealed class CachedCurrentUserAccessor : IEditableCurrentUserAccessor
{
    private readonly IEditableCurrentUserAccessor _root;

    private readonly Lazy<User?> _lazyUser;

    public CachedCurrentUserAccessor(IEditableCurrentUserAccessor root, AppDbContext db)
    {
        _root = root;
        _lazyUser = new Lazy<User?>(() =>
        {
            return db.Users.AsNoTracking()
                .AsSplitQuery()
                .Include(user => user.Roles)
                .Include(user => user.Permissions)
                .FirstOrDefault(user => user.Id == root.UserId);
        });
    }

    public Guid? UserId => _root.UserId;

    public User? UserDetailed => _lazyUser.Value;

    public void SetUser(User user) => _root.SetUser(user);
}
