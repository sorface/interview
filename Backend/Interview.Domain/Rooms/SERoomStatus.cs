using Ardalis.SmartEnum;

namespace Interview.Domain.Rooms;

public class SERoomStatus(string name, char value, EVRoomStatus enumValue) : SmartEnum<SERoomStatus, char>(name, value)
{
    public static readonly SERoomStatus New = new SERoomStatus("New", 'N', EVRoomStatus.New);

    public static readonly SERoomStatus Active = new SERoomStatus("Active", 'A', EVRoomStatus.Active);

    public static readonly SERoomStatus Review = new SERoomStatus("Review", 'R', EVRoomStatus.Review);

    public static readonly SERoomStatus Close = new SERoomStatus("Closed", 'C', EVRoomStatus.Close);

    public EVRoomStatus EnumValue { get; } = enumValue;

    public static SERoomStatus FromEnum(EVRoomStatus enumValue)
    {
        foreach (var roomStatus in List)
        {
            if (roomStatus.EnumValue == enumValue)
            {
                return roomStatus;
            }
        }

        throw new ArgumentException($"No room status found for value {enumValue}");
    }
}
