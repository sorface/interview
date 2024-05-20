using Interview.Domain.Questions;
using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomQuestions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoomQuestionTypeConfiguration : EntityTypeConfigurationBase<RoomQuestion>
{
    protected override void ConfigureCore(EntityTypeBuilder<RoomQuestion> builder)
    {
        builder.HasOne<Room>(roomQuestion => roomQuestion.Room)
            .WithMany(room => room.Questions)
            .IsRequired();

        builder.HasOne<Question>(roomQuestion => roomQuestion.Question)
            .WithMany()
            .IsRequired();

        builder.Property(roomQuestion => roomQuestion.State)
            .HasConversion(questionState => questionState!.Name, name => RoomQuestionState.FromName(name, false))
            .HasMaxLength(10)
            .IsRequired();
    }
}
