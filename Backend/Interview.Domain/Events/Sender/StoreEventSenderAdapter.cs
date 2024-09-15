using Interview.Domain.Events.Storage;

namespace Interview.Domain.Events.Sender;

public class StoreEventSenderAdapter : IEventSenderAdapter
{
    private readonly IEventSenderAdapter _root;
    private readonly IHotEventStorage _hotEventStorage;

    public StoreEventSenderAdapter(IEventSenderAdapter root, IHotEventStorage hotEventStorage)
    {
        _root = root;
        _hotEventStorage = hotEventStorage;
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

        await _hotEventStorage.AddAsync(storageEvent, cancellationToken);
    }
}
