using Ardalis.SmartEnum;

namespace Interview.Domain.RoomQuestions;

public enum RoomQuestionStateType
{
    /// <summary>
    /// Closed.
    /// </summary>
    Closed,

    /// <summary>
    /// Open.
    /// </summary>
    Open,

    /// <summary>
    /// Active.
    /// </summary>
    Active,
}

public sealed class RoomQuestionState : SmartEnum<RoomQuestionState>
{
    public static readonly RoomQuestionState Closed = new("Closed", (int)RoomQuestionStateType.Closed);
    public static readonly RoomQuestionState Open = new("Open", (int)RoomQuestionStateType.Open);
    public static readonly RoomQuestionState Active = new("Active", (int)RoomQuestionStateType.Active);

    private RoomQuestionState(string name, int value)
        : base(name, value)
    {
    }

    public RoomQuestionStateType EnumValue => (RoomQuestionStateType)Value;
}
