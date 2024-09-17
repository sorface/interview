using NSpecifications;

namespace Interview.Domain.Events.Storage;

public sealed class EmptyHotEventStorage : IHotEventStorage
{
    public ValueTask AddAsync(IStorageEvent @event, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public IAsyncEnumerable<IReadOnlyCollection<IStorageEvent>> GetBySpecAsync(ISpecification<IStorageEvent> spec, int chunkSize, CancellationToken cancellationToken)
    {
        return AsyncEnumerable.Empty<IReadOnlyCollection<IStorageEvent>>();
    }

    public IAsyncEnumerable<IReadOnlyCollection<IStorageEvent>> GetLatestBySpecAsync(ISpecification<IStorageEvent> spec, int chunkSize, CancellationToken cancellationToken)
    {
        return AsyncEnumerable.Empty<IReadOnlyCollection<IStorageEvent>>();
    }

    public ValueTask DeleteAsync(IEnumerable<IStorageEvent> items, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
