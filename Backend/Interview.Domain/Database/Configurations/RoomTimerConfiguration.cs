using Interview.Domain.Rooms.RoomTimers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class RoomTimerConfiguration : EntityTypeConfigurationBase<RoomTimer>
{
    protected override void ConfigureCore(EntityTypeBuilder<RoomTimer> builder)
    {
        builder.ToTable("RoomTimer");
        builder.Property(property => property.Duration)
            .HasConversion(e => e.TotalSeconds, value => TimeSpan.FromSeconds(value));
        builder.Property(property => property.ActualStartTime);
    }
}
