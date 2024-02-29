using Interview.Domain.Rooms;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class AvailableRoomPermissionConfiguration : EntityTypeConfigurationBase<AvailableRoomPermission>
{
    protected override void ConfigureCore(EntityTypeBuilder<AvailableRoomPermission> builder)
    {
        builder.Property(e => e.PermissionId);
        builder
            .HasOne(e => e.Permission)
            .WithMany()
            .HasForeignKey(e => e.PermissionId);
    }
}
