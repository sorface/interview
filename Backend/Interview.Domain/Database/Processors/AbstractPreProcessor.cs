using Interview.Domain.Events.ChangeEntityProcessors;
using Interview.Domain.Repository;

namespace Interview.Domain.Database.Processors
{
    public abstract class AbstractPreProcessor : IEntityPreProcessor
    {
        public virtual ValueTask ProcessAddedAsync(IReadOnlyCollection<Entity> entities, CancellationToken cancellationToken)
        {
            foreach (var entity in entities)
            {
                AddEntityHandler(entity, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return ValueTask.FromCanceled(cancellationToken);
                }
            }

            return ValueTask.CompletedTask;
        }

        public virtual ValueTask ProcessModifiedAsync(IReadOnlyCollection<(Entity Original, Entity Current)> entities, CancellationToken cancellationToken)
        {
            foreach (var entity in entities)
            {
                ModifyOriginalEntityHandler(entity.Original, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return ValueTask.FromCanceled(cancellationToken);
                }
            }

            return ValueTask.CompletedTask;
        }

        protected abstract void AddEntityHandler(Entity entity, CancellationToken cancellationToken = default);

        protected abstract void ModifyOriginalEntityHandler(Entity entity, CancellationToken cancellationToken = default);
    }
}
