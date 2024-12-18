using Interview.Domain.Events.Events.Serializers;
using Interview.Domain.Events.Storage;

namespace Interview.Domain.Events.Sender;

public class StoreEventSenderAdapter(IEventSenderAdapter root, IHotEventStorage hotEventStorage, IEventSerializer serializer)
    : IEventSenderAdapter
{
    public async Task SendAsync(
        IRoomEventProvider provider,
        IEventSender sender,
        CancellationToken cancellationToken)
    {
        await root.SendAsync(provider, sender, cancellationToken);

        var @event = provider.Event;
        var storageEvent = new StorageEvent
        {
            Id = @event.Id,
            RoomId = @event.RoomId,
            Type = @event.Type,
            Payload = @event.BuildStringPayload(serializer),
            Stateful = @event.Stateful,
            CreatedAt = @event.CreatedAt,
            CreatedById = @event.CreatedById,
        };

        await hotEventStorage.AddAsync(storageEvent, cancellationToken);
    }
}
