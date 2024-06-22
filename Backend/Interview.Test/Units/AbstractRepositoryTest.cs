using FluentAssertions;
using Interview.Domain;
using Interview.Domain.Database;
using Interview.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Interview.Test.Units;

public abstract class AbstractRepositoryTest<T, TRepository>
    where T : Entity
    where TRepository : IRepository<T>
{
    protected readonly Guid _defaultGuid = new("1fc864ec-e262-47f7-9378-4f8bfaf2b87c");

    protected readonly Mock<AppDbContext> _databaseContext;
    protected readonly Mock<DbSet<T>> _databaseSet;

    protected readonly TRepository _repository;

    protected AbstractRepositoryTest()
    {
        _databaseContext = new Mock<AppDbContext>(new DbContextOptionsBuilder().Options);

        _databaseSet = new Mock<DbSet<T>>();

        var queryable = new List<T> { GetInstance() }.AsQueryable();

        _databaseSet.As<IQueryable<T>>()
            .Setup(m => m.Provider).Returns(queryable.Provider);
        _databaseSet.As<IQueryable<T>>()
            .Setup(m => m.Expression).Returns(queryable.Expression);
        _databaseSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType).Returns(queryable.ElementType);
        _databaseSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        _databaseContext.Setup(context => context.Set<T>()).Returns(_databaseSet.Object);

        var obj = _databaseContext.Object;
        obj.SystemClock = new TestSystemClock();
        _repository = GetRepository(obj);
    }

    [Fact(DisplayName = "Creating an entity")]
    public async Task CreateEntityTest()
    {
        var entity = GetInstance();

        var operationToken = new CancellationToken();

        await _repository.CreateAsync(entity, operationToken);

        _databaseSet.Verify(databaseSet => databaseSet.Add(entity), Times.Once);
        _databaseContext.Verify(databaseContext => databaseContext.SaveChangesAsync(operationToken), Times.Once);
    }

    [Fact(DisplayName = "Updating an entity by entity object")]
    public async Task UpdateEntityTest()
    {
        var roleEntity = GetInstance();

        var operationToken = new CancellationToken();

        await _repository.UpdateAsync(roleEntity, operationToken);

        _databaseSet.Verify(databaseSet => databaseSet.Update(roleEntity), Times.Once);
        _databaseContext.Verify(databaseContext =>
            databaseContext.SaveChangesAsync(operationToken), Times.Once);
    }

    [Fact(DisplayName = "Deleting an entity by Entity object")]
    public async Task DeleteEntityTest()
    {
        var roleEntity = GetInstance();

        var operationToken = new CancellationToken();

        await _repository.DeleteAsync(roleEntity, operationToken);

        _databaseSet.Verify(databaseSet => databaseSet.Remove(roleEntity), Times.Once);
        _databaseContext.Verify(databaseContext =>
            databaseContext.SaveChangesAsync(operationToken), Times.Once);
    }

    protected abstract TRepository GetRepository(AppDbContext databaseSet);

    protected abstract T GetInstance();
}
