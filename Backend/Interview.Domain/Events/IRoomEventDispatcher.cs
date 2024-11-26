using Interview.Domain.Events.Events;

namespace Interview.Domain.Events;

public interface IRoomEventDispatcher
{
    Task WriteAsync(IRoomEvent @event, CancellationToken cancellationToken = default);
}
