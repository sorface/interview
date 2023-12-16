namespace Interview.Domain.Events.Sender;

public interface IEventSender
{
    Task SendAsync(ReadOnlyMemory<byte> @event, CancellationToken cancellationToken);
}
