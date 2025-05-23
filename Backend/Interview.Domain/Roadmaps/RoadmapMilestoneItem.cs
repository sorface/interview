using Interview.Domain.Questions;
using Interview.Domain.Repository;

namespace Interview.Domain.Roadmaps;

public class RoadmapMilestoneItem : Entity
{
    public required Guid RoadmapMilestoneId { get; set; }

    public required Guid QuestionTreeId { get; set; }

    public required int Order { get; set; }

    public RoadmapMilestone? RoadmapMilestone { get; set; }

    public QuestionTree? QuestionTree { get; set; }
}
