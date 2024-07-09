using Interview.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class CategoryConfiguration : EntityTypeConfigurationBase<Category>
{
    protected override void ConfigureCore(EntityTypeBuilder<Category> builder)
    {
        builder.Property(e => e.Name).IsRequired().HasMaxLength(128);
        builder.Property(question => question.IsArchived).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.ParentId);
        builder.HasOne(e => e.Parent)
            .WithMany()
            .HasForeignKey(e => e.ParentId)
            .IsRequired(false);
    }
}
