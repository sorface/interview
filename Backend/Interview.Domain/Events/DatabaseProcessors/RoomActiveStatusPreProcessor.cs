using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Rooms;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomActiveStatusPreProcessor : EntityPreProcessor<Room>
{
    private readonly ILogger<RoomActiveStatusPreProcessor> _logger;
    private readonly AppDbContext _db;
    private readonly ISystemClock _systemClock;

    public RoomActiveStatusPreProcessor(ILogger<RoomActiveStatusPreProcessor> logger, AppDbContext db, ISystemClock systemClock)
    {
        _logger = logger;
        _db = db;
        _systemClock = systemClock;
    }

    public override async ValueTask ProcessModifiedAsync(
        Room original, Room current, CancellationToken cancellationToken)
    {
        if (current.Timer is null)
        {
            await _db.Entry(current).Reference(e => e.Timer).LoadAsync(cancellationToken);
            if (current.Timer is null)
            {
                _logger.LogWarning("Timer is not present in room [{id}]", current.Id);
                return;
            }
        }

        if (original.Status != SERoomStatus.Active && current.Status == SERoomStatus.Active)
        {
            current.Timer.ActualStartTime = _systemClock.UtcNow.UtcDateTime;

            _logger.LogWarning("Timer actual start time is updated for room [{id}]", current.Id);
        }

        _logger.LogInformation(@"update room with timer {timer}", original.Timer?.Duration);
    }
}
