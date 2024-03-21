using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public sealed class UserTypeConfiguration : EntityTypeConfigurationBase<User>
{
    protected override void ConfigureCore(EntityTypeBuilder<User> builder)
    {
        builder.Property(e => e.Nickname).IsRequired().HasMaxLength(128);
        builder.Property(e => e.Avatar).HasMaxLength(250);
        builder.HasMany(user => user.Roles).WithMany();
        builder.HasMany(user => user.Permissions).WithMany();
    }
}
