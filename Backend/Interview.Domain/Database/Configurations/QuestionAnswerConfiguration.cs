using Interview.Domain.Questions.QuestionAnswers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class QuestionAnswerConfiguration : EntityTypeConfigurationBase<QuestionAnswer>
{
    protected override void ConfigureCore(EntityTypeBuilder<QuestionAnswer> builder)
    {
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.Content)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(e => e.CodeEditor)
            .IsRequired();

        builder.HasOne(e => e.Question)
            .WithMany(e => e.Answers)
            .HasForeignKey(e => e.QuestionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
