namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public sealed class RoomQuestionReactionPayload
{
    public Guid UserId { get; }

    public string? Payload { get; }

    public RoomQuestionReactionPayload(Guid userId, string? payload)
    {
        UserId = userId;
        Payload = payload;
    }
}
