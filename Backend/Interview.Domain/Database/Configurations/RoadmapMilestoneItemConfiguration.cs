using Interview.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoadmapMilestoneItemConfiguration : EntityTypeConfigurationBase<RoadmapMilestoneItem>
{
    protected override void ConfigureCore(EntityTypeBuilder<RoadmapMilestoneItem> builder)
    {
        builder.Property(e => e.Order).IsRequired();
        builder.HasOne(e => e.RoadmapMilestone).WithMany(e => e.Items).HasForeignKey(e => e.RoadmapMilestoneId);
        builder.HasOne(e => e.QuestionTree).WithMany().HasForeignKey(e => e.QuestionTreeId);
    }
}
