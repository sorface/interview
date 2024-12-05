using System.Collections.Concurrent;
using System.Threading.Channels;
using Interview.Domain.Events.Events;

namespace Interview.Domain.Events;

public class RoomEventDispatcher : IRoomEventDispatcher
{
    private readonly ConcurrentDictionary<Guid, Channel<IRoomEvent>> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(1);

    public IEnumerable<IRoomEvent> Read()
    {
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

    public async Task WriteAsync(IRoomEvent @event, CancellationToken cancellationToken = default)
    {
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

    public Task DropEventsAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        if (!_queue.TryRemove(roomId, out var channel))
        {
            return Task.CompletedTask;
        }

        _semaphore.Release();
        try
        {
            channel.Writer.TryComplete();
        }
        catch
        {
            // ignore
        }

        return Task.CompletedTask;
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
