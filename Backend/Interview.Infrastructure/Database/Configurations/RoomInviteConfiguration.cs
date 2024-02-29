using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class RoomInviteConfiguration : EntityTypeConfigurationBase<RoomInvite>
{
    protected override void ConfigureCore(EntityTypeBuilder<RoomInvite> builder)
    {
        builder.HasOne(entity => entity.Invite)
            .WithMany()
            .HasForeignKey(entity => entity.InviteById);
        builder.HasOne(entity => entity.Room)
            .WithMany()
            .HasForeignKey(entity => entity.RoomById);
        builder.Property(entity => entity.ParticipantType)
            .HasConversion(type => type!.Name, name => RoomParticipantType.FromName(name, false))
            .HasDefaultValue(RoomParticipantType.Viewer)
            .HasMaxLength(10)
            .IsRequired();
    }
}
