using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms.RoomQuestionEvaluations;

public class SERoomQuestionEvaluationState : SmartEnum<SERoomQuestionEvaluationState>
{
    public static readonly SERoomQuestionEvaluationState Draft = new("Draft", EVRoomQuestionEvaluationState.Draft);
    public static readonly SERoomQuestionEvaluationState Submitted = new("Submitted", EVRoomQuestionEvaluationState.Submitted);
    public static readonly SERoomQuestionEvaluationState Rejected = new("Rejected", EVRoomQuestionEvaluationState.Rejected);

    public EVRoomQuestionEvaluationState EnumValue { get; }

    private SERoomQuestionEvaluationState(string name, EVRoomQuestionEvaluationState enumValue)
        : base(name, (int)enumValue)
    {
        EnumValue = enumValue;
    }

    public static SERoomQuestionEvaluationState? FromValue(EVRoomQuestionEvaluationState enumValue)
    {
        return SERoomQuestionEvaluationState.FromValue((int)enumValue);
    }
}
