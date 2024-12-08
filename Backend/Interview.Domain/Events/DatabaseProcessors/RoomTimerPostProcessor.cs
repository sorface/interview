using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms;
using Interview.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomTimerPostProcessor(
    IRoomEventDispatcher eventDispatcher,
    ILogger<RoomTimerPostProcessor> logger,
    AppDbContext db,
    ICurrentUserAccessor currentUserAccessor)
    : EntityPostProcessor<Room>
{
    private readonly ILogger<RoomTimerPostProcessor> _logger = logger;

    public override async ValueTask ProcessModifiedAsync(
        Room original,
        Room current,
        CancellationToken cancellationToken)
    {
        if (original.Status == current.Status || current.Status != SERoomStatus.Active)
        {
            return;
        }

        await db.Entry(current).Reference(e => e.Timer).LoadAsync(cancellationToken);

        if (current.Timer?.ActualStartTime is null)
        {
            return;
        }

        var roomTimer = new RoomTimerStartEventPayload
        {
            DurationSec = current.Timer.Duration.TotalSeconds,
            StartTime = current.Timer.ActualStartTime.Value,
        };

        var @event = new RoomTimerStartEvent
        {
            RoomId = current.Id,
            Value = roomTimer,
            CreatedById = currentUserAccessor.GetUserIdOrThrow(),
        };

        await eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
