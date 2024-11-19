namespace Interview.Domain.PubSub.Factory;

public interface IEventBusPublisherFactory
{
    Task<IEventBusPublisher> CreateAsync(CancellationToken cancellationToken);
}
