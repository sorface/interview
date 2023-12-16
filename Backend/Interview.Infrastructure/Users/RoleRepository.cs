using Interview.Domain.Users.Roles;
using Interview.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.Users;

public class RoleRepository : EfRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext db)
        : base(db)
    {
    }

    protected override IQueryable<Role> ApplyIncludes(DbSet<Role> set) => set;
}
