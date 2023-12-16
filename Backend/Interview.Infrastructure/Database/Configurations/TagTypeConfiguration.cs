using Interview.Domain.Questions;
using Interview.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class TagTypeConfiguration : EntityTypeConfigurationBase<Tag>
{
    protected override void ConfigureCore(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(question => question.Value)
            .IsRequired()
            .HasMaxLength(128);
        builder.HasIndex(e => e.Value).IsUnique();
    }
}
