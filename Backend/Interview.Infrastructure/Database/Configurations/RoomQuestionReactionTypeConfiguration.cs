using Interview.Domain.Reactions;
using Interview.Domain.Rooms.RoomQuestionReactions;
using Interview.Domain.Rooms.RoomQuestions;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class RoomQuestionReactionTypeConfiguration : EntityTypeConfigurationBase<RoomQuestionReaction>
{
    protected override void ConfigureCore(EntityTypeBuilder<RoomQuestionReaction> builder)
    {
        builder.HasOne<RoomQuestion>(roomQuestionReaction => roomQuestionReaction.RoomQuestion)
            .WithMany()
            .IsRequired();

        builder.HasOne<Reaction>(roomQuestionReaction => roomQuestionReaction.Reaction)
            .WithMany()
            .IsRequired();

        builder.HasOne<User>(roomQuestionReaction => roomQuestionReaction.Sender)
            .WithMany()
            .IsRequired();
    }
}
