using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms;
using Interview.Domain.Users;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomPostProcessor(IRoomEventDispatcher eventDispatcher, ICurrentUserAccessor currentUserAccessor) : EntityPostProcessor<Room>
{
    public override async ValueTask ProcessModifiedAsync(Room original, Room current, CancellationToken cancellationToken)
    {
        var @event = original.Status != current.Status
            ? new RoomChangeStatusEvent
            {
                RoomId = current.Id,
                Value = current.Status.EnumValue.ToString(),
                CreatedById = currentUserAccessor.GetUserIdOrThrow(),
            }
            : null;

        if (@event is null)
        {
            return;
        }

        await eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
