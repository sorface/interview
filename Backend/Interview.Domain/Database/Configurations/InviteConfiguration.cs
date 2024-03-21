using Interview.Domain.Invites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public sealed class InviteConfiguration : EntityTypeConfigurationBase<Invite>
{
    protected override void ConfigureCore(EntityTypeBuilder<Invite> builder)
    {
        builder.Property(e => e.UsesMax)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(e => e.UsesCurrent)
            .IsRequired()
            .HasDefaultValue(0);
    }
}
