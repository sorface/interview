using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class RoomConfigurationTypeConfiguration : EntityTypeConfigurationBase<Domain.Rooms.RoomConfigurations.RoomConfiguration>
{
    protected override void ConfigureCore(EntityTypeBuilder<Domain.Rooms.RoomConfigurations.RoomConfiguration> builder)
    {
        builder.Property(e => e.CodeEditorContent);
    }
}
