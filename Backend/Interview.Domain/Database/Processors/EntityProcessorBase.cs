using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomConfigurations;

namespace Interview.Domain.Database.Processors;

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

    public async ValueTask ProcessModifiedAsync(
        IReadOnlyCollection<(Entity Original, Entity Current)> entities,
        CancellationToken cancellationToken)
    {
        foreach (var (original, current) in entities)
        {
            if (original is T originalC && current is T currentC)
            {
                await ProcessModifiedAsync(originalC, currentC, cancellationToken);
            }
        }
    }

    public abstract ValueTask ProcessAddedAsync(T entity, CancellationToken cancellationToken);

    public abstract ValueTask ProcessModifiedAsync(T original, T current, CancellationToken cancellationToken);
}
