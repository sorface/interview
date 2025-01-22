using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Permissions;
using Interview.Domain.Tags;
using Interview.Domain.Users;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Interview.Test.Integrations;

public class EntityAccessControlTest : IDisposable
{
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessorMock;
    private readonly EntityAccessControl _entityAccessControl;
    private readonly AppDbContext _appDbContext;

    public EntityAccessControlTest()
    {
        var clock = new TestSystemClock();
        _appDbContext = new TestAppDbContextFactory().Create(clock);

        // Создаем мок для ICurrentUserAccessor
        _currentUserAccessorMock = new Mock<ICurrentUserAccessor>();
        _entityAccessControl = new EntityAccessControl(_currentUserAccessorMock.Object, _appDbContext);
    }

    public void Dispose()
    {
        _appDbContext.Dispose();
    }

    [Fact]
    public async Task EnsureEditPermissionAsync_AdminUser_ShouldNotThrowException()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _currentUserAccessorMock.Setup(x => x.IsAdmin()).Returns(true);

        // Act
        Func<Task> act = async () => await _entityAccessControl.EnsureEditPermissionAsync<Tag>(entityId, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureEditPermissionAsync_NonAdminUserWithPermission_ShouldNotThrowException()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        TestAppDbContextFactory.AddUser(_appDbContext, currentUserId, "Usr_" + currentUserId);

        _currentUserAccessorMock.Setup(x => x.IsAdmin()).Returns(false);
        _currentUserAccessorMock.Setup(x => x.UserId).Returns(currentUserId);

        _appDbContext.Tag.Add(new Tag { Id = entityId, CreatedById = currentUserId });
        await _appDbContext.SaveChangesAsync();
        _appDbContext.ChangeTracker.Clear();

        // Act
        Func<Task> act = async () => await _entityAccessControl.EnsureEditPermissionAsync<Tag>(entityId, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureEditPermissionAsync_NonAdminUserWithoutPermission_ShouldThrowAccessDeniedException()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        TestAppDbContextFactory.AddUser(_appDbContext, currentUserId, "Usr_" + currentUserId);
        TestAppDbContextFactory.AddUser(_appDbContext, otherUserId, "Usr_" + currentUserId);

        _currentUserAccessorMock.Setup(x => x.IsAdmin()).Returns(false);
        _currentUserAccessorMock.Setup(x => x.UserId).Returns(currentUserId);

        _appDbContext.Tag.Add(new Tag { Id = entityId, CreatedById = otherUserId });
        await _appDbContext.SaveChangesAsync();
        _appDbContext.ChangeTracker.Clear();

        // Act
        Func<Task> act = async () => await _entityAccessControl.EnsureEditPermissionAsync<Tag>(entityId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AccessDeniedException>().WithMessage("You do not have permission to modify the entity.");
    }
}
