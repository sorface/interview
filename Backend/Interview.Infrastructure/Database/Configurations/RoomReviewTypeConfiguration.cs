using Interview.Domain.RoomReviews;
using Interview.Domain.Rooms;
using Interview.Domain.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interview.Infrastructure.Database.Configurations
{
    public class RoomReviewTypeConfiguration : EntityTypeConfigurationBase<RoomReview>
    {
        protected override void ConfigureCore(EntityTypeBuilder<RoomReview> builder)
        {
            builder.HasOne<User>(review => review.User)
                .WithMany()
                .IsRequired();

            builder.HasOne<Room>(review => review.Room)
                .WithMany()
                .IsRequired();

            builder.Property(review => review.SeRoomReviewState)
                .HasConversion(state => state.Name, name => SERoomReviewState.FromName(name, false))
                .HasMaxLength(10)
                .IsRequired();
        }
    }
}
