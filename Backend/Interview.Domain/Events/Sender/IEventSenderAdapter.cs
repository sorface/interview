namespace Interview.Domain.Events.Sender;

public interface IEventSenderAdapter
{
    Task SendAsync(IRoomEventProvider provider, IEventSender sender, CancellationToken cancellationToken);
}
