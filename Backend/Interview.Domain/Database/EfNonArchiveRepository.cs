using Interview.Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain.Database;

public abstract class EfNonArchiveRepository<T>(AppDbContext db) : EfRepository<T>(db), INonArchiveRepository<T>
    where T : ArchiveEntity
{
    protected override IQueryable<T> ApplyIncludes(DbSet<T> set) => set.Where(it => !it.IsArchived);

    protected override IQueryable<T> ApplyNonDetail(DbSet<T> set) => set.Where(it => !it.IsArchived);
}
