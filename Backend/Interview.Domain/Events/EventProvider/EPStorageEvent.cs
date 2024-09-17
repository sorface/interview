namespace Interview.Domain.Events.EventProvider;

public sealed class EPStorageEvent
{
    public required Guid Id { get; set; }

    public required string? Payload { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required Guid? CreatedById { get; set; }
}
