using Interview.Domain.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class QuestionSubjectTreeConfiguration : EntityTypeConfigurationBase<QuestionSubjectTree>
{
    protected override void ConfigureCore(EntityTypeBuilder<QuestionSubjectTree> builder)
    {
        builder.Property(e => e.QuestionId).IsRequired();
        builder.HasOne(e => e.Question)
            .WithMany()
            .HasForeignKey(e => e.QuestionId);
        builder.Property(e => e.Type)
            .HasConversion(e => e.Value, e => SEQuestionSubjectTreeType.FromValue(e))
            .HasComment($"Available values: [{string.Join(", ", SEQuestionSubjectTreeType.List.Select(e => e.Name + ": " + e.Value))}]")
            .IsRequired();
        builder.HasOne(e => e.ParentQuestionSubjectTree)
            .WithMany()
            .HasForeignKey(e => e.ParentQuestionSubjectTreeId);
    }
}
