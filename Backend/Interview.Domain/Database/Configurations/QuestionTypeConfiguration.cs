using Interview.Domain.Questions;
using Interview.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class QuestionTypeConfiguration : EntityTypeConfigurationBase<Question>
{
    protected override void ConfigureCore(EntityTypeBuilder<Question> builder)
    {
        builder.Property(question => question.Value).IsRequired().HasMaxLength(128);
        builder.Property(question => question.IsArchived).IsRequired().HasDefaultValue(false);
        builder.HasMany<Tag>(e => e.Tags).WithMany();
        builder.Property(question => question.RoomId).IsRequired(false);
        builder.HasOne(question => question.Room).WithMany().HasForeignKey(e => e.RoomId);
    }
}
