namespace Interview.Domain.Rooms.RoomQuestionReactions.Records;

public class RoomQuestionReactionCreateRequest
{
    public Guid ReactionId { get; set; }

    public Guid RoomId { get; set; }

    public string Payload { get; set; } = string.Empty;
}
