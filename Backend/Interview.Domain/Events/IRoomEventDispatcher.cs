using Interview.Domain.Events.Events;

namespace Interview.Domain.Events;

public interface IRoomEventDispatcher
{
    IEnumerable<IRoomEvent> Read();

    /// <summary>
    /// Send the event directly, ignoring the EventBus to notify other instances.
    /// </summary>
    /// <param name="event">Event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task WriteDirectlyAsync(IRoomEvent @event, CancellationToken cancellationToken = default);

    Task WriteAsync(IRoomEvent @event, CancellationToken cancellationToken = default);

    Task WaitAsync(CancellationToken cancellationToken = default);
}
