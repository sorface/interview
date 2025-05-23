namespace Interview.Domain.Roadmaps.UpsertRoadmap;

public class UpsertRoadmapItemRequest
{
    public Guid? Id { get; set; }

    public required EVRoadmapItemType Type { get; set; }

    public string? Name { get; set; }

    public Guid? QuestionTreeId { get; set; }

    public required int Order { get; set; }
}
