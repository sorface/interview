using Interview.Domain.Events;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class DbRoomEventConfiguration : EntityTypeConfigurationBase<DbRoomEvent>
{
    protected override void ConfigureCore(EntityTypeBuilder<DbRoomEvent> builder)
    {
        builder.HasIndex(e => new { e.RoomId, e.Type });
        builder.Property(e => e.RoomId).IsRequired();
        builder.Property(e => e.EventSenderId);
        builder.Property(e => e.Stateful).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.HasOne(e => e.Room)
            .WithMany()
            .HasForeignKey(e => e.RoomId)
            .IsRequired();
        builder.HasOne(e => e.EventSender)
            .WithMany()
            .HasForeignKey(e => e.EventSenderId);
    }
}
