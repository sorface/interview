using Interview.Domain.Rooms;
using Interview.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoomConfiguration : EntityTypeConfigurationBase<Room>
{
    protected override void ConfigureCore(EntityTypeBuilder<Room> builder)
    {
        builder.Property(room => room.Name).IsRequired().HasMaxLength(70);
        builder.Property(room => room.Status)
            .HasConversion(e => e.Value, e => SERoomStatus.FromValue(e))
            .IsRequired()
            .HasDefaultValue(SERoomStatus.New);
        builder
            .HasOne<Domain.Rooms.RoomConfigurations.RoomConfiguration>(room => room.Configuration)
            .WithOne(e => e.Room)
            .HasForeignKey<Domain.Rooms.RoomConfigurations.RoomConfiguration>(e => e.Id);
        builder.HasMany<Tag>(e => e.Tags).WithMany();
        builder.Property(room => room.AccessType)
            .HasConversion(e => e.Value, e => SERoomAccessType.FromValue(e))
            .IsRequired()
            .HasDefaultValue(SERoomAccessType.Public);
        builder.HasOne(e => e.Category)
            .WithMany(e => e.Rooms)
            .HasForeignKey(e => e.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
