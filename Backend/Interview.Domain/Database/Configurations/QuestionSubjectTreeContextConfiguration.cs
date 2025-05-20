using Interview.Domain.Questions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

/// <summary>
/// QuestionSubjectTreeContext configuration.
/// </summary>
public class QuestionSubjectTreeContextConfiguration : EntityTypeConfigurationBase<QuestionSubjectTreeContext>
{
    protected override void ConfigureCore(EntityTypeBuilder<QuestionSubjectTreeContext> builder)
    {
        builder.Property(e => e.Context).IsRequired();
    }
}
