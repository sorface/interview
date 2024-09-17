namespace Interview.Domain.Events.EventProvider;

public sealed class EPStorageEventRequest
{
    public required string Type { get; set; }

    public required DateTime? From { get; set; }

    public required DateTime? To { get; set; }
}
