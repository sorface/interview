using Interview.Domain.Rooms.RoomInvites;
using Interview.Domain.Rooms.RoomParticipants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

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
            .HasConversion(type => type!.Name, name => SERoomParticipantType.FromName(name, false))
            .HasDefaultValue(SERoomParticipantType.Viewer)
            .HasMaxLength(10)
            .IsRequired();
    }
}
