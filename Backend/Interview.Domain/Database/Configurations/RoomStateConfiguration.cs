using Interview.Domain.Rooms;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoomStateConfiguration : EntityTypeConfigurationBase<RoomState>
{
    protected override void ConfigureCore(EntityTypeBuilder<RoomState> builder)
    {
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Payload).IsRequired();
        builder.HasOne(e => e.Room)
            .WithMany(e => e.RoomStates)
            .IsRequired();
        builder.HasIndex(e => new { e.RoomId, e.Type }).IsUnique();
    }
}
