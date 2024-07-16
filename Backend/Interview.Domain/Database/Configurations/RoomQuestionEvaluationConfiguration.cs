using System.Linq.Expressions;
using Interview.Domain.Rooms.RoomQuestionEvaluations;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoomQuestionEvaluationConfiguration : EntityTypeConfigurationBase<RoomQuestionEvaluation>
{
    protected override Expression<Func<User, IEnumerable<RoomQuestionEvaluation>?>>? CreatedByNavigation => user => user.RoomQuestionEvaluations;

    protected override void ConfigureCore(EntityTypeBuilder<RoomQuestionEvaluation> builder)
    {
        builder.ToTable("RoomQuestionEvaluation");

        builder.HasAlternateKey(e => new { e.RoomQuestionId, e.CreatedById });
        builder.Property(e => e.Review);
        builder.Property(e => e.Mark);
        builder.Property(e => e.State)
            .HasConversion(state => state!.Name, name => SERoomQuestionEvaluationState.FromName(name, false))
            .HasMaxLength(10)
            .IsRequired();
        builder.HasOne<RoomQuestion>(evaluation => evaluation.RoomQuestion)
            .WithMany(roomQuestion => roomQuestion.Evaluations)
            .HasForeignKey(e => e.RoomQuestionId)
            .IsRequired();
    }
}
