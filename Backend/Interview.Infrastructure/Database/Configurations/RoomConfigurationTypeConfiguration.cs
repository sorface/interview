using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations;

public class RoomConfigurationTypeConfiguration : EntityTypeConfigurationBase<Domain.RoomConfigurations.RoomConfiguration>
{
    protected override void ConfigureCore(EntityTypeBuilder<Domain.RoomConfigurations.RoomConfiguration> builder)
    {
        builder.Property(e => e.CodeEditorContent);
    }
}
