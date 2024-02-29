namespace Interview.Domain.Reactions.Records;

public class ReactionDetail
{
    public Guid Id { get; set; }

    public ReactionType Type { get; set; } = ReactionType.Unknown;
}