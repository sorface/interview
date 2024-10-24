using System.Collections.Concurrent;
using Interview.Infrastructure.WebSocket.PubSub.Events;

namespace Interview.Infrastructure.WebSocket.PubSub.Factory;

public sealed class MemoryPubSubFactory : IPubSubFactory
{
    public Task<IPubSub> CreateAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IPubSub>(new MemoryPubSub());
    }
    
    private sealed class MemoryPubSub : IPubSub
    {
        private readonly ConcurrentDictionary<string, ConcurrentStack<Action<RedisRoomEvent?>>> _mapping = new();
        
        public Task SubscribeAsync(IPubSubKey key, Action<RedisRoomEvent?> callback, CancellationToken cancellationToken)
        {
            var strKey = key.BuildStringKey();
            var list = _mapping.GetOrAdd(strKey, static _ => new ConcurrentStack<Action<RedisRoomEvent?>>());
            list.Push(callback);
            return Task.CompletedTask;
        }

        public Task PublishAsync(IPubSubKey key, RedisRoomEvent roomEvent, CancellationToken cancellationToken)
        {
            var strKey = key.BuildStringKey();
            if (_mapping.TryGetValue(strKey, out var actions))
            {
                foreach (var action in actions)
                {
                    action(roomEvent);
                }
            }
            
            return Task.CompletedTask;
        }
    }
}
