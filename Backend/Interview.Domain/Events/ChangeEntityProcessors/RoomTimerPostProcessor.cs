using Interview.Domain.Events.Events;
using Interview.Domain.Repository;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomTimers;

namespace Interview.Domain.Events.ChangeEntityProcessors;

public class RoomTimerPostProcessor : IEntityPostProcessor
{
    private readonly IRoomEventDispatcher _eventDispatcher;

    public RoomTimerPostProcessor(IRoomEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
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
                case Room originalRoomTimer when currentEntity is Room currentRoomTimer:
                    {
                        if (currentRoomTimer.Timer is null)
                        {
                            break;
                        }

                        if (currentRoomTimer.Timer?.ActualStartTime is null)
                        {
                            return;
                        }

                        var timerEvent = new TimerEvent(
                            currentRoomTimer.Id,
                            new RoomTimerPayload
                            {
                                Duration = currentRoomTimer.Timer.Duration,
                                StartTime = ((DateTimeOffset)currentRoomTimer.Timer.ActualStartTime)
                                    .ToUnixTimeSeconds(),
                            });

                        await _eventDispatcher.WriteAsync(timerEvent, cancellationToken);

                        break;
                    }
            }
        }
    }

    private sealed class TimerEvent : RoomEvent<RoomTimerPayload>
    {
        public TimerEvent(Guid roomId, RoomTimerPayload? value)
            : base(roomId, EventType.StartRoomTimer, value, false)
        {
        }
    }

    private sealed class RoomTimerPayload
    {
        public long Duration { get; set; }

        public long StartTime { get; set; }
    }
}
