using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Interview.Backend.Auth;
using Interview.Domain.Database;
using Interview.Domain.Permissions;
using Interview.Domain.Repository;
using Interview.Domain.Users;
using Interview.Domain.Users.Roles;
using Interview.Domain.Users.Service;
using Interview.Infrastructure.RoomParticipants;
using Interview.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Interview.Test.Integrations;

public class UserServiceTest
{
    public static IEnumerable<object[]> UpsertUsersWhenUserNotExistsInDatabaseData
    {
        get
        {
            yield return ["Dima", RoleName.User];
            yield return ["Dima", RoleName.Admin];
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
            CreateCurrentPermissionAccessor(appDbContext),
            new CachedCurrentUserAccessor(new CurrentUserAccessor(), appDbContext),
            new RoomParticipantRepository(appDbContext)
        );

        var userService = new UserService(new UserRepository(appDbContext), new RoleRepository(appDbContext),
            new PermissionRepository(appDbContext), securityService, appDbContext, new MemoryCache(new MemoryCacheOptions()),
            new SemaphoreLockProvider<string>(NullLogger<SemaphoreLockProvider<string>>.Instance));

        var user = new User("Dima", "1");
        var upsertUser = await userService.UpsertByExternalIdAsync(user);

        var expectedUser = new User(entity.Id, user.Nickname, user.ExternalId);
        expectedUser.UpdateCreateDate(user.CreateDate);
        expectedUser.Roles.AddRange(entity.Roles);
        expectedUser.Permissions.AddRange(entity.Permissions);
        var datesForExcluding = CreateDatesExcluder();
        upsertUser.Should().BeEquivalentTo(expectedUser, e => e.Excluding(datesForExcluding));
    }

    [Theory(DisplayName = "'UpsertByTwitchIdentityAsync' when there is no such user in the database")]
    [MemberData(nameof(UpsertUsersWhenUserNotExistsInDatabaseData))]
    public async Task UpsertUsersWhenUserNotExistsInDatabase(string nickname, RoleName expectedRoleName)
    {
        var clock = new TestSystemClock();
        await using var appDbContext = new TestAppDbContextFactory().Create(clock);

        var securityService = new SecurityService(
            CreateCurrentPermissionAccessor(appDbContext),
            new CachedCurrentUserAccessor(new CurrentUserAccessor(), appDbContext),
            new RoomParticipantRepository(appDbContext)
        );
        var userService = new UserService(
            new UserRepository(appDbContext), new RoleRepository(appDbContext),
            new PermissionRepository(appDbContext), securityService, appDbContext,
            new MemoryCache(new MemoryCacheOptions()),
            new SemaphoreLockProvider<string>(NullLogger<SemaphoreLockProvider<string>>.Instance)
        );

        var user = new User(nickname, "1");
        user.Roles.Add(new Role(expectedRoleName));

        var upsertUser = await userService.UpsertByExternalIdAsync(user);

        var savedUser = await appDbContext.Users.SingleAsync(e => e.Id == upsertUser.Id);
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
        var securityService = new SecurityService(
            CreateCurrentPermissionAccessor(appDbContext),
            new CachedCurrentUserAccessor(new CurrentUserAccessor(), appDbContext),
            new RoomParticipantRepository(appDbContext)
        );

        var userService = new UserService(new UserRepository(appDbContext), new RoleRepository(appDbContext),
            new PermissionRepository(appDbContext), securityService, appDbContext, new MemoryCache(new MemoryCacheOptions()),
            new SemaphoreLockProvider<string>(NullLogger<SemaphoreLockProvider<string>>.Instance));

        var user = new User("Dima", "1");

        var error = await Assert.ThrowsAsync<Domain.NotFoundException>(async () => await userService.UpsertByExternalIdAsync(user));

        error.Message.Should().NotBeNull().And.NotBeEmpty();
    }

    private static CurrentPermissionAccessor CreateCurrentPermissionAccessor(AppDbContext appDbContext)
    {
        return new CurrentPermissionAccessor(appDbContext, new MemoryCache(new MemoryCacheOptions()), NullLogger<CurrentPermissionAccessor>.Instance);
    }

    private static Expression<Func<IMemberInfo, bool>> CreateDatesExcluder()
    {
        Expression<Func<IMemberInfo, bool>> excluder = info =>
            info.Name == nameof(Entity.UpdateDate) || info.Name == nameof(Entity.CreateDate);
        return excluder;
    }
}
