using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Repository;

public interface IRepository<T>
    where T : Entity
{
    Task<bool> HasAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

    Task<bool> HasDetailedAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

    Task CreateAsync(T entity, CancellationToken cancellationToken = default);

    Task CreateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task<T?> FindFirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

    Task<TRes?> FindFirstOrDefaultAsync<TRes>(ISpecification<T> specification, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default);

    Task<T?> FindFirstOrDefaultDetailedAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

    Task<TRes?> FindFirstOrDefaultDetailedAsync<TRes>(ISpecification<T> specification, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default);

    Task<List<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

    Task<List<TRes>> FindAsync<TRes>(ISpecification<T> specification, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default);

    Task<List<T>> FindDetailed(ISpecification<T> specification, CancellationToken cancellationToken = default);

    Task<List<TRes>> FindDetailed<TRes>(ISpecification<T> specification, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default);

    Task<T?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TRes?> FindByIdAsync<TRes>(Guid id, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default);

    Task<T?> FindByIdDetailedAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TRes?> FindByIdDetailedAsync<TRes>(Guid id, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default);

    Task<List<T>> FindByIdsAsync(ICollection<Guid> id, CancellationToken cancellationToken = default);

    Task<List<T>> FindByIdsDetailedAsync(ICollection<Guid> id, CancellationToken cancellationToken = default);

    Task<List<TRes>> FindByIdsAsync<TRes>(ICollection<Guid> id, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default);

    Task<List<TRes>> FindByIdsDetailedAsync<TRes>(ICollection<Guid> id, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    Task<bool> IsExistsWithIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IPagedList<T>> GetPageAsync(ISpecification<T> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<IPagedList<T>> GetPageDetailedAsync(ISpecification<T> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<IPagedList<T>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<IPagedList<T>> GetPageDetailedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<IPagedList<TRes>> GetPageAsync<TRes>(ISpecification<T> specification, IMapper<T, TRes> mapper, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<IPagedList<TRes>> GetPageDetailedAsync<TRes>(ISpecification<T> specification, IMapper<T, TRes> mapper, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<IPagedList<TRes>> GetPageAsync<TRes>(IMapper<T, TRes> mapper, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<IPagedList<TRes>> GetPageDetailedAsync<TRes>(IMapper<T, TRes> mapper, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
