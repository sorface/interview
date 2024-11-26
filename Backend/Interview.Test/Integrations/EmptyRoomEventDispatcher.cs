using Interview.Domain.Events;
using Interview.Domain.Events.Events;

namespace Interview.Test.Integrations
{
    public class EmptyRoomEventDispatcher : IRoomEventDispatcher
    {
        public Task WriteAsync(IRoomEvent @event, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
