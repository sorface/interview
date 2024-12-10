using Interview.Domain.Database;
using Interview.Domain.Users.Roles;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.Users;

public class RoleRepository(AppDbContext db) : EfRepository<Role>(db), IRoleRepository
{
    protected override IQueryable<Role> ApplyIncludes(DbSet<Role> set) => set;
}
