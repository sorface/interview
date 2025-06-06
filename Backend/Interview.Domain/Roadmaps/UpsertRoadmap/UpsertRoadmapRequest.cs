namespace Interview.Domain.Roadmaps.UpsertRoadmap;

public class UpsertRoadmapRequest
{
    public Guid? Id { get; set; }

    public required string Name { get; set; }

    public required int Order { get; set; }

    public string? ImageBase64 { get; set; }

    public string? Description { get; set; }

    public required HashSet<Guid> Tags { get; set; }

    public required List<UpsertRoadmapItemRequest> Items { get; set; }
}
