using Interview.Domain.Tags.Records.Response;

namespace Interview.Domain.Roadmaps.RoadmapById;

public class RoadmapResponse
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required int Order { get; set; }

    public required string? ImageBase64 { get; set; }

    public required string? Description { get; set; }

    public required List<TagItem> Tags { get; set; }

    public required List<RoadmapItemResponse> Items { get; set; }
}
