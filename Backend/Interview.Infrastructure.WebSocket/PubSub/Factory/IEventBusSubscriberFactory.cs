namespace Interview.Infrastructure.WebSocket.PubSub.Factory;

public interface IEventBusSubscriberFactory
{
    Task<IEventBusSubscriber> CreateAsync(CancellationToken cancellationToken);
}
