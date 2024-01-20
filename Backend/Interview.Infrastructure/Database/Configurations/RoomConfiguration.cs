using Interview.Domain.Rooms;
using Interview.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class RoomConfiguration : EntityTypeConfigurationBase<Room>
{
    protected override void ConfigureCore(EntityTypeBuilder<Room> builder)
    {
        builder.Property(room => room.Name).IsRequired().HasMaxLength(70);
        builder.Property(room => room.TwitchChannel).IsRequired().HasMaxLength(100);
        builder.Property(room => room.Status)
            .HasConversion(e => e.Value, e => SERoomStatus.FromValue(e))
            .IsRequired()
            .HasDefaultValue(SERoomStatus.New);
        builder
            .HasOne<Domain.RoomConfigurations.RoomConfiguration>(room => room.Configuration)
            .WithOne(e => e.Room)
            .HasForeignKey<Domain.RoomConfigurations.RoomConfiguration>(e => e.Id);
        builder.HasMany<Tag>(e => e.Tags).WithMany();
        builder.Property(room => room.AcсessType)
            .HasConversion(e => e.Value, e => SeRoomAcсessType.FromValue(e))
            .IsRequired()
            .HasDefaultValue(SeRoomAcсessType.Public);
    }
}
