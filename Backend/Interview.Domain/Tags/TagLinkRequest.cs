namespace Interview.Domain.Tags;

public class TagLinkRequest
{
    public Guid TagId { get; set; }

    public string HexColor { get; set; } = string.Empty;
}
