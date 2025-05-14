using Interview.Domain.Tags.Records.Response;

namespace Interview.Domain.Roadmaps.RoadmapPage;

public class RoadmapPageResponse
{
    public required string Name { get; set; }

    public required int Order { get; set; }

    public required List<TagItem> Tags { get; set; }

    public required List<RoadmapPageItemResponse> Items { get; set; }
}

public class RoadmapPageItemResponse
{
    public required EVRoadmapItemType Type { get; set; }

    public required string? Name { get; set; }

    public required Guid? QuestionTreeId { get; set; }

    public required Guid? RoomId { get; set; }

    public required int Order { get; set; }
}

public enum EVRoadmapItemType
{
    /// <summary>
    /// Milestone.
    /// </summary>
    Milestone,

    /// <summary>
    /// Question tree.
    /// </summary>
    QuestionTree,
}
