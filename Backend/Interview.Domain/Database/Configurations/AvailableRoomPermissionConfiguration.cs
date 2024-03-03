using Interview.Domain.Permissions;
using Interview.Domain.Rooms;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class AvailableRoomPermissionConfiguration : EntityTypeConfigurationBase<AvailableRoomPermission>
{
    protected override void ConfigureCore(EntityTypeBuilder<AvailableRoomPermission> builder)
    {
        builder.Property(e => e.PermissionId);
        builder
            .HasOne(e => e.Permission)
            .WithMany()
            .HasForeignKey(e => e.PermissionId);

        var availablePermissions = SEAvailableRoomPermission.List
            .Select(e => new AvailableRoomPermission
            {
                Id = e.Value,
                PermissionId = e.Permission.Id,
            })
            .ToArray();
        foreach (var availableRoomPermission in availablePermissions)
        {
            availableRoomPermission.UpdateUpdateDate(new DateTime(2024, 03, 02, 15, 0, 0, DateTimeKind.Utc));
        }

        builder.HasData(availablePermissions);
    }
}
