using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Interview.Domain.Events.Events;
using Interview.Domain.PubSub.Events;
using Interview.Domain.PubSub.Factory;

namespace Interview.Domain.Events;

public class RoomEventDispatcher : IRoomEventDispatcher
{
    private readonly ConcurrentDictionary<Guid, Channel<IRoomEvent>> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly IEventBusPublisherFactory _publisherFactory;

    public RoomEventDispatcher(IEventBusPublisherFactory publisherFactory)
    {
        _publisherFactory = publisherFactory;
    }

    public IEnumerable<IRoomEvent> Read()
    {
        yield break;
        foreach (var value in _queue.Values)
        {
            IRoomEvent? roomEvent = null;
            try
            {
                if (!value.Reader.TryRead(out roomEvent))
                {
                    continue;
                }
            }
            catch (ChannelClosedException)
            {
                // ignore: May occur when competitively accessing the queue
            }

            if (roomEvent is not null)
            {
                yield return roomEvent;
            }
        }
    }

    public async Task WriteDirectlyAsync(IRoomEvent @event, CancellationToken cancellationToken = default)
    {
        return;
        try
        {
            var channel = GetChannel(@event.RoomId);
            await channel.Writer.WriteAsync(@event, cancellationToken);
        }
        catch (ChannelClosedException)
        {
            // ignore: May occur when competitively accessing the queue
        }

        _semaphore.Release();
    }

    public async Task WriteAsync(IRoomEvent @event, CancellationToken cancellationToken = default)
    {
        await WriteDirectlyAsync(@event, cancellationToken);
        var publisher = await _publisherFactory.CreateAsync(cancellationToken);
        var sendAllInRoomEventBusEvent = new SendAllInRoomEventBusEvent
        {
            Event = JsonSerializer.SerializeToUtf8Bytes(@event),
        };
        await publisher.PublishAsync(new EventBusRoomEventKey(@event.RoomId), sendAllInRoomEventBusEvent, cancellationToken);
    }

    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        return _semaphore.WaitAsync(cancellationToken);
    }

    private static Channel<T> CreateBoundedChannel<T>(int capacity = 1024) => Channel.CreateBounded<T>(
        new BoundedChannelOptions(capacity)
        {
            SingleWriter = false,
            SingleReader = false,
            FullMode = BoundedChannelFullMode.DropOldest,
        });

    private Channel<IRoomEvent> GetChannel(Guid roomId)
    {
        return _queue.GetOrAdd(roomId, _ => CreateBoundedChannel<IRoomEvent>());
    }
}
