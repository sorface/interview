using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Interview.Domain.Permissions;
using Interview.Domain.Repository;
using Interview.Domain.Users;
using Interview.Domain.Users.Roles;
using Interview.Domain.Users.Service;
using Interview.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace Interview.Test.Integrations;

public class UserServiceTest
{
    public static IEnumerable<object[]> UpsertUsersWhenUserNotExistsInDatabaseData
    {
        get
        {
            yield return new object[] { "Dima", new AdminUsers(), RoleName.User };

            yield return new object[]
            {
                "Dima", new AdminUsers() { TwitchNicknames = new[] { "Dima" } }, RoleName.Admin
            };
        }
    }

    [Fact(DisplayName = "'UpsertByTwitchIdentityAsync' when there is already such a user in the database")]
    public async Task UpsertUsersWhenUserExistsInDatabase()
    {
        var clock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(clock);
        var entity = new User("Ivan", "1");
        appDbContext.Users.Add(entity);
        await appDbContext.SaveChangesAsync();

        var securityService = new SecurityService(
            new CurrentPermissionAccessor(appDbContext),
            new CachedCurrentUserAccessor(new CurrentUserAccessor(), appDbContext)
        );

        var userService = new UserService(new UserRepository(appDbContext), new RoleRepository(appDbContext),
            new AdminUsers(), new PermissionRepository(appDbContext), securityService);
        var user = new User("Dima", "1");
        var upsertUser = await userService.UpsertByTwitchIdentityAsync(user);

        var expectedUser = new User(entity.Id, user.Nickname, user.TwitchIdentity);
        expectedUser.UpdateCreateDate(user.CreateDate);
        expectedUser.Roles.AddRange(entity.Roles);
        expectedUser.Permissions.AddRange(entity.Permissions);
        var excluder = CreateDatesExcluder();
        upsertUser.Should().BeEquivalentTo(expectedUser, e => e.Excluding(excluder));
    }

    [Theory(DisplayName = "'UpsertByTwitchIdentityAsync' when there is no such user in the database")]
    [MemberData(nameof(UpsertUsersWhenUserNotExistsInDatabaseData))]
    public async Task UpsertUsersWhenUserNotExistsInDatabase(string nickname, AdminUsers adminUsers,
        RoleName expectedRoleName)
    {
        var clock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(clock);
        appDbContext.Users.Count().Should().Be(0);

        var securityService = new SecurityService(
            new CurrentPermissionAccessor(appDbContext),
            new CachedCurrentUserAccessor(new CurrentUserAccessor(), appDbContext)
        );
        var userService =
            new UserService(new UserRepository(appDbContext), new RoleRepository(appDbContext), adminUsers,
                new PermissionRepository(appDbContext), securityService);
        var user = new User(nickname, "1");

        var upsertUser = await userService.UpsertByTwitchIdentityAsync(user);

        var savedUser = await appDbContext.Users.SingleAsync();
        upsertUser.Should().BeEquivalentTo(savedUser);
        upsertUser.Roles.Should().ContainSingle(role => role.Name == expectedRoleName);
    }

    [Fact(DisplayName = "Inserting a user if there are no roles in the database")]
    public async Task UpsertUsersWhenDbNotContainRoles()
    {
        var clock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(clock);
        appDbContext.Roles.RemoveRange(appDbContext.Roles);
        await appDbContext.SaveChangesAsync();
        appDbContext.Users.Count().Should().Be(0);
        var securityService = new SecurityService(
            new CurrentPermissionAccessor(appDbContext),
            new CachedCurrentUserAccessor(new CurrentUserAccessor(), appDbContext)
        );
        var userService = new UserService(new UserRepository(appDbContext), new RoleRepository(appDbContext),
            new AdminUsers(), new PermissionRepository(appDbContext), securityService);
        var user = new User("Dima", "1");

        var error = await Assert.ThrowsAsync<Domain.NotFoundException>(async () =>
            await userService.UpsertByTwitchIdentityAsync(user));

        error.Message.Should().NotBeNull().And.NotBeEmpty();
    }

    private static Expression<Func<IMemberInfo, bool>> CreateDatesExcluder()
    {
        Expression<Func<IMemberInfo, bool>> excluder = info =>
            info.Name == nameof(Entity.UpdateDate) || info.Name == nameof(Entity.CreateDate);
        return excluder;
    }
}
