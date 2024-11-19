using Interview.Domain.PubSub.Events;

namespace Interview.Domain.PubSub;

public interface IEventBus : IEventBusSubscriber, IEventBusPublisher
{
}

public interface IEventBusPublisher
{
    Task PublishAsync<TKey>(TKey key, EventBusEvent roomEventBusEvent, CancellationToken cancellationToken)
        where TKey : IEventBusKey;
}

public interface IEventBusSubscriber
{
    Task<IAsyncDisposable> SubscribeAsync<TKey>(TKey key, Action<TKey, EventBusEvent?> callback, CancellationToken cancellationToken)
        where TKey : IEventBusKey;
}
