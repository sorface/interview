using System.Text.Json.Serialization;
using Interview.Domain.Events.Events;

namespace Interview.Domain.Events.DatabaseProcessors.Records.Room;

public class RoomQuestionReactionEvent : RoomEvent<RoomQuestionReactionPayload>
{
}

public sealed class RoomQuestionReactionPayload
{
    public required Guid UserId { get; init; }

    public required string? Payload { get; init; }
}
