using NSpecifications;

namespace Interview.Domain.Events.Storage;

public interface IHotEventStorage
{
    ValueTask AddAsync(IStorageEvent @event, CancellationToken cancellationToken);

    IAsyncEnumerable<IReadOnlyCollection<IStorageEvent>> GetBySpecAsync(ISpecification<IStorageEvent> spec, int chunkSize, CancellationToken cancellationToken);

    IAsyncEnumerable<IReadOnlyCollection<IStorageEvent>> GetLatestBySpecAsync(ISpecification<IStorageEvent> spec, int chunkSize, CancellationToken cancellationToken);

    ValueTask DeleteAsync(IEnumerable<IStorageEvent> items, CancellationToken cancellationToken);
}
