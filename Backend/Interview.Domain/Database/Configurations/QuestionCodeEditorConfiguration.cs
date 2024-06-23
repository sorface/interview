using Interview.Domain.Questions.CodeEditors;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class QuestionCodeEditorConfiguration : EntityTypeConfigurationBase<QuestionCodeEditor>
{
    protected override void ConfigureCore(EntityTypeBuilder<QuestionCodeEditor> builder)
    {
        builder.Property(e => e.Content)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(e => e.Lang)
            .HasMaxLength(32)
            .IsRequired();
    }
}
