using Interview.Domain.Repository;

namespace Interview.Domain.Database.Processors;

public abstract class EntityPreProcessor<T> : EntityProcessorBase<T>, IEntityPreProcessor
    where T : Entity
{
    protected override ValueTask ProcessAddedAsync(T entity, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    protected override ValueTask ProcessModifiedAsync(T original, T current, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
