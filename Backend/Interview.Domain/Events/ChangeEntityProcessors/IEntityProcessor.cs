using Interview.Domain.Repository;

namespace Interview.Domain.Events.ChangeEntityProcessors;

public interface IEntityProcessor
{
    ValueTask ProcessAddedAsync(IReadOnlyCollection<Entity> entities, CancellationToken cancellationToken);

    ValueTask ProcessModifiedAsync(IReadOnlyCollection<(Entity Original, Entity Current)> entities, CancellationToken cancellationToken);
}

public interface IEntityPreProcessor : IEntityProcessor
{
}

public interface IEntityPostProcessor : IEntityProcessor
{
}

public abstract class EntityProcessorBase<T> : IEntityProcessor
    where T : Entity
{
    public async ValueTask ProcessAddedAsync(IReadOnlyCollection<Entity> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities)
        {
            if (entity is T obj)
            {
                await ProcessAddedAsync(obj, cancellationToken);
            }
        }
    }

    public async ValueTask ProcessModifiedAsync(IReadOnlyCollection<(Entity Original, Entity Current)> entities, CancellationToken cancellationToken)
    {
        foreach (var (original, current) in entities)
        {
            if (original is T originalC && current is T currentC)
            {
                await ProcessModifiedAsync(originalC, currentC, cancellationToken);
            }
        }
    }

    protected abstract ValueTask ProcessAddedAsync(T entity, CancellationToken cancellationToken);

    protected abstract ValueTask ProcessModifiedAsync(T original, T current, CancellationToken cancellationToken);
}
