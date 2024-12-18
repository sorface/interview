using System.Text;
using Interview.Domain.Events.Events;
using Interview.Domain.Events.Events.Serializers;

namespace Interview.Domain.Events.Sender;

public sealed class CachedRoomEventProvider : IRoomEventProvider
{
    private readonly Lazy<ReadOnlyMemory<byte>> _lazy;

    public CachedRoomEventProvider(IRoomEvent @event, IEventSerializer eventSerializer)
    {
        Event = @event;
        _lazy = new Lazy<ReadOnlyMemory<byte>>(() =>
        {
            var eventAsString = eventSerializer.SerializeAsString(Event);
            return Encoding.UTF8.GetBytes(eventAsString);
        });
    }

    public IRoomEvent Event { get; }

    public ReadOnlyMemory<byte> ToBytes() => _lazy.Value;
}
