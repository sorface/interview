using Interview.Domain.Events.Storage;

namespace Interview.Domain.Events.Sender;

public class StoreEventSenderAdapter : IEventSenderAdapter
{
    private readonly IEventSenderAdapter _root;
    private readonly IEventStorage _eventStorage;

    public StoreEventSenderAdapter(IEventSenderAdapter root, IEventStorage eventStorage)
    {
        _root = root;
        _eventStorage = eventStorage;
    }

    public async Task SendAsync(
        IRoomEventProvider provider,
        IEventSender sender,
        CancellationToken cancellationToken)
    {
        await _root.SendAsync(provider, sender, cancellationToken);

        var @event = provider.Event;
        var storageEvent = new StorageEvent
        {
            Id = @event.Id,
            RoomId = @event.RoomId,
            Type = @event.Type,
            Payload = @event.BuildStringPayload(),
            Stateful = @event.Stateful,
            CreatedAt = @event.CreatedAt,
            CreatedById = @event.CreatedById,
        };

        await _eventStorage.AddAsync(storageEvent, cancellationToken);
    }
}
