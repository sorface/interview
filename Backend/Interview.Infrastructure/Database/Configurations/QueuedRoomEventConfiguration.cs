using Interview.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class QueuedRoomEventConfiguration : EntityTypeConfigurationBase<QueuedRoomEvent>
{
    protected override void ConfigureCore(EntityTypeBuilder<QueuedRoomEvent> builder)
    {
        builder.Property(e => e.RoomId).IsRequired();
        builder.HasIndex(e => e.RoomId).IsUnique();
        builder.HasOne<Room>()
            .WithOne(e => e.QueuedRoomEvent)
            .HasForeignKey<QueuedRoomEvent>(e => e.RoomId)
            .IsRequired();
    }
}
