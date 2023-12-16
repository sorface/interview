namespace Interview.Domain.RoomQuestionReactions.Records.Response;

public class RoomQuestionReactionDetail
{
    public Guid RoomId { get; set; }

    public Guid Reaction { get; set; }

    public Guid Question { get; set; }
}
