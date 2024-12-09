using FluentAssertions;
using Interview.Domain.Permissions;
using Interview.Domain.Users;
using Interview.Domain.Users.Permissions;
using Interview.Domain.Users.Records;
using Interview.Domain.Users.Roles;
using Interview.Domain.Users.Service;
using Moq;
using NSpecifications;

namespace Interview.Test.Units.Users;

public class UserServiceTest
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRoleRepository> _mockRoleRepository;
    private readonly Mock<IPermissionRepository> _mockPermissionRepository;
    private readonly Mock<ISecurityService> _mockSecurityService;

    private readonly UserService _userService;

    public UserServiceTest()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockPermissionRepository = new Mock<IPermissionRepository>();
        _mockSecurityService = new Mock<ISecurityService>();

        _userService = new UserService(_mockUserRepository.Object, _mockRoleRepository.Object,
            _mockPermissionRepository.Object, _mockSecurityService.Object);
    }

    [Fact]
    public async Task UpsertUsersWhenUserNotExistsInDatabaseAndRoleNotFound()
    {
        var user = new User("Dima", "1");

        _mockUserRepository.Setup(repository =>
                repository.FindByExternalIdAsync(user.ExternalId, default))
            .Returns(() => Task.FromResult<User?>(null));

        _mockRoleRepository.Setup(repository =>
                repository.FindAsync(It.IsAny<ISpecification<Role>>(), default))
            .Returns(() => Task.FromResult(new List<Role>()));

        var throwsAsync = await Assert.ThrowsAsync<Domain.NotFoundException>(
            async () => await _userService.UpsertByExternalIdAsync(user));

        throwsAsync.Message.Should().NotBeNull().And.NotBeEmpty();

        _mockUserRepository.Verify(repository =>
            repository.FindByExternalIdAsync(user.ExternalId, default), Times.Once);
        _mockRoleRepository.Verify(repository =>
            repository.FindAsync(It.IsAny<ISpecification<Role>>(), default), Times.Once);
        _mockUserRepository.Verify(repository =>
            repository.CreateAsync(It.IsAny<User>(), default), Times.Never);
    }

    [Fact(DisplayName = "Get of the user permissions")]
    public async Task GetPermissionsTest()
    {
        var user = new User("Dima", "1");
        var expectedResult = new Dictionary<string, List<PermissionItem>>();

        _mockUserRepository.Setup(repository =>
                repository.FindFirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult<User?>(user));

        _mockUserRepository.Setup(repository =>
                repository.FindPermissionByUserId(user.Id, default))
            .Returns(() => Task.FromResult(expectedResult));

        var result = await _userService.GetPermissionsAsync(user.Id, default);

        _mockUserRepository.Verify(repository =>
                repository.FindFirstOrDefaultAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUserRepository.Verify(repository =>
                repository.FindPermissionByUserId(It.Is<Guid>(it => it == user.Id), It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(expectedResult, result);
    }
}
