using Interview.Domain.Users.Permissions;
using Interview.Infrastructure.Database;

namespace Interview.Infrastructure.Users;

public class PermissionRepository : EfRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(AppDbContext db)
        : base(db)
    {
    }
}
