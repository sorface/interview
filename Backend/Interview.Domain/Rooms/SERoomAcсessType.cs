using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms;

public class SERoomAcсessType : SmartEnum<SERoomAcсessType>
{
    public static readonly SERoomAcсessType Public =
        new SERoomAcсessType("Public", (int)EVRoomAccessType.Public, EVRoomAccessType.Public);

    public static readonly SERoomAcсessType Private =
        new SERoomAcсessType("Private", (int)EVRoomAccessType.Private, EVRoomAccessType.Private);

    public SERoomAcсessType(string name, int value, EVRoomAccessType enumValue)
        : base(name, value)
    {
        EnumValue = enumValue;
    }

    public EVRoomAccessType EnumValue { get; }
}
