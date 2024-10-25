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
        private readonly ConcurrentDictionary<string, List<Action<RedisRoomEvent?>>> _mapping = new();
        
        public Task<IAsyncDisposable> SubscribeAsync(IPubSubKey key, Action<RedisRoomEvent?> callback, CancellationToken cancellationToken)
        {
            var strKey = key.BuildStringKey();
            var list = _mapping.GetOrAdd(strKey, static _ => new List<Action<RedisRoomEvent?>>());
            list.Add(callback);
            return Task.FromResult<IAsyncDisposable>(new Unsubscriber(strKey, this, callback));
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
        
        private sealed class Unsubscriber : IAsyncDisposable
        {
            private readonly string _key;
            private readonly MemoryPubSub _root;
            private readonly Action<RedisRoomEvent?> _callback;

            public Unsubscriber(string key, MemoryPubSub root, Action<RedisRoomEvent?> callback)
            {
                _key = key;
                _root = root;
                _callback = callback;
            }

            public ValueTask DisposeAsync()
            {
                if (!_root._mapping.TryGetValue(_key, out var items))
                {
                    return ValueTask.CompletedTask;
                }

                items.Remove(_callback);
                return ValueTask.CompletedTask;
            }
        }
    }
}
