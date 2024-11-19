namespace Interview.Domain.PubSub.Factory;

public interface IEventBusSubscriberFactory
{
    Task<IEventBusSubscriber> CreateAsync(CancellationToken cancellationToken);
}
