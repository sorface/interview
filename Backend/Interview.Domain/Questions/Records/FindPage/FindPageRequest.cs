namespace Interview.Domain.Questions.Records.FindPage;

public class FindPageRequest
{
    public HashSet<Guid>? Tags { get; set; }

    public string? Value { get; set; }

    public Guid? CategoryId { get; set; }

    public PageRequest Page { get; set; } = new PageRequest();
}
