using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.Sender;

public interface IRoomEventProvider
{
    public IRoomEvent Event { get; }

    public ReadOnlyMemory<byte> ToBytes();
}
