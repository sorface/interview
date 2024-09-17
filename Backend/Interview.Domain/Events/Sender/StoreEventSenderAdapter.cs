using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Storage;

namespace Interview.Domain.Events.Sender;

public class StoreEventSenderAdapter : IEventSenderAdapter
{
    private readonly IEventSenderAdapter _root;
    private readonly IHotEventStorage _hotEventStorage;
    private readonly IRoomEventSerializer _serializer;

    public StoreEventSenderAdapter(IEventSenderAdapter root, IHotEventStorage hotEventStorage, IRoomEventSerializer serializer)
    {
        _root = root;
        _hotEventStorage = hotEventStorage;
        _serializer = serializer;
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
            Payload = @event.BuildStringPayload(_serializer),
            Stateful = @event.Stateful,
            CreatedAt = @event.CreatedAt,
            CreatedById = @event.CreatedById,
        };

        await _hotEventStorage.AddAsync(storageEvent, cancellationToken);
    }
}
