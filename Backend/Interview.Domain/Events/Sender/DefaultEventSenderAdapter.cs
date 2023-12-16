namespace Interview.Domain.Events.Sender;

public sealed class DefaultEventSenderAdapter : IEventSenderAdapter
{
    public Task SendAsync(
        IRoomEventProvider provider,
        IEventSender sender,
        CancellationToken cancellationToken)
    {
        var bytes = provider.ToBytes();
        return sender.SendAsync(bytes, cancellationToken);
    }
}
