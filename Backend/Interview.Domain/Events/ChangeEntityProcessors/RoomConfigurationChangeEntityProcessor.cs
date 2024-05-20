using Interview.Domain.Events.Events;
using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomConfigurations;

namespace Interview.Domain.Events.ChangeEntityProcessors;

public class RoomConfigurationChangeEntityProcessor : IEntityPostProcessor
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomConfigurationChangeEntityProcessor(IRoomEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public async ValueTask ProcessAddedAsync(IReadOnlyCollection<Entity> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities.OfType<RoomConfiguration>())
        {
            var @event = CreateEvent(entity, null);
            if (@event is not null)
            {
                await _eventDispatcher.WriteAsync(@event, cancellationToken);
            }
        }
    }

    public async ValueTask ProcessModifiedAsync(IReadOnlyCollection<(Entity Original, Entity Current)> entities, CancellationToken cancellationToken)
    {
        foreach (var (originalEntity, currentEntity) in entities)
        {
            switch (originalEntity)
            {
                case RoomConfiguration originalC when currentEntity is RoomConfiguration currentC:
                    {
                        var e = CreateEvent(currentC, originalC);
                        if (e is not null)
                        {
                            await _eventDispatcher.WriteAsync(e, cancellationToken);
                        }

                        break;
                    }
            }
        }
    }

    private static IRoomEvent? CreateEvent(RoomConfiguration current, RoomConfiguration? original)
    {
        if (original is null || original.CodeEditorContent != current.CodeEditorContent)
        {
            return new ChangeCodeEditorRoomEvent(current.Id, current.CodeEditorContent);
        }

        return null;
    }

    public sealed class ChangeCodeEditorRoomEvent : RoomEvent
    {
        public ChangeCodeEditorRoomEvent(Guid roomId, string? value)
            : base(roomId, EventType.ChangeCodeEditor, value, false)
        {
        }
    }
}
