using Interview.Domain.Events;
using Interview.Domain.Rooms.RoomParticipants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class AppEventConfiguration : EntityTypeConfigurationBase<AppEvent>
{
    protected override void ConfigureCore(EntityTypeBuilder<AppEvent> builder)
    {
        builder.Property(e => e.Type).IsRequired().HasMaxLength(128);
        builder.Property(e => e.Stateful).IsRequired().HasDefaultValue(false);
        builder.HasMany(e => e.Roles).WithMany();
        builder.HasIndex(e => e.Type).IsUnique();

        var participantTypesComparer = new ValueComparer<List<SERoomParticipantType>>(
            (l, r) => l != null && r != null && l.SequenceEqual(r),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Value)),
            c => c.ToList());
        builder.Property(e => e.ParticipantTypes)
            .HasConversion(
                e => e == null ? null : string.Join(",", e.Select(rp => rp.Name)),
                e => e == null
                    ? null
                    : e.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => SERoomParticipantType.FromName(e, true)).ToList())
            .Metadata
            .SetValueComparer(participantTypesComparer);
    }
}
