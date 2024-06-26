using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms.RoomQuestionEvaluations;

public class SERoomQuestionEvaluationState : SmartEnum<SERoomQuestionEvaluationState>
{
    public static readonly SERoomQuestionEvaluationState Draft = new("Draft", 0, EVRoomQuestionEvaluationState.Draft);
    public static readonly SERoomQuestionEvaluationState Submitted = new("Submitted", 0, EVRoomQuestionEvaluationState.Submitted);

    public EVRoomQuestionEvaluationState EnumValue { get; }

    private SERoomQuestionEvaluationState(string name, int value, EVRoomQuestionEvaluationState enumValue)
        : base(name, value)
    {
        EnumValue = enumValue;
    }

    public static SERoomQuestionEvaluationState? FromEnum(EVRoomQuestionEvaluationState enumValue)
    {
        return List.FirstOrDefault(it => it.EnumValue == enumValue);
    }
}
