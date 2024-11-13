using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database;

public static class PropertyBuilderExt
{
    public static PropertyBuilder<TProperty> ConfigureRequiredEnum<TProperty>(this PropertyBuilder<TProperty> self, TProperty defaultValue, int maxLength)
        where TProperty : struct, Enum
    {
        var names = Enum.GetNames(typeof(TProperty));
        var availableValuesStr = string.Join(", ", names);
        return self
            .HasConversion(e => e.ToString(), e => Enum.Parse<TProperty>(e))
            .HasComment($"Available values: [{availableValuesStr}]")
            .IsRequired()
            .HasMaxLength(maxLength)
            .HasDefaultValue(defaultValue);
    }
}
