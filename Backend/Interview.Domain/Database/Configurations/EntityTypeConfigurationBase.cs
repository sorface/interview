using System.Linq.Expressions;
using Interview.Domain.Repository;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Domain.Database.Configurations;

public abstract class EntityTypeConfigurationBase<T> : IEntityTypeConfiguration<T>
    where T : Entity
{
    protected virtual Expression<Func<User, IEnumerable<T>?>>? CreatedByNavigation => null;

    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.HasOne(entity => entity.CreatedBy)
            .WithMany(CreatedByNavigation)
            .HasForeignKey(entity => entity.CreatedById);

        ConfigureCore(builder);
    }

    protected abstract void ConfigureCore(EntityTypeBuilder<T> builder);
}
