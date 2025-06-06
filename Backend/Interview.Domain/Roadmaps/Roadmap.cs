using Interview.Domain.Repository;
using Interview.Domain.Tags;

namespace Interview.Domain.Roadmaps;

public class Roadmap : ArchiveEntity
{
    public required string Name { get; set; }

    public required int Order { get; set; }

    public string? ImageBase64 { get; set; }

    public string? Description { get; set; }

    public List<Tag> Tags { get; set; } = new List<Tag>();

    public List<RoadmapMilestone> Milestones { get; set; } = new List<RoadmapMilestone>();
}
