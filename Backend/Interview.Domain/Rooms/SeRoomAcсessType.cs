using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms;

public class SeRoomAcсessType : SmartEnum<SeRoomAcсessType>
{
    public static readonly SeRoomAcсessType Public =
        new SeRoomAcсessType("Public", (int)EVRoomAcсessType.Public, EVRoomAcсessType.Public);

    public static readonly SeRoomAcсessType Private =
        new SeRoomAcсessType("Private", (int)EVRoomAcсessType.Private, EVRoomAcсessType.Private);

    public SeRoomAcсessType(string name, int value, EVRoomAcсessType enumValue)
        : base(name, value)
    {
        EnumValue = enumValue;
    }

    public EVRoomAcсessType EnumValue { get; }
}
