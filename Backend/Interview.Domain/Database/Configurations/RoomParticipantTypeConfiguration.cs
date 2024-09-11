using Interview.Domain.Rooms;
using Interview.Domain.Rooms.RoomParticipants;
using Interview.Domain.Rooms.RoomReviews;
using Interview.Domain.Users;
using Interview.Domain.Users.Permissions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoomParticipantTypeConfiguration : EntityTypeConfigurationBase<RoomParticipant>
{
    protected override void ConfigureCore(EntityTypeBuilder<RoomParticipant> builder)
    {
        builder.HasOne<User>(participant => participant.User)
            .WithMany(user => user.RoomParticipants)
            .HasForeignKey(participant => participant.UserId)
            .IsRequired();

        builder
            .HasOne<RoomReview>(participant => participant.Review)
            .WithOne(review => review.Participant)
            .HasForeignKey<RoomReview>(review => review.Id);

        builder.HasOne<Room>(participant => participant.Room)
            .WithMany(room => room.Participants)
            .HasForeignKey(participant => participant.RoomId)
            .IsRequired();

        builder.Property(romeUser => romeUser.Type)
            .HasConversion(type => type.Name, name => SERoomParticipantType.FromName(name, false))
            .HasMaxLength(64)
            .IsRequired();

        builder
            .HasMany(participant => participant.Permissions)
            .WithMany(permission => permission.Participants)
            .UsingEntity(
                "RoomParticipantPermission",
                l => l.HasOne(typeof(Permission)).WithMany().HasForeignKey("FK_PERMISSION_ID").HasPrincipalKey(nameof(Permission.Id)),
                r => r.HasOne(typeof(RoomParticipant)).WithMany().HasForeignKey("FK_PARTICIPANT_ID").HasPrincipalKey(nameof(RoomParticipant.Id)),
                j => j.HasKey("FK_PERMISSION_ID", "FK_PARTICIPANT_ID"));
    }
}
