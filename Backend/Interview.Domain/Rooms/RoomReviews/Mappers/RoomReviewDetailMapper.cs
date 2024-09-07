using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomReviews.Records;

namespace Interview.Domain.Rooms.RoomReviews.Mappers;

public abstract class RoomReviewDetailMapper
{
    public static readonly Mapper<RoomReview, RoomReviewDetail> Instance = new(review => new RoomReviewDetail
    {
        Id = review.Id,
        RoomId = review.Participant.RoomId,
        UserId = review.Participant.Id,
        Review = review.Review,
        State = review.State.Name,
    });
}
