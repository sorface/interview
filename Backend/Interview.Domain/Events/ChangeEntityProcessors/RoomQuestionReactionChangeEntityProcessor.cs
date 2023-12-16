using Interview.Domain.Events.Events;
using Interview.Domain.Reactions;
using Interview.Domain.Repository;
using Interview.Domain.RoomQuestionReactions;

namespace Interview.Domain.Events.ChangeEntityProcessors;

public class RoomQuestionReactionChangeEntityProcessor : IEntityPostProcessor
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomQuestionReactionChangeEntityProcessor(IRoomEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public async ValueTask ProcessAddedAsync(IReadOnlyCollection<Entity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities.OfType<RoomQuestionReaction>())
        {
            await _eventDispatcher.WriteAsync(CreateEvent(entity), cancellationToken);
        }
    }

    public ValueTask ProcessModifiedAsync(IReadOnlyCollection<(Entity Original, Entity Current)> entities, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    private IRoomEvent CreateEvent(RoomQuestionReaction entity)
    {
        return new RoomEvent<RoomQuestionReactionPayload>(
            entity.RoomQuestion!.Room!.Id,
            entity.Reaction!.Type.Name,
            new RoomQuestionReactionPayload(entity.Sender!.Id, entity.Payload),
            false);
    }

    private sealed class RoomQuestionReactionPayload
    {
        public Guid UserId { get; }

        public string? Payload { get; }

        public RoomQuestionReactionPayload(Guid userId, string? payload)
        {
            UserId = userId;
            Payload = payload;
        }
    }
}
