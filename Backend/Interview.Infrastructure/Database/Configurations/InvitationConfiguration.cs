using Interview.Domain.Invitations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public sealed class InvitationConfiguration : EntityTypeConfigurationBase<Invitation>
{
    protected override void ConfigureCore(EntityTypeBuilder<Invitation> builder)
    {
        builder.Property(e => e.Hash).IsRequired().HasMaxLength(25);
    }
}
