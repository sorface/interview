using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms;

public class SERoomAcсessType : SmartEnum<SERoomAcсessType>
{
    public static readonly SERoomAcсessType Public =
        new SERoomAcсessType("Public", (int)EVRoomAcсessType.Public, EVRoomAcсessType.Public);

    public static readonly SERoomAcсessType Private =
        new SERoomAcсessType("Private", (int)EVRoomAcсessType.Private, EVRoomAcсessType.Private);

    public SERoomAcсessType(string name, int value, EVRoomAcсessType enumValue)
        : base(name, value)
    {
        EnumValue = enumValue;
    }

    public EVRoomAcсessType EnumValue { get; }
}
