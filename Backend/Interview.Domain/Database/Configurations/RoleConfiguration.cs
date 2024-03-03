using Interview.Domain.Users.Roles;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoleConfiguration : EntityTypeConfigurationBase<Role>
{
    protected override void ConfigureCore(EntityTypeBuilder<Role> builder)
    {
        builder.Property(e => e.Name)
            .HasConversion(roleName => roleName.Name, name => RoleName.FromName(name, false))
            .HasMaxLength(64)
            .IsRequired();

        var roles = new[]
        {
            new Role(RoleName.Admin),
            new Role(RoleName.User),
        };
        foreach (var role in roles)
        {
            role.UpdateCreateDate(new DateTime(2023, 08, 01));
        }

        builder.HasData(roles);
    }
}
