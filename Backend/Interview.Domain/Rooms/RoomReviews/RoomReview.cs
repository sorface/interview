using Interview.Domain.Repository;
using Interview.Domain.Rooms.RoomParticipants;

namespace Interview.Domain.Rooms.RoomReviews;

public class RoomReview : Entity
{
    public RoomReview(RoomParticipant participant)
        : this(participant, SERoomReviewState.Open)
    {
    }

    public RoomReview(RoomParticipant participant, SERoomReviewState state)
    {
        Participant = participant;
        State = state;
    }

    public RoomParticipant Participant { get; private set; }

    public string Review { get; set; } = string.Empty;

    public SERoomReviewState State { get; set; } = SERoomReviewState.Open;
}
