using Interview.Domain.Database;
using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.ChangeEntityProcessors;

public class RoomActiveStatusPreProcessor : IEntityPreProcessor
{
    private readonly ILogger<RoomActiveStatusPreProcessor> _logger;
    private readonly AppDbContext _db;

    public RoomActiveStatusPreProcessor(ILogger<RoomActiveStatusPreProcessor> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public ValueTask ProcessAddedAsync(IReadOnlyCollection<Entity> entities, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask ProcessModifiedAsync(
        IReadOnlyCollection<(Entity Original, Entity Current)> entities,
        CancellationToken cancellationToken)
    {
        foreach (var (originalEntity, currentEntity) in entities)
        {
            switch (originalEntity)
            {
                case Room originalC when currentEntity is Room currentC:
                    {
                        if (currentC.Timer is null)
                        {
                            await _db.Entry(currentC).Reference(e => e.Timer).LoadAsync(cancellationToken);
                            if (currentC.Timer is null)
                            {
                                _logger.LogWarning("Timer is not present in room [{id}]", currentC.Id);
                                break;
                            }
                        }

                        if (originalC.Status != SERoomStatus.Active && currentC.Status == SERoomStatus.Active)
                        {
                            currentC.Timer.ActualStartTime = DateTime.Now;

                            _logger.LogWarning("Timer actual start time is updated for room [{id}]", currentC.Id);
                        }

                        _logger.LogInformation(@"update room with timer {timer}", originalC.Timer?.Duration);

                        break;
                    }
            }
        }
    }
}
