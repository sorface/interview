using Interview.Domain.Questions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class QuestionTreeConfiguration : EntityTypeConfigurationBase<QuestionTree>
{
    protected override void ConfigureCore(EntityTypeBuilder<QuestionTree> builder)
    {
        builder.Property(e => e.Name)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(e => e.Order).IsRequired();
        builder.Property(e => e.RootQuestionSubjectTreeId).IsRequired();
        builder.HasOne(e => e.RootQuestionSubjectTree)
            .WithMany()
            .HasForeignKey(e => e.RootQuestionSubjectTreeId);
        builder.HasOne(e => e.ParentQuestionTree)
            .WithMany()
            .HasForeignKey(e => e.ParentQuestionTreeId);
    }
}
