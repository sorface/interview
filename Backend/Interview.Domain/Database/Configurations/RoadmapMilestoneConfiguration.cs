using Interview.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoadmapMilestoneConfiguration : EntityTypeConfigurationBase<RoadmapMilestone>
{
    protected override void ConfigureCore(EntityTypeBuilder<RoadmapMilestone> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(128);
        builder.Property(e => e.Order).IsRequired();
        builder.HasOne<Roadmap>().WithMany(e => e.Milestones).HasForeignKey(e => e.RoadmapId);
        builder.HasOne(e => e.ParentRoadmapMilestone).WithMany(e => e.ChildrenMilestones).HasForeignKey(e => e.ParentRoadmapMilestoneId);
    }
}
