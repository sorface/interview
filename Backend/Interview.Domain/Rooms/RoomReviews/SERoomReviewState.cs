using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms.RoomReviews;

public class SERoomReviewState : SmartEnum<SERoomReviewState>
{
    public static readonly SERoomReviewState Open = new("Open", 0, EVRoomReviewState.Open);
    public static readonly SERoomReviewState Closed = new("Closed", 1, EVRoomReviewState.Closed);

    public EVRoomReviewState EnumValue { get; }

    private SERoomReviewState(string name, int value, EVRoomReviewState enumValue)
        : base(name, value)
    {
        EnumValue = enumValue;
    }

    public static SERoomReviewState? FromEnum(EVRoomReviewState enumValue)
    {
        return List.FirstOrDefault(it => it.EnumValue == enumValue);
    }
}
