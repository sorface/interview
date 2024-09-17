using System.Runtime.CompilerServices;
using Interview.Domain.Events.Storage;
using NSpecifications;

namespace Interview.Test;

public sealed class InMemoryHotEventStorage : IHotEventStorage
{
    private readonly List<IStorageEvent> _storage = new();

    public ValueTask AddAsync(IStorageEvent @event, CancellationToken cancellationToken)
    {
        _storage.Add(@event);
        return ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<IReadOnlyCollection<IStorageEvent>> GetBySpecAsync(ISpecification<IStorageEvent> spec, int chunkSize, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var res in _storage.Where(spec.IsSatisfiedBy).Chunk(chunkSize).ToAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return res;
        }
    }

    public async IAsyncEnumerable<IReadOnlyCollection<IStorageEvent>> GetLatestBySpecAsync(ISpecification<IStorageEvent> spec, int chunkSize, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var res in _storage.Where(spec.IsSatisfiedBy).OrderByDescending(e => e.CreatedAt).Chunk(chunkSize).ToAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return res;
        }
    }

    public ValueTask DeleteAsync(IEnumerable<IStorageEvent> items, CancellationToken cancellationToken)
    {
        foreach (var e in items)
        {
            _storage.Remove(e);
        }

        return ValueTask.CompletedTask;
    }
}
