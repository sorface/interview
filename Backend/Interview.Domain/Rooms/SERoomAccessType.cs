using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms;

public class SERoomAccessType(string name, int value, EVRoomAccessType enumValue) : SmartEnum<SERoomAccessType>(name, value)
{
    public static readonly SERoomAccessType Public =
        new SERoomAccessType("Public", (int)EVRoomAccessType.Public, EVRoomAccessType.Public);

    public static readonly SERoomAccessType Private =
        new SERoomAccessType("Private", (int)EVRoomAccessType.Private, EVRoomAccessType.Private);

    public EVRoomAccessType EnumValue { get; } = enumValue;
}
