using System.Collections.Concurrent;
using Interview.Infrastructure.WebSocket.PubSub.Events;

namespace Interview.Infrastructure.WebSocket.PubSub.Factory;

public sealed class MemoryEventBusFactory : IEventBusPublisherFactory, IEventBusSubscriberFactory
{
    Task<IEventBusPublisher> IEventBusPublisherFactory.CreateAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IEventBusPublisher>(new MemoryEventBus());
    }

    Task<IEventBusSubscriber> IEventBusSubscriberFactory.CreateAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IEventBusSubscriber>(new MemoryEventBus());
    }
    
    private sealed class MemoryEventBus : IEventBus
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<object, Action<IEventBusKey, EventBusEvent?>>> _mapping = new();
        
        public Task<IAsyncDisposable> SubscribeAsync<TKey>(TKey key, Action<TKey, EventBusEvent?> callback, CancellationToken cancellationToken)
            where TKey : IEventBusKey
        {
            var strKey = key.BuildStringKey();
            var list = _mapping.GetOrAdd(strKey, static _ => new ConcurrentDictionary<object, Action<IEventBusKey, EventBusEvent?>>());
            list.TryAdd(callback, (k, ev) => callback((TKey)k, ev));
            return Task.FromResult<IAsyncDisposable>(new Unsubscriber(strKey, this, callback));
        }

        public Task PublishAsync<TKey>(TKey key, EventBusEvent roomEventBusEvent, CancellationToken cancellationToken)
            where TKey : IEventBusKey
        {
            var strKey = key.BuildStringKey();
            if (_mapping.TryGetValue(strKey, out var actions))
            {
                foreach (var action in actions.Values)
                {
                    action(key, roomEventBusEvent);
                }
            }
            
            return Task.CompletedTask;
        }
        
        private sealed class Unsubscriber : IAsyncDisposable
        {
            private readonly string _key;
            private readonly MemoryEventBus _root;
            private readonly object _callback;

            public Unsubscriber(string key, MemoryEventBus root, object callback)
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

                items.TryRemove(_callback, out _);
                return ValueTask.CompletedTask;
            }
        }
    }
}
