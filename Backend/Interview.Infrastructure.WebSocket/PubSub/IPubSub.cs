using Interview.Infrastructure.WebSocket.PubSub.Events;

namespace Interview.Infrastructure.WebSocket.PubSub;

public interface IPubSub
{
    Task<IAsyncDisposable> SubscribeAsync(IPubSubKey key, Action<RedisRoomEvent?> callback, CancellationToken cancellationToken);
    Task PublishAsync(IPubSubKey key, RedisRoomEvent roomEvent, CancellationToken cancellationToken);
}
