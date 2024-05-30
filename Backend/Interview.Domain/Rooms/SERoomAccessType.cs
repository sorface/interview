using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms;

public class SERoomAccessType : SmartEnum<SERoomAccessType>
{
    public static readonly SERoomAccessType Public =
        new SERoomAccessType("Public", (int)EVRoomAccessType.Public, EVRoomAccessType.Public);

    public static readonly SERoomAccessType Private =
        new SERoomAccessType("Private", (int)EVRoomAccessType.Private, EVRoomAccessType.Private);

    public SERoomAccessType(string name, int value, EVRoomAccessType enumValue)
        : base(name, value)
    {
        EnumValue = enumValue;
    }

    public EVRoomAccessType EnumValue { get; }
}
