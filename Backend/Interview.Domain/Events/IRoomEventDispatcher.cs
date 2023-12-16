using Interview.Domain.Events.Events;

namespace Interview.Domain.Events;

public interface IRoomEventDispatcher
{
    IEnumerable<IRoomEvent> Read();

    Task WriteAsync(IRoomEvent @event, CancellationToken cancellationToken = default);

    Task DropEventsAsync(Guid roomId, CancellationToken cancellationToken = default);

    Task WaitAsync(CancellationToken cancellationToken = default);
}
