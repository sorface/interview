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

        var storageEvent = new StorageEvent
        {
            Id = provider.Event.Id,
            RoomId = provider.Event.RoomId,
            Type = provider.Event.Type,
            Payload = provider.Event.BuildStringPayload(),
            Stateful = provider.Event.Stateful,
            CreatedAt = provider.Event.CreatedAt,
        };

        await _eventStorage.AddAsync(storageEvent, cancellationToken);
    }
}
