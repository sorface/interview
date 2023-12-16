using Interview.Domain.Repository;
using Interview.Domain.RoomReviews.Records;

namespace Interview.Domain.RoomReviews.Mappers
{
    public abstract class RoomReviewDetailMapper
    {
        private RoomReviewDetailMapper()
        {
        }

        public static readonly Mapper<RoomReview, RoomReviewDetail> Instance = new(review => new RoomReviewDetail
        {
            Id = review.Id,
            RoomId = review.Room!.Id,
            UserId = review.User!.Id,
            Review = review.Review,
            State = review.SeRoomReviewState.Name,
        });
    }
}
