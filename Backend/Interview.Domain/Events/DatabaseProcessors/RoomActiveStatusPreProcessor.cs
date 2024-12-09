using Interview.Domain.Database;
using Interview.Domain.Database.Processors;
using Interview.Domain.Rooms;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.DatabaseProcessors;

public class RoomActiveStatusPreProcessor(ILogger<RoomActiveStatusPreProcessor> logger, AppDbContext db, ISystemClock systemClock)
    : EntityPreProcessor<Room>
{
    public override async ValueTask ProcessModifiedAsync(
        Room original, Room current, CancellationToken cancellationToken)
    {
        if (current.Timer is null)
        {
            await db.Entry(current).Reference(e => e.Timer).LoadAsync(cancellationToken);
            if (current.Timer is null)
            {
                logger.LogWarning("Timer is not present in room [{id}]", current.Id);
                return;
            }
        }

        if (original.Status != SERoomStatus.Active && current.Status == SERoomStatus.Active)
        {
            current.Timer.ActualStartTime = systemClock.UtcNow.UtcDateTime;

            logger.LogWarning("Timer actual start time is updated for room [{id}]", current.Id);
        }

        logger.LogInformation(@"update room with timer {timer}", original.Timer?.Duration);
    }
}
