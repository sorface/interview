using Interview.Domain.Questions;
using Interview.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class QuestionConfiguration : EntityTypeConfigurationBase<Question>
{
    protected override void ConfigureCore(EntityTypeBuilder<Question> builder)
    {
        builder.Property(question => question.Value).IsRequired().HasMaxLength(128);
        builder.Property(question => question.IsArchived).IsRequired().HasDefaultValue(false);
        builder.HasMany<Tag>(e => e.Tags).WithMany();
        builder.Property(e => e.Type)
            .HasConversion(e => e.Value, e => SEQuestionType.FromValue(e))
            .HasComment($"Available values: [{string.Join(", ", SEQuestionType.List.Select(e => e.Name + ": " + e.Value))}]")
            .IsRequired()
            .HasDefaultValue(SEQuestionType.Public);
        builder.HasOne(e => e.Category)
            .WithMany(e => e.Questions)
            .HasForeignKey(e => e.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
