using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms.RoomReviews;

public class SERoomReviewState : SmartEnum<SERoomReviewState>
{
    public static readonly SERoomReviewState Open = new("Open", EVRoomReviewState.Open);
    public static readonly SERoomReviewState Closed = new("Closed", EVRoomReviewState.Closed);
    public static readonly SERoomReviewState Rejected = new("Rejected", EVRoomReviewState.Rejected);

    public EVRoomReviewState EnumValue { get; }

    private SERoomReviewState(string name, EVRoomReviewState enumValue)
        : base(name, (int)enumValue)
    {
        EnumValue = enumValue;
    }

    public static SERoomReviewState? FromEnum(EVRoomReviewState enumValue)
    {
        return List.FirstOrDefault(it => it.EnumValue == enumValue);
    }
}
