using Interview.Domain.Repository;

namespace Interview.Domain.Roadmaps;

public class RoadmapMilestone : Entity
{
    public required string Name { get; set; }

    public required int Order { get; set; }

    public required Guid RoadmapId { get; set; }

    public required Guid? ParentRoadmapMilestoneId { get; set; }

    public RoadmapMilestone? ParentRoadmapMilestone { get; set; }

    public List<RoadmapMilestone> ChildrenMilestones { get; set; } = new List<RoadmapMilestone>();

    public List<RoadmapMilestoneItem> Items { get; set; } = new List<RoadmapMilestoneItem>();
}
