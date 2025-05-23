using Interview.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoadmapConfiguration : EntityTypeConfigurationBase<Roadmap>
{
    protected override void ConfigureCore(EntityTypeBuilder<Roadmap> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(128);
        builder.Property(e => e.Order).IsRequired();
        builder.HasMany(e => e.Tags).WithMany();
    }
}
