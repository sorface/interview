using Interview.Domain.Events.Events;
using Interview.Domain.Repository;
using Interview.Domain.Rooms;

namespace Interview.Domain.Events.ChangeEntityProcessors;

public class RoomChangeEntityProcessor : IEntityPostProcessor
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomChangeEntityProcessor(IRoomEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public ValueTask ProcessAddedAsync(IReadOnlyCollection<Entity> entities, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask ProcessModifiedAsync(IReadOnlyCollection<(Entity Original, Entity Current)> entities, CancellationToken cancellationToken)
    {
        foreach (var (originalEntity, currentEntity) in entities)
        {
            switch (originalEntity)
            {
                case Room originalC when currentEntity is Room currentC:
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

    private static IRoomEvent? CreateEvent(Room current, Room? original)
    {
        if (original is null || original.Status != current.Status)
        {
            return new ChangeRoomStatusEvent(current.Id, current.Status.EnumValue.ToString());
        }

        return null;
    }

    public sealed class ChangeRoomStatusEvent : RoomEvent
    {
        public ChangeRoomStatusEvent(Guid roomId, string? value)
            : base(roomId, EventType.ChangeRoomStatus, value, false)
        {
        }
    }
}
