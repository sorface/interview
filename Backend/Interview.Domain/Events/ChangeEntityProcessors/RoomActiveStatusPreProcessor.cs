using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Microsoft.Extensions.Logging;

namespace Interview.Domain.Events.ChangeEntityProcessors;

public class RoomActiveStatusPreProcessor : IEntityPreProcessor
{
    private readonly ILogger<RoomActiveStatusPreProcessor> _logger;

    public RoomActiveStatusPreProcessor(ILogger<RoomActiveStatusPreProcessor> logger)
    {
        _logger = logger;
    }

    public ValueTask ProcessAddedAsync(IReadOnlyCollection<Entity> entities, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ProcessModifiedAsync(
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
                            _logger.LogWarning("Timer is not present in room [{id}]", currentC.Id);
                            break;
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

        return ValueTask.CompletedTask;
    }
}
