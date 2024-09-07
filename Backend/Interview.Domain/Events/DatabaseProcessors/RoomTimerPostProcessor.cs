using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms;
using Interview.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomTimerPostProcessor : EntityPostProcessor<Room>
{
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly ILogger<RoomTimerPostProcessor> _logger;
    private readonly AppDbContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public RoomTimerPostProcessor(
        IRoomEventDispatcher eventDispatcher,
        ILogger<RoomTimerPostProcessor> logger,
        AppDbContext db,
        ICurrentUserAccessor currentUserAccessor)
    {
        _eventDispatcher = eventDispatcher;
        _logger = logger;
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public override async ValueTask ProcessModifiedAsync(
        Room original,
        Room current,
        CancellationToken cancellationToken)
    {
        if (original.Status == current.Status || current.Status != SERoomStatus.Active)
        {
            return;
        }

        await _db.Entry(current).Reference(e => e.Timer).LoadAsync(cancellationToken);

        if (current.Timer?.ActualStartTime is null)
        {
            return;
        }

        var roomTimer = new RoomTimerStartEventPayload
        {
            DurationSec = current.Timer.Duration.TotalSeconds,
            StartTime = current.Timer.ActualStartTime.Value,
        };

        var @event = new RoomTimerStartEvent(current.Id, roomTimer, _currentUserAccessor.GetUserIdOrThrow());

        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
