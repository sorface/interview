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
        private readonly ConcurrentDictionary<string, List<Action<EventBusEvent?>>> _mapping = new();
        
        public Task<IAsyncDisposable> SubscribeAsync(IEventBusKey key, Action<EventBusEvent?> callback, CancellationToken cancellationToken)
        {
            var strKey = key.BuildStringKey();
            var list = _mapping.GetOrAdd(strKey, static _ => new List<Action<EventBusEvent?>>());
            list.Add(callback);
            return Task.FromResult<IAsyncDisposable>(new Unsubscriber(strKey, this, callback));
        }

        public Task PublishAsync(IEventBusKey key, EventBusEvent roomEventBusEvent, CancellationToken cancellationToken)
        {
            var strKey = key.BuildStringKey();
            if (_mapping.TryGetValue(strKey, out var actions))
            {
                foreach (var action in actions)
                {
                    action(roomEventBusEvent);
                }
            }
            
            return Task.CompletedTask;
        }
        
        private sealed class Unsubscriber : IAsyncDisposable
        {
            private readonly string _key;
            private readonly MemoryEventBus _root;
            private readonly Action<EventBusEvent?> _callback;

            public Unsubscriber(string key, MemoryEventBus root, Action<EventBusEvent?> callback)
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
