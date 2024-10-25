namespace Interview.Infrastructure.WebSocket.PubSub.Factory;

public interface IEventBusPublisherFactory
{
    Task<IEventBusPublisher> CreateAsync(CancellationToken cancellationToken);
}
