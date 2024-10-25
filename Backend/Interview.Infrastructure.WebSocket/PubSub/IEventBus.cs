using Interview.Infrastructure.WebSocket.PubSub.Events;

namespace Interview.Infrastructure.WebSocket.PubSub;

public interface IEventBus : IEventBusSubscriber, IEventBusPublisher
{
}

public interface IEventBusPublisher
{
    Task PublishAsync(IEventBusKey key, EventBusEvent roomEventBusEvent, CancellationToken cancellationToken);
}

public interface IEventBusSubscriber
{
    Task<IAsyncDisposable> SubscribeAsync(IEventBusKey key, Action<EventBusEvent?> callback, CancellationToken cancellationToken);
}
