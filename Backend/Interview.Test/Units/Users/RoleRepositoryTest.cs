using Interview.Domain.Database;
using Interview.Domain.Users.Roles;
using Interview.Infrastructure.Users;

namespace Interview.Test.Units.Users;

public class RoleRepositoryTest : AbstractRepositoryTest<Role, RoleRepository>
{

    protected override RoleRepository GetRepository(AppDbContext databaseSet)
    {
        return new RoleRepository(databaseSet);
    }

    protected override Role GetInstance()
    {
        return new Role(RoleName.User);
    }

}
