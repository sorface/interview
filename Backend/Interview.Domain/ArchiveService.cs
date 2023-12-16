using Interview.Domain.Repository;

namespace Interview.Domain;

public class ArchiveService<T>
    where T : ArchiveEntity
{
    private readonly IRepository<T> _repository;

    public ArchiveService(IRepository<T> repository)
    {
        _repository = repository;
    }

    public async Task<T> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _repository.FindByIdAsync(id, cancellationToken);

        if (question == null)
        {
            throw NotFoundException.Create<T>(id);
        }

        question.IsArchived = true;

        await _repository.UpdateAsync(question, cancellationToken);

        return question;
    }

    public async Task<T> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _repository.FindByIdAsync(id, cancellationToken);

        if (question == null)
        {
            throw NotFoundException.Create<T>(id);
        }

        if (!question.IsArchived)
        {
            throw new UserException($"The {typeof(T).Name} is not archived");
        }

        question.IsArchived = false;

        await _repository.UpdateAsync(question, cancellationToken);

        return question;
    }
}
