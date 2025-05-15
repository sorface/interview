using Interview.Domain.Tags.Records.Response;

namespace Interview.Domain.Roadmaps.RoadmapPage;

public class RoadmapPageResponse
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required int Order { get; set; }

    public required List<TagItem> Tags { get; set; }

    public required List<RoadmapPageItemResponse> Items { get; set; }
}
