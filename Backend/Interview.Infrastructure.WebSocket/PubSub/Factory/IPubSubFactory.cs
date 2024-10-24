namespace Interview.Infrastructure.WebSocket.PubSub.Factory;

public interface IPubSubFactory
{
    Task<IPubSub> CreateAsync(CancellationToken cancellationToken);
}
