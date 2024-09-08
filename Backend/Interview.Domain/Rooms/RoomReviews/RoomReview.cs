using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Rooms.RoomReviews;

public class RoomReview : Entity
{
    public RoomReview(RoomParticipant participant)
    {
        Participant = participant;
    }

    public RoomReview(RoomParticipant participant, SERoomReviewState state)
    {
        Participant = participant;
        State = state;
    }

    private RoomReview()
    {
    }

    public RoomParticipant Participant;

    public string Review { get; set; } = string.Empty;

    public SERoomReviewState State { get; set; } = SERoomReviewState.Open;
}
