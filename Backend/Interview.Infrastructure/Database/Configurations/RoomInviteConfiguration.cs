using Interview.Domain.RoomInvites;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class RoomInviteConfiguration : EntityTypeConfigurationBase<RoomInvite>
{
    protected override void ConfigureCore(EntityTypeBuilder<RoomInvite> builder)
    {
        throw new NotImplementedException();
    }
}
