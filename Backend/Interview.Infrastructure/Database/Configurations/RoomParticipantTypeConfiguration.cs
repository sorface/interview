using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class RoomParticipantTypeConfiguration : EntityTypeConfigurationBase<RoomParticipant>
{
    protected override void ConfigureCore(EntityTypeBuilder<RoomParticipant> builder)
    {
        builder.HasOne<User>(participant => participant.User)
            .WithMany()
            .IsRequired();

        builder.HasOne<Room>(participant => participant.Room)
            .WithMany(room => room.Participants)
            .IsRequired();

        builder.Property(romeUser => romeUser.Type)
            .HasConversion(type => type.Name, name => RoomParticipantType.FromName(name, false))
            .HasMaxLength(64)
            .IsRequired();

        builder
            .HasMany(e => e.Permissions)
            .WithOne();
    }
}
