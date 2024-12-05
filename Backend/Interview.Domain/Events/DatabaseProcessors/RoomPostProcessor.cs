using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms;
using Interview.Domain.Users;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomPostProcessor : EntityPostProcessor<Room>
{
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public RoomPostProcessor(IRoomEventDispatcher eventDispatcher, ICurrentUserAccessor currentUserAccessor)
    {
        _eventDispatcher = eventDispatcher;
        _currentUserAccessor = currentUserAccessor;
    }

    public override async ValueTask ProcessModifiedAsync(Room original, Room current, CancellationToken cancellationToken)
    {
        var @event = original.Status != current.Status
            ? new RoomChangeStatusEvent(current.Id, current.Status.EnumValue.ToString(), _currentUserAccessor.GetUserIdOrThrow())
            : null;

        if (@event is null)
        {
            return;
        }

        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
