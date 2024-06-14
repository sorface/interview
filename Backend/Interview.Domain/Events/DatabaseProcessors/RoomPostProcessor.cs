using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomPostProcessor : EntityPostProcessor<Room>
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomPostProcessor(IRoomEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    protected override async ValueTask ProcessModifiedAsync(Room original, Room current, CancellationToken cancellationToken)
    {
        var @event = original.Status != current.Status
            ? new RoomChangeStatusEvent(current.Id, current.Status.EnumValue.ToString())
            : null;

        if (@event is null)
        {
            return;
        }

        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
