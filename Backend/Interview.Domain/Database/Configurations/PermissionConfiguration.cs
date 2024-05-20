using Interview.Domain.Permissions;
using Interview.Domain.Users.Permissions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class PermissionConfiguration : EntityTypeConfigurationBase<Permission>
{
    protected override void ConfigureCore(EntityTypeBuilder<Permission> builder)
    {
        builder.Property(e => e.Type)
            .HasConversion(permissionType => permissionType.Name, name => SEPermission.FromName(name, false))
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(e => new { e.Type }).IsUnique();

        var permissions = SEPermission.List
            .Where(permission => permission != SEPermission.Unknown)
            .Select(permission => new Permission(permission))
            .ToList();

        foreach (var permission in permissions)
        {
            permission.UpdateCreateDate(new DateTime(2023, 08, 31));
        }

        builder.HasData(permissions);
    }
}
