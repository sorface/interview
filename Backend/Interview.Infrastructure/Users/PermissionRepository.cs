using Interview.Domain.Database;
using Interview.Domain.Users.Permissions;

namespace Interview.Infrastructure.Users;

public class PermissionRepository : EfRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(AppDbContext db)
        : base(db)
    {
    }
}
