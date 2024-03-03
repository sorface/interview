using Interview.Domain.Reactions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public class ReactionConfiguration : EntityTypeConfigurationBase<Reaction>
{
    protected override void ConfigureCore(EntityTypeBuilder<Reaction> builder)
    {
        builder.Property(question => question.Type)
            .IsRequired()
            .HasConversion(e => e.Name, e => ReactionType.FromName(e, false));

        builder.HasIndex(e => e.Type);

        var entities = ReactionType.List.Where(e => e != ReactionType.Unknown)
            .Select(rt => new Reaction
            {
                Id = rt.Id,
                Type = rt,
            })
            .ToList();

        foreach (var entity in entities)
        {
            entity.UpdateCreateDate(new DateTime(2023, 08, 01));
        }

        builder.HasData(entities);
    }
}
