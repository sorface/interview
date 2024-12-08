using Interview.Domain.Database;
using Interview.Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Interview.Domain;

public class ArchiveService<T>(AppDbContext db)
    where T : ArchiveEntity
{
    public async Task<T> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var set = db.Set<T>();
        var question = await set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (question == null)
        {
            throw NotFoundException.Create<T>(id);
        }

        question.IsArchived = true;
        await db.SaveChangesAsync(cancellationToken);
        return question;
    }

    public async Task<T> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var set = db.Set<T>();
        var question = await set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (question == null)
        {
            throw NotFoundException.Create<T>(id);
        }

        if (!question.IsArchived)
        {
            throw new UserException($"The {typeof(T).Name} is not archived");
        }

        question.IsArchived = false;
        await db.SaveChangesAsync(cancellationToken);
        return question;
    }
}
