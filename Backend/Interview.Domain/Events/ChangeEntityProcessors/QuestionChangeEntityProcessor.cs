using Interview.Domain.Connections;
using Interview.Domain.Events.Events;
using Interview.Domain.Questions;
using Interview.Domain.Repository;

namespace Interview.Domain.Events.ChangeEntityProcessors;

public class QuestionChangeEntityProcessor : IEntityPostProcessor
{
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly IActiveRoomSource _connectUserSource;

    public QuestionChangeEntityProcessor(IRoomEventDispatcher eventDispatcher, IActiveRoomSource connectUserSource)
    {
        _eventDispatcher = eventDispatcher;
        _connectUserSource = connectUserSource;
    }

    public ValueTask ProcessAddedAsync(IReadOnlyCollection<Entity> entities, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask ProcessModifiedAsync(IReadOnlyCollection<(Entity Original, Entity Current)> entities, CancellationToken cancellationToken = default)
    {
        foreach (var (originalEntity, currentEntity) in entities)
        {
            if (originalEntity is not Question original || currentEntity is not Question current)
            {
                continue;
            }

            foreach (var roomId in _connectUserSource.ActiveRooms)
            {
                await _eventDispatcher.WriteAsync(CreateEvent(current, original, roomId), cancellationToken);
            }
        }
    }

    private IRoomEvent CreateEvent(Question current, Question original, Guid roomId)
    {
        return new RoomEvent<EventPayload>(
            roomId,
            EventType.ChangeQuestion,
            new EventPayload(current.Id, original.Value, current.Value),
            false);
    }

    public sealed class EventPayload
    {
        public Guid QuestionId { get; }

        public string OldValue { get; }

        public string NewValue { get; }

        public EventPayload(Guid questionId, string oldValue, string newValue)
        {
            QuestionId = questionId;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
