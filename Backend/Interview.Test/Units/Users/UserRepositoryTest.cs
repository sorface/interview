using Interview.Domain.Users;
using Interview.Infrastructure.Database;
using Interview.Infrastructure.Users;

namespace Interview.Test.Units.Users;

public class UserRepositoryTest : AbstractRepositoryTest<User, UserRepository>
{

    protected override UserRepository GetRepository(AppDbContext databaseSet)
    {
        return new UserRepository(databaseSet);
    }

    protected override User GetInstance()
    {
        return new User("NICKNAME", "1");
    }

}
