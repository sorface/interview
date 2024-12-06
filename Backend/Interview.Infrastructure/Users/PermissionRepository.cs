using Interview.Domain.Database;
using Interview.Domain.Users.Permissions;

namespace Interview.Infrastructure.Users;

public class PermissionRepository(AppDbContext db) : EfRepository<Permission>(db), IPermissionRepository;
