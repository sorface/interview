using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Events.DatabaseProcessors.Records.Room;
using Interview.Domain.Rooms;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomTimerPostProcessor : EntityPostProcessor<Room>
{
    private readonly IRoomEventDispatcher _eventDispatcher;
    private readonly ILogger<RoomTimerPostProcessor> _logger;
    private readonly AppDbContext _db;

    public RoomTimerPostProcessor(
        IRoomEventDispatcher eventDispatcher,
        ILogger<RoomTimerPostProcessor> logger,
        AppDbContext db)
    {
        _eventDispatcher = eventDispatcher;
        _logger = logger;
        _db = db;
    }

    public override async ValueTask ProcessModifiedAsync(
        Room original,
        Room current,
        CancellationToken cancellationToken)
    {
        await _db.Entry(current).Reference(e => e.Timer).LoadAsync(cancellationToken);

        if (current.Timer?.ActualStartTime is null)
        {
            return;
        }

        var roomTimer = new RoomTimerPayload
        {
            Duration = current.Timer.Duration,
            StartTime = ((DateTimeOffset)current.Timer.ActualStartTime).ToUnixTimeSeconds(),
        };

        var @event = new RoomTimerStartEvent(current.Id, roomTimer);

        await _eventDispatcher.WriteAsync(@event, cancellationToken);
    }
}
