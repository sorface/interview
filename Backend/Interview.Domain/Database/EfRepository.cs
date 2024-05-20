using Interview.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using NSpecifications;
using X.PagedList;

namespace Interview.Domain.Database;

public abstract class EfRepository<T> : IRepository<T>
    where T : Entity
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> Set;

    public EfRepository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public Task<bool> HasAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set).AsNoTracking().AnyAsync(specification.Expression, cancellationToken);
    }

    public Task<bool> HasDetailedAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set).AsNoTracking().AnyAsync(specification.Expression, cancellationToken);
    }

    public Task CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Set.Add(entity);
        return Db.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await Set.AddRangeAsync(entities, cancellationToken);
        await Db.SaveChangesAsync(cancellationToken);
    }

    public Task<T?> FindFirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set).FirstOrDefaultAsync(specification.Expression, cancellationToken);
    }

    public Task<TRes?> FindFirstOrDefaultAsync<TRes>(ISpecification<T> specification, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set).Where(specification.Expression).Select(mapper.Expression).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<T?> FindFirstOrDefaultDetailedAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set).FirstOrDefaultAsync(specification.Expression, cancellationToken);
    }

    public Task<TRes?> FindFirstOrDefaultDetailedAsync<TRes>(ISpecification<T> specification, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set).Where(specification.Expression).Select(mapper.Expression).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set).Where(specification.Expression).ToListAsync(cancellationToken);
    }

    public Task<List<TRes>> FindAsync<TRes>(ISpecification<T> specification, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set).Where(specification.Expression).Select(mapper.Expression).ToListAsync(cancellationToken);
    }

    public Task<List<T>> FindDetailed(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set).Where(specification.Expression).ToListAsync(cancellationToken);
    }

    public Task<List<TRes>> FindDetailed<TRes>(ISpecification<T> specification, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set).Where(specification.Expression).Select(mapper.Expression).ToListAsync(cancellationToken);
    }

    public Task<T?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public Task<TRes?> FindByIdAsync<TRes>(Guid id, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set).Where(e => e.Id == id).Select(mapper.Expression).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<T?> FindByIdDetailedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public Task<TRes?> FindByIdDetailedAsync<TRes>(Guid id, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set).Where(e => e.Id == id).Select(mapper.Expression).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<T>> FindByIdsAsync(ICollection<Guid> id, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set).Where(e => id.Contains(e.Id)).ToListAsync(cancellationToken);
    }

    public Task<List<T>> FindByIdsDetailedAsync(ICollection<Guid> id, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set).Where(e => id.Contains(e.Id)).ToListAsync(cancellationToken);
    }

    public Task<List<TRes>> FindByIdsAsync<TRes>(ICollection<Guid> id, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set).Where(e => id.Contains(e.Id)).Select(mapper.Expression).ToListAsync(cancellationToken);
    }

    public Task<List<TRes>> FindByIdsDetailedAsync<TRes>(ICollection<Guid> id, IMapper<T, TRes> mapper, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set).Where(e => id.Contains(e.Id)).Select(mapper.Expression).ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Set.Update(entity);
        return Db.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Set.Remove(entity);
        return Db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsExistsWithIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        ApplyNonDetail(Set).AnyAsync(entity => entity.Id == id, cancellationToken);

    public Task<IPagedList<T>> GetPageAsync(ISpecification<T> specification, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set)
            .OrderBy(entity => entity.Id)
            .Where(specification.Expression)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<IPagedList<T>> GetPageDetailedAsync(
        ISpecification<T> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set)
            .OrderBy(entity => entity.Id)
            .Where(specification.Expression)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<IPagedList<T>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set).OrderBy(entity => entity.Id).ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<IPagedList<T>> GetPageDetailedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set)
            .OrderBy(entity => entity.Id)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<IPagedList<TRes>> GetPageAsync<TRes>(
        ISpecification<T> specification,
        IMapper<T, TRes> mapper,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set)
            .Where(specification.Expression)
            .OrderBy(entity => entity.Id)
            .Select(mapper.Expression)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<IPagedList<TRes>> GetPageDetailedAsync<TRes>(
        ISpecification<T> specification,
        IMapper<T, TRes> mapper,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set)
            .Where(specification.Expression)
            .OrderBy(entity => entity.Id)
            .Select(mapper.Expression)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<IPagedList<TRes>> GetPageAsync<TRes>(
        IMapper<T, TRes> mapper,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return ApplyNonDetail(Set)
            .OrderBy(entity => entity.Id)
            .Select(mapper.Expression)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<IPagedList<TRes>> GetPageDetailedAsync<TRes>(
        IMapper<T, TRes> mapper,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return ApplyIncludes(Set)
            .OrderBy(entity => entity.Id)
            .Select(mapper.Expression)
            .ToPagedListAsync(pageNumber, pageSize, cancellationToken);
    }

    protected virtual IQueryable<T> ApplyIncludes(DbSet<T> set) => set;

    protected virtual IQueryable<T> ApplyNonDetail(DbSet<T> set) => set;
}
