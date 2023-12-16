using Interview.Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Interview.Infrastructure.Database;

public abstract class EfNonArchiveRepository<T> : EfRepository<T>, INonArchiveRepository<T>
    where T : ArchiveEntity
{
    protected EfNonArchiveRepository(AppDbContext db)
        : base(db)
    {
    }

    protected override IQueryable<T> ApplyIncludes(DbSet<T> set) => set.Where(it => !it.IsArchived);

    protected override IQueryable<T> ApplyNonDetail(DbSet<T> set) => set.Where(it => !it.IsArchived);
}
