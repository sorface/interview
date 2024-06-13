using Interview.Domain.Database;
using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.ChangeEntityProcessors;

public class RoomActiveStatusPreProcessor : EntityProcessorBase<Room>, IEntityPreProcessor
{
    private readonly ILogger<RoomActiveStatusPreProcessor> _logger;
    private readonly AppDbContext _db;

    public RoomActiveStatusPreProcessor(ILogger<RoomActiveStatusPreProcessor> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    protected override ValueTask ProcessAddedAsync(Room entity, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    protected override async ValueTask ProcessModifiedAsync(Room original, Room current, CancellationToken cancellationToken)
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
            current.Timer.ActualStartTime = DateTime.Now;

            _logger.LogWarning("Timer actual start time is updated for room [{id}]", current.Id);
        }

        _logger.LogInformation(@"update room with timer {timer}", original.Timer?.Duration);
    }
}
