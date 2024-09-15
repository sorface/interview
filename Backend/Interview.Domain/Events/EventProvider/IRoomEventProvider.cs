namespace Interview.Domain.Events.EventProvider;

public interface IRoomEventProvider
{
    Task<IEnumerable<EPStorageEvent>> GetEventsAsync(EPStorageEventRequest request, CancellationToken cancellationToken);

    Task<EPStorageEvent?> GetLatestEventAsync(EPStorageEventRequest request, CancellationToken cancellationToken);
}
