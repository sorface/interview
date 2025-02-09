using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms;

public class SERoomType : SmartEnum<SERoomType, char>
{
    public static readonly SERoomType Standard = new(EVRoomType.Standard, "Standard", 'S');

    public static readonly SERoomType AI = new(EVRoomType.AI, "AI", 'A');

    private SERoomType(EVRoomType enumValue, string name, char value) : base(name, value)
    {
        EnumValue = enumValue;
    }

    public EVRoomType EnumValue { get; }
}

public enum EVRoomType
{
    /// <summary>
    /// Standard.
    /// </summary>
    Standard,

    /// <summary>
    /// AI.
    /// </summary>
    AI,
}
