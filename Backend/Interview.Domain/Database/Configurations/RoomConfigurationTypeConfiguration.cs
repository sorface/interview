using Interview.Domain.Rooms.RoomConfigurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoomConfigurationTypeConfiguration : EntityTypeConfigurationBase<Domain.Rooms.RoomConfigurations.RoomConfiguration>
{
    protected override void ConfigureCore(EntityTypeBuilder<Domain.Rooms.RoomConfigurations.RoomConfiguration> builder)
    {
        builder.Property(e => e.CodeEditorContent);
        builder.Property(e => e.CodeEditorEnabled);
        builder.Property(e => e.CodeEditorChangeSource).ConfigureRequiredEnum(EVRoomCodeEditorChangeSource.User, 12);
    }
}
