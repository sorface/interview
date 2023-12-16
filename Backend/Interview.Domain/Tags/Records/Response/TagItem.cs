namespace Interview.Domain.Tags.Records.Response;

public class TagItem
{
    public required Guid Id { get; init; }

    public required string Value { get; init; }

    public required string HexValue { get; init; }
}
