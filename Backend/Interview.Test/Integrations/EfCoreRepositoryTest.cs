using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Interview.Domain.Repository;
using Interview.Domain.Users;
using Interview.Domain.Users.Roles;
using Interview.Infrastructure.Database;
using Interview.Infrastructure.Users;
using X.PagedList;

namespace Interview.Test.Integrations;

public class EfCoreRepositoryTest
{
    private static Guid TestUserId => Guid.Parse("5ef1f78a-031b-4e0c-9839-4148e27713a1");
    private static Guid TestUserId2 => Guid.Parse("6ef1f78a-031b-4e0c-9839-4148e27713a1");

    [Fact(DisplayName = "FindByIdAsync should not include roles")]
    public async Task DbContext_FindByIdAsync()
    {
        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var expectedUser = CreateUserWithoutRoles(TestUserId);

        var dbUser = await repository.FindByIdAsync(TestUserId);

        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser);
    }

    [Fact(DisplayName = "FindByIdAsync with mapper should not include roles")]
    public async Task DbContext_FindByIdAsync_With_Mapper()
    {
        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var mapper = CreateShortUserWhithoutRolesMapper();
        var expectedUser = mapper.Map(CreateUserWithoutRoles(TestUserId));

        var dbUser = await repository.FindByIdAsync(TestUserId, mapper);

        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser);
    }

    [Fact(DisplayName = "FindByIdDetailedAsync should include roles")]
    public async Task DbContext_FindByIdDetailedAsync()
    {
        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var expectedUser = CreateUserWithRoles(TestUserId);

        var dbUser = await repository.FindByIdDetailedAsync(TestUserId);
        var excluder = CreateDatesExcluder();
        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser, e => e.Excluding(excluder));
    }

    [Fact(DisplayName = "FindByIdDetailedAsync with mapper should include roles")]
    public async Task DbContext_FindByIdDetailedAsync_With_Mapper()
    {
        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var mapper = CreateShortUserWithRolesMapper();
        var expectedUser = mapper.Map(CreateUserWithRoles(TestUserId));

        var dbUser = await repository.FindByIdDetailedAsync(TestUserId, mapper);

        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser);
    }

    [Fact(DisplayName = "FindByIdsAsync should not include roles")]
    public async Task DbContext_FindByIdsAsync()
    {
        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var expectedUser = new[] { CreateUserWithoutRoles(TestUserId), CreateUserWithoutRoles(TestUserId2) };

        var dbUser = await repository.FindByIdsAsync(new List<Guid> { TestUserId, TestUserId2 });

        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser);
    }

    [Fact(DisplayName = "FindByIdsAsync with mapper should not include roles")]
    public async Task DbContext_FindByIdsAsync_With_Mapper()
    {
        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var mapper = CreateShortUserWhithoutRolesMapper();
        var expectedUser = new[]
        {
            mapper.Map(CreateUserWithoutRoles(TestUserId)),
            mapper.Map(CreateUserWithoutRoles(TestUserId2))
        };

        var dbUser = await repository.FindByIdsAsync(new List<Guid> { TestUserId, TestUserId2 }, mapper);

        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser);
    }

    [Fact(DisplayName = "FindByIdsDetailedAsync should include roles")]
    public async Task DbContext_FindByIdsDetailedAsync()
    {
        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var expectedUser = new[] { CreateUserWithRoles(TestUserId), CreateUserWithRoles(TestUserId2) };

        var dbUser = await repository.FindByIdsDetailedAsync(new List<Guid> { TestUserId, TestUserId2 });

        var excluder = CreateDatesExcluder();
        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser, e => e.Excluding(excluder));
    }

    private static Expression<Func<IMemberInfo, bool>> CreateDatesExcluder()
    {
        Expression<Func<IMemberInfo, bool>> excluder = info =>
            info.Name == nameof(Entity.UpdateDate) || info.Name == nameof(Entity.CreateDate);
        return excluder;
    }

    [Fact(DisplayName = "FindByIdsDetailedAsync with mapper should include roles")]
    public async Task DbContext_FindByIdsDetailedAsync_With_Mapper()
    {
        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var mapper = CreateShortUserWithRolesMapper();
        var expectedUser = new[]
        {
            mapper.Map(CreateUserWithRoles(TestUserId)),
            mapper.Map(CreateUserWithRoles(TestUserId2))
        };

        var dbUser = await repository
            .FindByIdsDetailedAsync(new List<Guid> { TestUserId, TestUserId2 }, mapper);

        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser);
    }

    [Fact(DisplayName = "FindPageAsync should not include roles")]
    public async Task DbContext_GetPageAsync()
    {
        const int pageNumber = 1;
        const int pageSize = 2;

        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var expectedUser = new StaticPagedList<User>(new[]
        {
            CreateUserWithoutRoles(TestUserId),
            CreateUserWithoutRoles(TestUserId2),
        }, pageNumber, pageSize, pageSize);

        var dbUser = await repository.GetPageAsync(pageNumber, pageSize);

        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser);
    }

    [Fact(DisplayName = "FindPageAsync with mapper should not include roles")]
    public async Task DbContext_GetPageAsync_With_Mapper()
    {
        const int pageNumber = 1;
        const int pageSize = 2;

        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var mapper = CreateShortUserWhithoutRolesMapper();
        var expectedUser = new StaticPagedList<ShortUserWithoutRoles>(new[]
        {
            mapper.Map(CreateUserWithoutRoles(TestUserId)),
            mapper.Map(CreateUserWithoutRoles(TestUserId2)),
        }, pageNumber, pageSize, pageSize);

        var dbUser = await repository.GetPageAsync(mapper, pageNumber, pageSize);

        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser);
    }

    [Fact(DisplayName = "GetPageDetailedAsync should include roles")]
    public async Task DbContext_GetPageDetailedAsync()
    {
        const int pageNumber = 1;
        const int pageSize = 2;

        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var expectedUser = new StaticPagedList<User>(new[]
        {
            CreateUserWithRoles(TestUserId),
            CreateUserWithRoles(TestUserId2),
        }, pageNumber, pageSize, pageSize);

        var dbUser = await repository.GetPageDetailedAsync(pageNumber, pageSize);

        var excluder = CreateDatesExcluder();
        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser, e => e.Excluding(excluder));
    }

    [Fact(DisplayName = "GetPageDetailedAsync with roles should include roles")]
    public async Task DbContext_GetPageDetailedAsync_With_Roles()
    {
        const int pageNumber = 1;
        const int pageSize = 2;

        await using var context = PrepareContext();
        var repository = new UserRepository(context);
        var mapper = CreateShortUserWithRolesMapper();
        var expectedUser = new StaticPagedList<ShortUserWithRoles>(new[]
        {
            mapper.Map(CreateUserWithRoles(TestUserId)),
            mapper.Map(CreateUserWithRoles(TestUserId2)),
        }, pageNumber, pageSize, pageSize);

        var dbUser = await repository.GetPageDetailedAsync(mapper, pageNumber, pageSize);

        dbUser.Should().NotBeNull().And.BeEquivalentTo(expectedUser);
    }

    private static User CreateUserWithoutRoles(Guid id)
    {
        var user = new User(id, "TEST " + id, "1" + id);
        var clock = new TestSystemClock();
        user.UpdateCreateDate(clock.UtcNow.UtcDateTime);
        return user;
    }

    private static User CreateUserWithRoles(Guid id)
    {
        return CreateUserWithRoles(id, new Role(RoleName.User));
    }

    private static User CreateUserWithRoles(Guid id, Role role)
    {
        var user = CreateUserWithoutRoles(id);
        user.Roles.Add(role);
        var clock = new TestSystemClock();
        role.UpdateCreateDate(clock.UtcNow.UtcDateTime);
        return user;
    }

    private static AppDbContext PrepareContext()
    {
        var clock = new TestSystemClock();
        var appDbContext = new TestAppDbContextFactory().Create(clock);
        appDbContext.Users.RemoveRange(appDbContext.Users);
        var dbRole = appDbContext.Roles.Find(RoleName.User.Id);
        appDbContext.Users.Add(CreateUserWithRoles(TestUserId, dbRole!));
        appDbContext.Users.Add(CreateUserWithRoles(TestUserId2, dbRole!));
        appDbContext.SaveChanges();
        appDbContext.ChangeTracker.Clear();
        return appDbContext;
    }

    private IMapper<User, ShortUserWithoutRoles> CreateShortUserWhithoutRolesMapper()
    {
        return new Mapper<User, ShortUserWithoutRoles>(user => new ShortUserWithoutRoles
        {
            Id = user.Id,
            Nickname = user.Nickname
        });
    }

    private IMapper<User, ShortUserWithRoles> CreateShortUserWithRolesMapper()
    {
        return new Mapper<User, ShortUserWithRoles>(user => new ShortUserWithRoles
        {
            Id = user.Id,
            Nickname = user.Nickname,
            Roles = user.Roles.Select(e => e.Name.Name).ToList()
        });
    }

    private sealed class ShortUserWithoutRoles
    {
        public Guid Id { get; set; }
        public string Nickname { get; set; } = string.Empty;
    }

    private sealed class ShortUserWithRoles
    {
        public Guid Id { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
