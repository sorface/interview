namespace Interview.Domain.Roadmaps.RoadmapById;

public class RoadmapItemResponse
{
    public required Guid? Id { get; set; }

    public required EVRoadmapItemType Type { get; set; }

    public required string? Name { get; set; }

    public required Guid? QuestionTreeId { get; set; }

    public required Guid? RoomId { get; set; }

    public required int Order { get; set; }
}
